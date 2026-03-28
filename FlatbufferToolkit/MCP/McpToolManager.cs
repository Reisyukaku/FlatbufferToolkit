

using FlatbufferToolkit.Utils;
using System.Text.Json;

namespace FlatbufferToolkit.MCP
{
    public class McpToolManager
    {
        private static McpToolManager? _instance;
        public static McpToolManager Instance => _instance ??= new McpToolManager();

        public Func<long, int, string>? ReadHexProvider { get; set; }
        public Func<string>? ReadSchemaProvider { get; set; }
        public Action<string>? WriteSchemaProvider { get; set; }

        public static readonly McpTool[] Tools =
        [
            new McpTool("read_hex", "Reads hex data from the currently open binary file")
                .WithParam("offset", "integer")
                .WithParam("length", "integer"),

            new McpTool("read_schema", "Reads the schema fbs text currently in the editor"),

            new McpTool("write_schema", "Writes new text to the schema editor")
                .WithParam("text", "string"),
        ];

        private McpToolManager() { }

        public object CallTool(JsonElement @params)
        {
            var name = @params.GetProperty("name").GetString();
            var args = @params.TryGetProperty("arguments", out var a) ? a : default;
            Logger.Instance.Log(LogLevel.LOG, $"CallTool: {name}");

            try
            {
                var text = name switch
                {
                    "read_hex" when ReadHexProvider != null => ReadHexProvider(args.GetProperty("offset").GetInt64(), args.GetProperty("length").GetInt32()),
                    "read_schema" when ReadSchemaProvider != null => ReadSchemaProvider() ?? "",
                    "write_schema" when WriteSchemaProvider != null => WriteSchemaAndAck(args),
                    _ => throw new InvalidOperationException("Tool not found or editor not ready.")
                };

                Logger.Instance.Log(LogLevel.LOG, $"CallTool result: {text}");
                return new { content = new[] { new { type = "text", text } }, isError = false };
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(LogLevel.LOG, $"CallTool error: {ex.Message}");
                return new { content = new[] { new { type = "text", text = ex.Message } }, isError = true };
            }
        }

        private string WriteSchemaAndAck(JsonElement args)
        {
            WriteSchemaProvider!(args.GetProperty("text").GetString() ?? "");
            return "Schema updated successfully.";
        }
    }

    public class McpTool(string name, string description)
    {
        private readonly List<(string Name, string Type)> _params = [];

        public string Name => name;

        public McpTool WithParam(string paramName, string type)
        {
            _params.Add((paramName, type));
            return this;
        }

        public object ToSchema() => new
        {
            name,
            description,
            inputSchema = new
            {
                type = "object",
                properties = _params.ToDictionary(p => p.Name, p => new { type = p.Type }),
                required = _params.Select(p => p.Name).ToArray()
            }
        };
    }
    }