using Be.Windows.Forms;
using FlatbufferToolkit.UI.HexView;
using FlatbufferToolkit.UI.IDE;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;

namespace FlatbufferToolkit.UI
{
    public partial class EditorForm : UserControl
    {
        private FileAccessor _file;
        private Dictionary<string, DataGridViewRow> dataInspRowLut = [];

        public event EventHandler HexSelectionChanged;
        public event EventHandler<FileAccessor> RequestNewFile;

        public EditorForm()
        {
            InitializeComponent();
            InitDataInspector();
            InitIDE();
            InitHexView();
            UpdateSettings();
            Dock = DockStyle.Fill;
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

        public async void ParseSchema()
        {
            UIEnabled(false);

            treeView.Nodes.Clear();
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

            var binread = new FlatBufferBinWalk(hexView, treeView, _file.Bytes, schema);
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

        public int GetCurrentTextLine() => schemaText.CurrentLine;

        public string GetName() => _file.Name;

        public Tuple<long, long> GetSelectedHex()
        {
            var start = hexView.SelectionStart;
            var length = hexView.SelectionLength;
            return Tuple.Create(start, length);
        }

        #region FILE_HANDLING
        public void LoadFile(FileAccessor file)
        {
            _file = file;

            ResetHexView();
            treeView.Nodes.Clear();
            schemaText.Text = string.Empty;

            hexView.ByteProvider = new DynamicByteProvider(_file.Bytes);
            CreateTemplateSchema(_file.Name);
        }

        public void SaveSchema()
        {
            using var sfd = new SaveFileDialog();
            sfd.Filter = "Flatbuffer Schema|*.fbs|All files|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            File.WriteAllText(sfd.FileName, schemaText.Text);
        }
        #endregion

        #region UI_UPDATE
        private void UIEnabled(bool b)
        {
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

        public void UpdateSettings()
        {
            schemaText.ShowLineNumbers(Settings.Instance.ShowTextNumbers);
            dataInspectorPanel.Visible = Settings.Instance.ShowDataInspector;
            tableLayoutPanel1.PerformLayout();
        }

        private void UpdateDataInspector()
        {
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

        #region UI_INIT
        private void InitIDE()
        {
            schemaText.InitFbsLexer();
            schemaText.SetupAutoComplete();
        }

        private void InitHexView()
        {
            hexView.SelectionLengthChanged += (s, e) => HexSelectionChanged?.Invoke(this, EventArgs.Empty);

            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripMenuItem("Copy", null, (s, e) => hexView.Copy()));
            menu.Items.Add(new ToolStripMenuItem("Select All", null, (s, e) => hexView.SelectAll()));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Open in new tab", null, (s, e) =>
            {
                var start = hexView.SelectionStart;
                var length = hexView.SelectionLength;

                var selectedBytes = new byte[length];
                Array.Copy(_file.Bytes, start, selectedBytes, 0, length);
                RequestNewFile?.Invoke(this, new FileAccessor($"Buffer_0x{start.ToString("X")}", selectedBytes));
            }));
            hexView.ContextMenuStrip = menu;
        }

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
        }
        #endregion

        private void hexView_MouseUp(object sender, MouseEventArgs e) => UpdateDataInspector();
    }
}
