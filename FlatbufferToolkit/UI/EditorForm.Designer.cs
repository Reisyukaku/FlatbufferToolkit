namespace FlatbufferToolkit.UI
{
    partial class EditorForm
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            tableLayoutPanel1 = new TableLayoutPanel();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            hexView = new Be.Windows.Forms.HexBox();
            treeView = new TreeView();
            schemaText = new ScintillaNET.Scintilla();
            dataInspectorPanel = new Panel();
            dataInspectorSettings = new GroupBox();
            dataInspector = new GroupBox();
            dataInspectorGrid = new DataGridView();
            Unit = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            dataInspectorPanel.SuspendLayout();
            dataInspector.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataInspectorGrid).BeginInit();
            SuspendLayout();
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
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1255, 714);
            tableLayoutPanel1.TabIndex = 9;
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
            splitContainer1.Size = new Size(991, 708);
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
            splitContainer2.Size = new Size(650, 708);
            splitContainer2.SplitterDistance = 385;
            splitContainer2.TabIndex = 0;
            // 
            // hexView
            // 
            hexView.BackColor = SystemColors.Window;
            hexView.BackColorDisabled = SystemColors.ControlLight;
            hexView.BorderStyle = BorderStyle.FixedSingle;
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
            hexView.Size = new Size(650, 385);
            hexView.StringViewVisible = true;
            hexView.TabIndex = 0;
            hexView.VScrollBarVisible = true;
            hexView.MouseUp += hexView_MouseUp;
            // 
            // treeView
            // 
            treeView.BorderStyle = BorderStyle.FixedSingle;
            treeView.Dock = DockStyle.Fill;
            treeView.Location = new Point(0, 0);
            treeView.Name = "treeView";
            treeView.ShowNodeToolTips = true;
            treeView.Size = new Size(650, 319);
            treeView.TabIndex = 2;
            // 
            // schemaText
            // 
            schemaText.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 215);
            schemaText.BorderStyle = ScintillaNET.BorderStyle.FixedSingle;
            schemaText.Dock = DockStyle.Fill;
            schemaText.LexerName = null;
            schemaText.Location = new Point(0, 0);
            schemaText.Margin = new Padding(2);
            schemaText.Name = "schemaText";
            schemaText.Size = new Size(337, 708);
            schemaText.TabIndex = 1;
            // 
            // dataInspectorPanel
            // 
            dataInspectorPanel.Controls.Add(dataInspectorSettings);
            dataInspectorPanel.Controls.Add(dataInspector);
            dataInspectorPanel.Dock = DockStyle.Fill;
            dataInspectorPanel.Location = new Point(1000, 3);
            dataInspectorPanel.Name = "dataInspectorPanel";
            dataInspectorPanel.Size = new Size(252, 708);
            dataInspectorPanel.TabIndex = 4;
            // 
            // dataInspectorSettings
            // 
            dataInspectorSettings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataInspectorSettings.Location = new Point(0, 281);
            dataInspectorSettings.Name = "dataInspectorSettings";
            dataInspectorSettings.Size = new Size(304, 907);
            dataInspectorSettings.TabIndex = 8;
            dataInspectorSettings.TabStop = false;
            dataInspectorSettings.Text = "Settings";
            // 
            // dataInspector
            // 
            dataInspector.Controls.Add(dataInspectorGrid);
            dataInspector.Dock = DockStyle.Top;
            dataInspector.Location = new Point(0, 0);
            dataInspector.Name = "dataInspector";
            dataInspector.Size = new Size(252, 277);
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
            dataInspectorGrid.BorderStyle = BorderStyle.None;
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
            dataInspectorGrid.Location = new Point(3, 19);
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
            dataInspectorGrid.Size = new Size(246, 255);
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
            // EditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "EditorForm";
            Size = new Size(1255, 714);
            tableLayoutPanel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            dataInspectorPanel.ResumeLayout(false);
            dataInspector.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataInspectorGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private Be.Windows.Forms.HexBox hexView;
        private TreeView treeView;
        private ScintillaNET.Scintilla schemaText;
        private Panel dataInspectorPanel;
        private GroupBox dataInspectorSettings;
        private GroupBox dataInspector;
        private DataGridView dataInspectorGrid;
        private DataGridViewTextBoxColumn Unit;
        private DataGridViewTextBoxColumn Value;
    }
}
