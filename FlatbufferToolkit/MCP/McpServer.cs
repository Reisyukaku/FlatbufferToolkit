using FlatbufferToolkit.Utils;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FlatbufferToolkit.MCP
{
    public class McpServerManager
    {
        private static McpServerManager? _instance;
        public static McpServerManager Instance => _instance ??= new McpServerManager();

        private readonly Dictionary<string, StreamWriter> _sessions = new();

        private McpServerManager() { }

        public void Start() => Task.Run(ListenAsync);

        private async Task ListenAsync()
        {
            Logger.Instance.Log(LogLevel.LOG, "Server started on http://localhost:8765/");
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8765/");
            listener.Start();

            while (true)
            {
                var ctx = await listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(ctx));
            }
        }

        private async Task HandleRequest(HttpListenerContext ctx)
        {
            var path = ctx.Request.Url?.AbsolutePath;
            var method = ctx.Request.HttpMethod;

            Logger.Instance.Log(LogLevel.LOG, $"{method} {path}");
            LogHeaders(ctx.Request.Headers, "REQ HEADERS");

            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            if (path == "/mcp" || path == "/sse")
            {
                if (method == "POST") await HandlePost(ctx);
                else if (method == "GET") await HandleSse(ctx);
                else if (method == "DELETE") await HandleDelete(ctx);
                else ctx.Response.StatusCode = 405;
            }
            else if (path == "/message" && method == "POST")
                await HandleMessage(ctx);
            else if (path is "/.well-known/oauth-protected-resource" or "/.well-known/oauth-protected-resource/sse")
                await HandleOAuthDiscovery(ctx);
            else
            {
                Logger.Instance.Log(LogLevel.LOG, $"404 unhandled: {method} {path}");
                ctx.Response.StatusCode = 404;
            }

            try { ctx.Response.Close(); } catch { }
        }

        private async Task HandleOAuthDiscovery(HttpListenerContext ctx)
        {
            Logger.Instance.Log(LogLevel.LOG, "OAuth discovery request — returning empty JSON");
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = 200;
            var bytes = Encoding.UTF8.GetBytes("{}");
            await ctx.Response.OutputStream.WriteAsync(bytes);
        }

        private async Task HandleSse(HttpListenerContext ctx)
        {
            var sessionId = Guid.NewGuid().ToString();
            Logger.Instance.Log(LogLevel.LOG, $"SSE connection opened, sessionId={sessionId}");

            ctx.Response.ContentType = "text/event-stream";
            ctx.Response.Headers.Add("Cache-Control", "no-cache");
            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            var writer = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8, leaveOpen: true);
            _sessions[sessionId] = writer;

            try
            {
                var endpoint = $"http://localhost:8765/message?sessionId={sessionId}";
                Logger.Instance.Log(LogLevel.LOG, $"SSE sending endpoint: {endpoint}");
                await SendSseEvent(writer, $"{{\"type\":\"endpoint\",\"uri\":\"{endpoint}\"}}");

                while (true)
                {
                    await Task.Delay(15000);
                    Logger.Instance.Log(LogLevel.LOG, $"SSE ping sessionId={sessionId}");
                    await writer.WriteAsync(": ping\n\n");
                    await writer.FlushAsync();
                }
            }
            catch (Exception ex) { Logger.Instance.Log(LogLevel.LOG, $"SSE stream closed: {ex.Message}"); }
            finally
            {
                Logger.Instance.Log(LogLevel.LOG, $"SSE session removed: {sessionId}");
                _sessions.Remove(sessionId);
                writer.Dispose();
            }
        }

        private async Task HandleMessage(HttpListenerContext ctx)
        {
            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            ctx.Response.StatusCode = 202;
            ctx.Response.Close();

            var sessionId = ctx.Request.QueryString["sessionId"];
            Logger.Instance.Log(LogLevel.LOG, $"SSE message received, sessionId={sessionId}");

            if (sessionId == null || !_sessions.TryGetValue(sessionId, out var sseWriter) || sseWriter == null)
            {
                Logger.Instance.Log(LogLevel.LOG, $"SSE message dropped — session not found: {sessionId}");
                Logger.Instance.Log(LogLevel.LOG, $"Active sessions: [{string.Join(", ", _sessions.Keys)}]");
                return;
            }

            using var reader = new StreamReader(ctx.Request.InputStream);
            var body = await reader.ReadToEndAsync();
            Logger.Instance.Log(LogLevel.LOG, $"--> (SSE) {body}");

            try
            {
                using var doc = JsonDocument.Parse(body);
                var response = BuildResponse(doc.RootElement);
                Logger.Instance.Log(LogLevel.LOG, $"<-- (SSE) {response}");
                await SendSseEvent(sseWriter, response);
            }
            catch (Exception ex) { Logger.Instance.Log(LogLevel.LOG, $"SSE message error: {ex.Message}"); }
        }

        private static async Task SendSseEvent(StreamWriter writer, string data)
        {
            await writer.WriteAsync($"data: {data}\n\n");
            await writer.FlushAsync();
        }

        private async Task HandlePost(HttpListenerContext ctx)
        {
            using var reader = new StreamReader(ctx.Request.InputStream);
            var body = await reader.ReadToEndAsync();
            Logger.Instance.Log(LogLevel.LOG, $"--> (POST) {body}");

            if (string.IsNullOrWhiteSpace(body))
            {
                Logger.Instance.Log(LogLevel.LOG, "POST body was empty — returning 400");
                ctx.Response.StatusCode = 400;
                return;
            }

            JsonDocument doc;
            try { doc = JsonDocument.Parse(body); }
            catch (Exception ex)
            {
                Logger.Instance.Log(LogLevel.LOG, $"POST JSON parse error: {ex.Message}");
                ctx.Response.StatusCode = 400;
                return;
            }

            using (doc)
            {
                var root = doc.RootElement;

                if (IsNotificationOrResponse(root))
                {
                    Logger.Instance.Log(LogLevel.LOG, "POST is notification/response — returning 202");
                    ctx.Response.StatusCode = 202;
                    return;
                }

                var method = root.TryGetProperty("method", out var m) ? m.GetString() : null;
                Logger.Instance.Log(LogLevel.LOG, $"POST method={method}");

                if (method != "initialize")
                {
                    var sessionId = ctx.Request.Headers["Mcp-Session-Id"];
                    Logger.Instance.Log(LogLevel.LOG, $"POST session check: Mcp-Session-Id={sessionId}, known={_sessions.ContainsKey(sessionId ?? "")}");
                    Logger.Instance.Log(LogLevel.LOG, $"Active sessions: [{string.Join(", ", _sessions.Keys)}]");

                    if (sessionId == null || !_sessions.ContainsKey(sessionId))
                    {
                        Logger.Instance.Log(LogLevel.LOG, "POST rejected — session not found, returning 400");
                        ctx.Response.StatusCode = 400;
                        return;
                    }
                }

                var response = BuildResponse(root);
                Logger.Instance.Log(LogLevel.LOG, $"<-- (POST) {response}");

                if (method == "initialize")
                {
                    var newSessionId = Guid.NewGuid().ToString();
                    _sessions[newSessionId] = null!;
                    ctx.Response.Headers.Add("Mcp-Session-Id", newSessionId);
                    Logger.Instance.Log(LogLevel.LOG, $"New session created: {newSessionId}");
                }

                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = 200;
                var bytes = Encoding.UTF8.GetBytes(response);
                await ctx.Response.OutputStream.WriteAsync(bytes);
            }
        }

        private Task HandleGet(HttpListenerContext ctx)
        {
            Logger.Instance.Log(LogLevel.LOG, "GET /mcp — returning 405");
            ctx.Response.StatusCode = 405;
            return Task.CompletedTask;
        }

        private Task HandleDelete(HttpListenerContext ctx)
        {
            var sessionId = ctx.Request.Headers["Mcp-Session-Id"];
            Logger.Instance.Log(LogLevel.LOG, $"DELETE session: {sessionId}");
            if (sessionId != null)
                _sessions.Remove(sessionId);
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        }

        private static bool IsNotificationOrResponse(JsonElement root)
        {
            var hasMethod = root.TryGetProperty("method", out _);
            var hasId = root.TryGetProperty("id", out _);
            var hasResult = root.TryGetProperty("result", out _);
            var hasError = root.TryGetProperty("error", out _);

            if (!hasMethod && (hasResult || hasError)) return true;
            if (hasMethod && !hasId) return true;
            return false;
        }

        private string BuildResponse(JsonElement root)
        {
            var method = root.TryGetProperty("method", out var m) ? m.GetString() : null;
            var id = root.TryGetProperty("id", out var idEl) ? idEl.Clone() : default;

            object result = method switch
            {
                "initialize" => BuildInitialize(),
                "tools/list" => BuildToolList(),
                "tools/call" => McpToolManager.Instance.CallTool(root.GetProperty("params")),
                _ => new { }
            };

            return JsonSerializer.Serialize(new { jsonrpc = "2.0", id, result });
        }

        private static object BuildInitialize() => new
        {
            protocolVersion = "2025-06-18",
            capabilities = new { tools = new { } },
            serverInfo = new { name = "flatbuffer_toolkit", version = "1.0.0" }
        };

        private static object BuildToolList() => new
        {
            tools = McpToolManager.Tools.Select(t => t.ToSchema())
        };

        private static void LogHeaders(System.Collections.Specialized.NameValueCollection headers, string label)
        {
            foreach (string key in headers)
                Logger.Instance.Log(LogLevel.LOG, $"[MCP] {label}: {key}={headers[key]}");
        }
    }
}