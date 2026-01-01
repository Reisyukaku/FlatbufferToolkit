using Be.Windows.Forms;
using FlatbufferToolkit.UI;
using FlatbufferToolkit.UI.HexView;
using FlatbufferToolkit.UI.IDE;
using FlatbufferToolkit.Utils;
using ScintillaNET;
using System.Diagnostics;
using System.Security.Cryptography;

namespace FlatbufferToolkit;

public partial class MainForm : Form
{

    public MainForm()
    {
        InitializeComponent();

        Progress.Initialize(progressBar1, progressLbl);
    }

    EditorForm GetCurrentTab()
    {
        var tab = tabCtrl.SelectedTab;
        if (tab == null)
            return null;

        return tab.Controls.OfType<EditorForm>().FirstOrDefault();
    }

    private void CreateNew(FileAccessor file)
    {
        var tab = new TabPage();
        tab.Text = file.Name;
        var editor = new EditorForm();
        editor.HexSelectionChanged += hexView_SelectionLengthChanged;
        editor.RequestNewFile += Editor_RequestNewFile;
        editor.LoadFile(file);
        tab.Controls.Add(editor);
        tabCtrl.TabPages.Add(tab);
        tabCtrl.SelectedIndex = tabCtrl.TabCount - 1;
    }

    private void UpdateHexLabel()
    {
        var selected = GetCurrentTab().GetSelectedHex();
        hexLbl.Text = $"Hex: 0x{selected.Item1:X} | 0x{selected.Item2:X} bytes";
    }

    #region UI_CALLBACKS
    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Title = "Open Flatbuffer Binary File";
        ofd.Filter = "All files|*.*";
        if (ofd.ShowDialog() != DialogResult.OK) return;

        CreateNew(new FileAccessor(ofd.FileName));

        outTxt.Text = string.Empty;
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files != null && files.Length > 0)
        {
            foreach (var file in files)
            {
                CreateNew(new FileAccessor(file));
            }
            outTxt.Text = string.Empty;
        }
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void MainForm_Load(object sender, EventArgs e) => Logger.Initialize(outTxt);
    private void saveSchemaAsToolStripMenuItem_Click(object sender, EventArgs e) => GetCurrentTab().SaveSchema();
    private void hexView_SelectionLengthChanged(object sender, EventArgs e) => UpdateHexLabel();
    private void Editor_RequestNewFile(object sender, FileAccessor e) => CreateNew(e);
    private void tabCtrl_SelectedIndexChanged(object sender, EventArgs e)
    {
        GetCurrentTab().UpdateSettings();
        UpdateHexLabel();
    }

    private void dataInspectorToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Settings.Instance.ShowDataInspector = dataInspectorToolStripMenuItem.Checked;
        GetCurrentTab().UpdateSettings();
    }

    private void showLineNumbersToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Settings.Instance.ShowTextNumbers = showLineNumbersToolStripMenuItem.Checked;
        GetCurrentTab().UpdateSettings();
    }

    private void schemaText_UpdateUI(object sender, UpdateUIEventArgs e) => textLbl.Text = $"Text: Line {GetCurrentTab().GetCurrentTextLine() + 1}";

    private void runToolStripMenuItem_Click(object sender, EventArgs e)
    {
        GetCurrentTab().ParseSchema();
        outTxt.Text = string.Empty;
    }

    #endregion
}