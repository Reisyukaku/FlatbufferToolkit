

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
        public Func<byte[]>? BufferSnapshotProvider { get; set; }

        private static readonly JsonSerializerOptions InspectionJsonOptions = new(JsonSerializerDefaults.Web);

        public static readonly McpTool[] Tools =
        [
            new McpTool("read_hex", "Reads hex data from the currently open binary file")
                .WithParam("offset", "integer")
                .WithParam("length", "integer"),

            new McpTool("read_schema", "Reads the schema fbs text currently in the editor"),

            new McpTool("write_schema", "Writes new text to the schema editor")
                .WithParam("text", "string"),

            new McpTool("inspect_table", "Inspects a FlatBuffers table without a schema. Returns vtable metadata, field slots, absolute addresses, raw bytes, scalar hypotheses and possible reference targets. Omit offset to resolve the root reference at byte 0. Supplied offset must be the table address, not the address of a reference to it. Does not infer field types. Results are paged; pass nextField as start_field to continue.")
                .WithParam("offset", "integer", "Absolute table byte address in the active buffer. Omit to start at root (ordinary, non-size-prefixed FlatBuffers).", required: false)
                .WithParam("start_field", "integer", "First field slot to return; defaults to 0.", required: false)
                .WithParam("count", "integer", "Maximum field slots to return, 1 to 256; defaults to 64.", required: false),
        ];

        private McpToolManager() { }

        public object CallTool(JsonElement @params)
        {
            var name = @params.GetProperty("name").GetString();
            var args = @params.TryGetProperty("arguments", out var a) ? a : default;
            Logger.Instance.Log(LogLevel.LOG, $"CallTool: {name}");

            try
            {
                if (name == "inspect_table")
                {
                    if (args.ValueKind is not (JsonValueKind.Object or JsonValueKind.Undefined))
                        throw new ArgumentException("Tool arguments must be an object.");
                    var snapshot = BufferSnapshotProvider?.Invoke()
                        ?? throw new InvalidOperationException("Editor not ready: no active binary buffer.");
                    var inspection = FlatBufferInspector.InspectTable(snapshot,
                        args.ValueKind == JsonValueKind.Object && args.TryGetProperty("offset", out var offset) ? offset.GetInt64() : null,
                        args.ValueKind == JsonValueKind.Object && args.TryGetProperty("start_field", out var start) ? start.GetInt32() : 0,
                        args.ValueKind == JsonValueKind.Object && args.TryGetProperty("count", out var count) ? count.GetInt32() : 64);
                    var structuredContent = JsonSerializer.SerializeToElement(inspection, InspectionJsonOptions);
                    var inspectionText = structuredContent.GetRawText();
                    Logger.Instance.Log(LogLevel.LOG, $"Inspected table at 0x{inspection.TableOffset:X}: {inspection.Fields.Count}/{inspection.FieldCount} field slots.");
                    return new { content = new[] { new { type = "text", text = inspectionText } }, structuredContent, isError = false };
                }

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
        private readonly List<(string Name, string Type, string Description, bool Required)> _params = [];

        public string Name => name;

        public McpTool WithParam(string paramName, string type, string description = "", bool required = true)
        {
            _params.Add((paramName, type, description, required));
            return this;
        }

        public object ToSchema() => new
        {
            name,
            description,
            inputSchema = new
            {
                type = "object",
                properties = _params.ToDictionary(p => p.Name, p => new { type = p.Type, description = p.Description }),
                required = _params.Where(p => p.Required).Select(p => p.Name).ToArray()
            }
        };
    }
    }
