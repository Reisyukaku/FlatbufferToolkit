using System.Text.RegularExpressions;
using Avalonia.Controls;
using FlatbufferToolkit.Utils;

namespace FlatbufferToolkit.UI.Views;

public partial class EditorView : UserControl
{
    private FileAccessor? _file;
    public event EventHandler? StatusChanged;
    public event EventHandler<FileAccessor>? NewBufferRequested;
    public bool IsParsing
    {
        get;
        private set;
    }
    public long SelectionStart => Hex.SelectionStart;
    public long SelectionLength => Hex.SelectionLength;
    public int CurrentLine => SchemaText.TextArea.Caret.Line;
    public string FileName => _file?.Name ?? "Buffer";

    public EditorView()
    {
        InitializeComponent();
        Hex.SelectionChanged += (_, _) =>
        {
            UpdateInspector();
            StatusChanged?.Invoke(this, EventArgs.Empty);
        };
        Hex.BufferChanged += (_, _) => Structure.ItemsSource = null;
        Hex.OpenSelectionRequested += (_, _) =>
        {
            if (SelectionLength > 0)
            {
                NewBufferRequested?.Invoke(this, new FileAccessor($"Buffer_0x{SelectionStart:X}", Hex.SelectedBytes()));
            }
        };
        SchemaText.TextArea.Caret.PositionChanged += (_, _) => StatusChanged?.Invoke(this, EventArgs.Empty);
        UpdateSettings();
    }

    public void LoadFile(FileAccessor file)
    {
        _file = file;
        Hex.Load(file.Bytes);
        string rootName = Regex.Replace(file.Name, "[^a-zA-Z0-9_]", "_");
        if (string.IsNullOrEmpty(rootName) || char.IsDigit(rootName[0]))
        {
            rootName = "Buffer_" + rootName;
        }

        SchemaText.Text = $"table {rootName}\n{{\n\n}}\nroot_type {rootName};\n";
    }

    public async Task ParseAsync(IProgress<ParseProgress>? progress = null)
    {
        if (IsParsing)
        {
            return;
        }

        string text = SchemaText.Text;
        byte[] bytes = GetBufferSnapshot();
        IsParsing = true;
        SchemaText.IsReadOnly = true;
        Hex.IsEnabled = false;
        StatusChanged?.Invoke(this, EventArgs.Empty);
        Structure.ItemsSource = null;
        Hex.SetHighlights([]);
        try
        {
            var result = await Task.Run(() =>
            {
                var schema = new SchemaParser().Parse(text, progress);
                if (schema == null)
                {
                    return (Nodes: (List<InspectionNode>?)null, Highlights: (List<ByteHighlight>?)null);
                }

                using var walk = new FlatBufferBinWalk(bytes, schema, progress);
                return walk.ReadRoot() == null
                    ? (Nodes: (List<InspectionNode>?)null, Highlights: (List<ByteHighlight>?)null)
                    : (Nodes: walk.Nodes, Highlights: walk.Highlights);
            });
            if (result.Nodes != null)
            {
                Structure.ItemsSource = result.Nodes;
                Hex.SetHighlights(result.Highlights!);
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Log(LogLevel.ERROR, ex.Message);
        }
        finally
        {
            IsParsing = false;
            SchemaText.IsReadOnly = false;
            Hex.IsEnabled = true;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public byte[] GetBufferSnapshot()
    {
        return Hex.Buffer.ToArray();
    }

    public string ReadSchema()
    {
        return SchemaText.Text;
    }

    public void WriteSchema(string text)
    {
        if (IsParsing)
        {
            throw new InvalidOperationException("Wait for parsing to finish before replacing the schema.");
        }

        SchemaText.Text = text;
    }

    public string ReadHex(long offset, int length)
    {
        var bytes = Hex.Buffer;
        if (offset < 0 || length < 0 || offset > bytes.LongLength)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        length = (int)Math.Min(length, bytes.LongLength - offset);
        Hex.Select(offset, length);
        return BitConverter.ToString(bytes, (int)offset, length).Replace("-", " ");
    }

    public void UpdateSettings()
    {
        SchemaText.ShowLineNumbers = Settings.Instance.ShowTextNumbers;
        InspectorPanel.IsVisible = Settings.Instance.ShowDataInspector;
    }

    private void StructureSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Structure.SelectedItem is InspectionNode node && node.Offset >= 0 && node.Offset < Hex.Buffer.LongLength)
        {
            Hex.Select(node.Offset, 1);
        }
    }

    private void UpdateInspector()
    {
        var bytes = Hex.Buffer.AsSpan((int)SelectionStart, (int)Math.Min(SelectionLength, 8));
        Inspector.Inspect(bytes);
    }
}
