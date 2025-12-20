using Be.Windows.Forms;
using FlatbufferToolkit.UI;
using FlatbufferToolkit.UI.HexView;
using FlatbufferToolkit.UI.IDE;
using FlatbufferToolkit.Utils;
using ScintillaNET;
using System.Diagnostics;

namespace FlatbufferToolkit;

public partial class MainForm : Form
{
    private byte[] fileBytes { get; set; } = [];
    private Dictionary<string, DataGridViewRow> dataInspRowLut = [];

    public MainForm()
    {
        InitializeComponent();

        InitDataInspector();
        InitIDE();
        Progress.Initialize(progressBar1, progressLbl);
    }

    private void CreateTemplateSchema(string defaultName)
    {
        const string defaultSchema =
            """
            table {0}
            {{

            }}
            root_type {0};
            """;
        schemaText.Text = string.Format(defaultSchema, defaultName);
    }

    private async void ParseSchema()
    {
        UIEnabled(false);

        treeView.Nodes.Clear();
        outTxt.Text = string.Empty;
        hexView.HighlightedRegions.Clear();

        var parser = new SchemaParser();
        var schema = await Task.Run(() => parser.Parse(schemaText));

        if (schema == null) goto end;

        Trace.WriteLine($"Parsed schema with {schema.Structs.Count} tables/structs");
        Trace.WriteLine($"Root type: {schema.RootType}");

        foreach (var structDef in schema.Structs.Values)
        {
            Trace.WriteLine($"\n{(structDef.IsStruct ? "Struct" : "Table")}: {structDef.Name}");
            foreach (var field in structDef.Fields)
            {
                Trace.WriteLine($"  - {field.Name}: {field.Type.BaseType}");
            }
        }

        var binread = new FlatBufferBinWalk(hexView, treeView, fileBytes, schema);
        var fbs = await Task.Run(() =>
        {
            try
            {
                return binread.ReadRoot();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error reading FlatBuffer: {ex.Message}");
                return null;
            }
        });

        if (fbs == null) goto end;

        foreach (var root in fbs)
        {
            Trace.WriteLine(root.ToString());
        }
        hexView.Invalidate(true);

        end:
        UIEnabled(true);
    }

    #region UI_INIT
    private void InitDataInspector()
    {
        void AddRow(string name, object value)
        {
            int rowIndex = dataInspectorGrid.Rows.Add(name, value);
            dataInspRowLut[name] = dataInspectorGrid.Rows[rowIndex];
        }

        AddRow("U8", 0);
        AddRow("S8", 0);
        AddRow("U16", 0);
        AddRow("S16", 0);
        AddRow("U32", 0);
        AddRow("S32", 0);
        AddRow("U64", 0);
        AddRow("S64", 0);
        AddRow("Float", 0.0);
        AddRow("Double", 0.0);
        UpdateDataInspectorSettings();
    }

    private void InitIDE()
    {
        schemaText.InitFbsLexer();
        schemaText.SetupAutoComplete();
        UpdateIDESettings();
    }
    #endregion

    #region UI_UPDATE
    private void UIEnabled(bool b)
    {
        runToolStripMenuItem.Enabled = b;
        hexView.Enabled = b;
        schemaText.Enabled = b;
    }

    private void ResetHexView()
    {
        if (hexView.ByteProvider is IDisposable d)
        {
            d.Dispose();
        }
        hexView.ByteProvider = null;
        hexView.HighlightedRegions.Clear();
    }

    private void UpdateDataInspectorSettings()
    {
        dataInspectorPanel.Visible = dataInspectorToolStripMenuItem.Checked;
        tableLayoutPanel1.PerformLayout();
    }

    private void UpdateIDESettings()
    {
        schemaText.ShowLineNumbers(showLineNumbersToolStripMenuItem.Checked);
    }

    private void UpdateSelectedHex()
    {
        var start = hexView.SelectionStart;
        var length = hexView.SelectionLength;
        hexLbl.Text = $"Hex: 0x{start:X} | 0x{length:X} bytes";

        byte[] val = hexView.GetSelectedBytes();
        if (val.Length == 0) return;

        dataInspRowLut["U8"].Cells[1].Value = (byte)val[0];
        dataInspRowLut["S8"].Cells[1].Value = (sbyte)val[0];
        dataInspRowLut["U16"].Cells[1].Value = BitConverter.ToUInt16(val);
        dataInspRowLut["S16"].Cells[1].Value = BitConverter.ToInt16(val);
        dataInspRowLut["U32"].Cells[1].Value = BitConverter.ToUInt32(val);
        dataInspRowLut["S32"].Cells[1].Value = BitConverter.ToInt32(val);
        dataInspRowLut["U64"].Cells[1].Value = BitConverter.ToUInt64(val);
        dataInspRowLut["S64"].Cells[1].Value = BitConverter.ToInt64(val);
        dataInspRowLut["Float"].Cells[1].Value = BitConverter.ToSingle(val);
        dataInspRowLut["Double"].Cells[1].Value = BitConverter.ToDouble(val);
    }
    #endregion

    #region FILE_HANDLING
    private void LoadFile(string filepath)
    {
        ResetHexView();
        treeView.Nodes.Clear();
        schemaText.Text = string.Empty;
        outTxt.Text = string.Empty;

        fileBytes = File.ReadAllBytes(filepath);
        hexView.ByteProvider = new FileByteProvider(filepath);
        var name = Path.GetFileNameWithoutExtension(filepath);
        CreateTemplateSchema(name);
    }

    private void SaveSchema()
    {
        using var sfd = new SaveFileDialog();
        sfd.Filter = "Flatbuffer Schema|*.fbs|All files|*.*";
        if (sfd.ShowDialog() != DialogResult.OK) return;

        File.WriteAllText(sfd.FileName, schemaText.Text);
    }
    #endregion

    #region UI_CALLBACKS
    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Title = "Open Flatbuffer Binary File";
        ofd.Filter = "All files|*.*";
        if (ofd.ShowDialog() != DialogResult.OK) return;
        LoadFile(ofd.FileName);
    }

    private void MainForm_Load(object sender, EventArgs e) => Logger.Initialize(outTxt);

    private void saveSchemaAsToolStripMenuItem_Click(object sender, EventArgs e) => SaveSchema();

    private void hexView_MouseUp(object sender, MouseEventArgs e) => UpdateSelectedHex();

    private void dataInspectorToolStripMenuItem_Click(object sender, EventArgs e) => UpdateDataInspectorSettings();

    private void showLineNumbersToolStripMenuItem_Click(object sender, EventArgs e) => UpdateIDESettings();

    private void schemaText_UpdateUI(object sender, UpdateUIEventArgs e) => textLbl.Text = $"Text: Line {schemaText.CurrentLine + 1}";

    private void runToolStripMenuItem_Click(object sender, EventArgs e) => ParseSchema();

    #endregion
}