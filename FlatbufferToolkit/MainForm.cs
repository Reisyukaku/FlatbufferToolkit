using Be.Windows.Forms;
using FlatBuffersParser;
using FlatbufferToolkit.UI.HexView;
using FlatbufferToolkit.UI.IDE;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FlatbufferHelper
{
    public partial class MainForm : Form
    {
        private byte[] fileBytes;
        Dictionary<string, DataGridViewRow> dataInspRowLut = new Dictionary<string, DataGridViewRow>();

        public MainForm()
        {
            InitializeComponent();
            InitDataInspector();
            InitIDE();
        }

        private void ClearForm()
        {
            if (hexView.ByteProvider is IDisposable d)
            {
                d.Dispose();
            }
            hexView.ByteProvider = null;
            treeView.Nodes.Clear();
            schemaText.Text = string.Empty;
            outTxt.Text = string.Empty;
        }

        private void InitDataInspector()
        {
            var AddRow = (string name, object value) =>
            {
                int rowIndex = dataInspectorGrid.Rows.Add(name, value);
                dataInspRowLut[name] = dataInspectorGrid.Rows[rowIndex];
            };
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
            UpdateDataInspectorVisibility();
        }

        private void InitIDE()
        {
            schemaText.InitFbsLexer();
            schemaText.SetupAutoComplete();
        }

        private void UpdateDataInspectorVisibility()
        {
            dataInspectorPanel.Visible = dataInspectorToolStripMenuItem.Checked;
            tableLayoutPanel1.PerformLayout();
        }

        private void UpdateSelectedHex()
        {
            var start = hexView.SelectionStart;
            var length = hexView.SelectionLength;
            hexLbl.Text = string.Format("Hex: 0x{0} | 0x{1} bytes", start.ToString("X"), length.ToString("X"));

            byte[] val = hexView.GetSelectedBytes();
            if (val.Count() <= 0) return;

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

        private void ParseSchema()
        {
            var parser = new SchemaParser();
            var schema = parser.Parse(schemaText.Text);
            if (schema == null) return;

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

            var binread = new FlatBufferBinWalk(ref hexView, ref treeView, fileBytes, schema);
            var fbs = binread.ReadRoot();
            if (fbs == null) return;

            foreach (var root in fbs)
            {
                Trace.WriteLine(root.ToString());
            }
            hexView.Invalidate(true);
        }

        private void LoadFile(string filepath)
        {
            ClearForm();

            fileBytes = File.ReadAllBytes(filepath);
            hexView.ByteProvider = new FileByteProvider(filepath);
            var name = Path.GetFileNameWithoutExtension(filepath);
            CreateTemplateSchema(name);
        }

        private void SaveSchema()
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Flatbuffer Schema|*.fbs|All files|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            File.WriteAllText(sfd.FileName, schemaText.Text);
        }

        private void CreateTemplateSchema(string defaultName)
        {
            const string defaultSchema =
                "table {0}\n" +
                "{{\n" +
                "\n" +
                "}}\n" +
                "root_type {0};";
            schemaText.Text = string.Format(defaultSchema, defaultName);
        }

        #region UI
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            LoadFile(ofd.FileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            treeView.Nodes.Clear();
            hexView.HighlightedRegions.Clear();
            outTxt.Text = string.Empty;
            ParseSchema();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Logger.Initialize(ref outTxt);
        }

        private void saveSchemaAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSchema();
        }

        private void hexView_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateSelectedHex();
        }

        private void dataInspectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateDataInspectorVisibility();
        }
        #endregion
    }
}
