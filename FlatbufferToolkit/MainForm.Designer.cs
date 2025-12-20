using Be.Windows.Forms;
using ScintillaNET;
using System.ComponentModel.Design;

namespace FlatbufferToolkit
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveSchemaAsToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            dataInspectorToolStripMenuItem = new ToolStripMenuItem();
            iDEToolStripMenuItem = new ToolStripMenuItem();
            showLineNumbersToolStripMenuItem = new ToolStripMenuItem();
            runToolStripMenuItem = new ToolStripMenuItem();
            hexView = new HexBox();
            schemaText = new Scintilla();
            treeView = new TreeView();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            outTxt = new RichTextBox();
            hexLbl = new Label();
            dataInspector = new GroupBox();
            dataInspectorGrid = new DataGridView();
            Unit = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            tableLayoutPanel1 = new TableLayoutPanel();
            dataInspectorPanel = new Panel();
            dataInspectorSettings = new GroupBox();
            textLbl = new Label();
            progressBar1 = new ProgressBar();
            progressLbl = new Label();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            dataInspector.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataInspectorGrid).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            dataInspectorPanel.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = Color.Transparent;
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, runToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1258, 25);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveSchemaAsToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(39, 21);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(152, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // saveSchemaAsToolStripMenuItem
            // 
            saveSchemaAsToolStripMenuItem.Name = "saveSchemaAsToolStripMenuItem";
            saveSchemaAsToolStripMenuItem.Size = new Size(152, 22);
            saveSchemaAsToolStripMenuItem.Text = "Save Schema";
            saveSchemaAsToolStripMenuItem.Click += saveSchemaAsToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dataInspectorToolStripMenuItem, iDEToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(47, 21);
            viewToolStripMenuItem.Text = "View";
            // 
            // dataInspectorToolStripMenuItem
            // 
            dataInspectorToolStripMenuItem.CheckOnClick = true;
            dataInspectorToolStripMenuItem.Name = "dataInspectorToolStripMenuItem";
            dataInspectorToolStripMenuItem.Size = new Size(161, 22);
            dataInspectorToolStripMenuItem.Text = "Data Inspector";
            dataInspectorToolStripMenuItem.Click += dataInspectorToolStripMenuItem_Click;
            // 
            // iDEToolStripMenuItem
            // 
            iDEToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showLineNumbersToolStripMenuItem });
            iDEToolStripMenuItem.Name = "iDEToolStripMenuItem";
            iDEToolStripMenuItem.Size = new Size(161, 22);
            iDEToolStripMenuItem.Text = "IDE";
            // 
            // showLineNumbersToolStripMenuItem
            // 
            showLineNumbersToolStripMenuItem.Checked = true;
            showLineNumbersToolStripMenuItem.CheckOnClick = true;
            showLineNumbersToolStripMenuItem.CheckState = CheckState.Checked;
            showLineNumbersToolStripMenuItem.Name = "showLineNumbersToolStripMenuItem";
            showLineNumbersToolStripMenuItem.Size = new Size(192, 22);
            showLineNumbersToolStripMenuItem.Text = "Show Line Numbers";
            showLineNumbersToolStripMenuItem.Click += showLineNumbersToolStripMenuItem_Click;
            // 
            // runToolStripMenuItem
            // 
            runToolStripMenuItem.Name = "runToolStripMenuItem";
            runToolStripMenuItem.Size = new Size(42, 21);
            runToolStripMenuItem.Text = "Run";
            runToolStripMenuItem.Click += runToolStripMenuItem_Click;
            // 
            // hexView
            // 
            hexView.BackColor = SystemColors.Window;
            hexView.BackColorDisabled = SystemColors.ControlLight;
            hexView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            hexView.ColumnInfoVisible = true;
            hexView.Dock = DockStyle.Fill;
            hexView.Font = new Font("Consolas", 10F);
            hexView.InfoForeColor = SystemColors.ControlDark;
            hexView.LineInfoVisible = true;
            hexView.Location = new Point(0, 0);
            hexView.Name = "hexView";
            hexView.ReadOnly = true;
            hexView.SelectionBackColor = SystemColors.Highlight;
            hexView.SelectionForeColor = SystemColors.HighlightText;
            hexView.ShadowSelectionColor = Color.FromArgb(100, 60, 188, 255);
            hexView.Size = new Size(650, 310);
            hexView.StringViewVisible = true;
            hexView.TabIndex = 0;
            hexView.VScrollBarVisible = true;
            hexView.MouseUp += hexView_MouseUp;
            // 
            // schemaText
            // 
            schemaText.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 215);
            schemaText.BorderStyle = ScintillaNET.BorderStyle.FixedSingle;
            schemaText.Dock = DockStyle.Fill;
            schemaText.LexerName = null;
            schemaText.Location = new Point(0, 0);
            schemaText.Margin = new Padding(2, 2, 2, 2);
            schemaText.Name = "schemaText";
            schemaText.Size = new Size(324, 567);
            schemaText.TabIndex = 1;
            schemaText.UpdateUI += schemaText_UpdateUI;
            // 
            // treeView
            // 
            treeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            treeView.Dock = DockStyle.Fill;
            treeView.Location = new Point(0, 0);
            treeView.Name = "treeView";
            treeView.ShowNodeToolTips = true;
            treeView.Size = new Size(650, 252);
            treeView.TabIndex = 2;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(schemaText);
            splitContainer1.Size = new Size(978, 567);
            splitContainer1.SplitterDistance = 650;
            splitContainer1.TabIndex = 3;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(hexView);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(treeView);
            splitContainer2.Size = new Size(650, 567);
            splitContainer2.SplitterDistance = 310;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // outTxt
            // 
            outTxt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            outTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            outTxt.Location = new Point(8, 605);
            outTxt.Name = "outTxt";
            outTxt.ReadOnly = true;
            outTxt.ScrollBars = RichTextBoxScrollBars.Vertical;
            outTxt.Size = new Size(1243, 137);
            outTxt.TabIndex = 5;
            outTxt.Text = "";
            // 
            // hexLbl
            // 
            hexLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            hexLbl.AutoSize = true;
            hexLbl.Location = new Point(12, 748);
            hexLbl.Margin = new Padding(3, 3, 3, 3);
            hexLbl.Name = "hexLbl";
            hexLbl.Size = new Size(33, 17);
            hexLbl.TabIndex = 6;
            hexLbl.Text = "Hex:";
            // 
            // dataInspector
            // 
            dataInspector.Controls.Add(dataInspectorGrid);
            dataInspector.Dock = DockStyle.Top;
            dataInspector.Location = new Point(0, 0);
            dataInspector.Name = "dataInspector";
            dataInspector.Size = new Size(252, 314);
            dataInspector.TabIndex = 7;
            dataInspector.TabStop = false;
            dataInspector.Text = "Data Inspector";
            // 
            // dataInspectorGrid
            // 
            dataInspectorGrid.AllowUserToAddRows = false;
            dataInspectorGrid.AllowUserToDeleteRows = false;
            dataInspectorGrid.AllowUserToResizeColumns = false;
            dataInspectorGrid.AllowUserToResizeRows = false;
            dataInspectorGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataInspectorGrid.BackgroundColor = SystemColors.Control;
            dataInspectorGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataInspectorGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataInspectorGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataInspectorGrid.ColumnHeadersVisible = false;
            dataInspectorGrid.Columns.AddRange(new DataGridViewColumn[] { Unit, Value });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.NullValue = null;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataInspectorGrid.DefaultCellStyle = dataGridViewCellStyle2;
            dataInspectorGrid.Dock = DockStyle.Fill;
            dataInspectorGrid.Location = new Point(3, 21);
            dataInspectorGrid.Margin = new Padding(0);
            dataInspectorGrid.Name = "dataInspectorGrid";
            dataInspectorGrid.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dataInspectorGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataInspectorGrid.RowHeadersVisible = false;
            dataInspectorGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataInspectorGrid.ScrollBars = ScrollBars.None;
            dataInspectorGrid.Size = new Size(246, 290);
            dataInspectorGrid.TabIndex = 0;
            // 
            // Unit
            // 
            Unit.HeaderText = "Unit";
            Unit.MinimumWidth = 8;
            Unit.Name = "Unit";
            Unit.ReadOnly = true;
            // 
            // Value
            // 
            Value.HeaderText = "Value";
            Value.MinimumWidth = 8;
            Value.Name = "Value";
            Value.ReadOnly = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(splitContainer1, 0, 0);
            tableLayoutPanel1.Controls.Add(dataInspectorPanel, 1, 0);
            tableLayoutPanel1.Location = new Point(8, 27);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1242, 573);
            tableLayoutPanel1.TabIndex = 8;
            // 
            // dataInspectorPanel
            // 
            dataInspectorPanel.Controls.Add(dataInspectorSettings);
            dataInspectorPanel.Controls.Add(dataInspector);
            dataInspectorPanel.Dock = DockStyle.Fill;
            dataInspectorPanel.Location = new Point(987, 3);
            dataInspectorPanel.Name = "dataInspectorPanel";
            dataInspectorPanel.Size = new Size(252, 567);
            dataInspectorPanel.TabIndex = 4;
            // 
            // dataInspectorSettings
            // 
            dataInspectorSettings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataInspectorSettings.Location = new Point(0, 318);
            dataInspectorSettings.Name = "dataInspectorSettings";
            dataInspectorSettings.Size = new Size(252, 250);
            dataInspectorSettings.TabIndex = 8;
            dataInspectorSettings.TabStop = false;
            dataInspectorSettings.Text = "Settings";
            // 
            // textLbl
            // 
            textLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textLbl.AutoSize = true;
            textLbl.Location = new Point(235, 748);
            textLbl.Margin = new Padding(3, 3, 3, 3);
            textLbl.Name = "textLbl";
            textLbl.Size = new Size(34, 17);
            textLbl.TabIndex = 9;
            textLbl.Text = "Text:";
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            progressBar1.Location = new Point(1124, 748);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(126, 17);
            progressBar1.TabIndex = 10;
            // 
            // progressLbl
            // 
            progressLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            progressLbl.Location = new Point(917, 748);
            progressLbl.Margin = new Padding(3, 3, 3, 3);
            progressLbl.Name = "progressLbl";
            progressLbl.RightToLeft = RightToLeft.No;
            progressLbl.Size = new Size(201, 17);
            progressLbl.TabIndex = 11;
            progressLbl.Text = "Ready";
            progressLbl.TextAlign = ContentAlignment.TopRight;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1258, 774);
            Controls.Add(progressLbl);
            Controls.Add(progressBar1);
            Controls.Add(textLbl);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(hexLbl);
            Controls.Add(outTxt);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(515, 284);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Flatbuffer Toolkit";
            Load += MainForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            dataInspector.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataInspectorGrid).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            dataInspectorPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private HexBox hexView;
        private Scintilla schemaText;
        private TreeView treeView;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private RichTextBox outTxt;
        private ToolStripMenuItem saveSchemaAsToolStripMenuItem;
        private Label hexLbl;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem dataInspectorToolStripMenuItem;
        private GroupBox dataInspector;
        private TableLayoutPanel tableLayoutPanel1;
        private DataGridView dataInspectorGrid;
        private DataGridViewTextBoxColumn Unit;
        private DataGridViewTextBoxColumn Value;
        private Panel dataInspectorPanel;
        private GroupBox dataInspectorSettings;
        private ToolStripMenuItem iDEToolStripMenuItem;
        private ToolStripMenuItem showLineNumbersToolStripMenuItem;
        private Label textLbl;
        private ProgressBar progressBar1;
        private Label progressLbl;
        private ToolStripMenuItem runToolStripMenuItem;
    }
}
