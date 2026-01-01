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
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveSchemaAsToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            dataInspectorToolStripMenuItem = new ToolStripMenuItem();
            iDEToolStripMenuItem = new ToolStripMenuItem();
            showLineNumbersToolStripMenuItem = new ToolStripMenuItem();
            runToolStripMenuItem = new ToolStripMenuItem();
            outTxt = new RichTextBox();
            hexLbl = new Label();
            textLbl = new Label();
            progressBar1 = new ProgressBar();
            progressLbl = new Label();
            tabCtrl = new TabControl();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = Color.Transparent;
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, runToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1258, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveSchemaAsToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(143, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // saveSchemaAsToolStripMenuItem
            // 
            saveSchemaAsToolStripMenuItem.Name = "saveSchemaAsToolStripMenuItem";
            saveSchemaAsToolStripMenuItem.Size = new Size(143, 22);
            saveSchemaAsToolStripMenuItem.Text = "Save Schema";
            saveSchemaAsToolStripMenuItem.Click += saveSchemaAsToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dataInspectorToolStripMenuItem, iDEToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // dataInspectorToolStripMenuItem
            // 
            dataInspectorToolStripMenuItem.CheckOnClick = true;
            dataInspectorToolStripMenuItem.Name = "dataInspectorToolStripMenuItem";
            dataInspectorToolStripMenuItem.Size = new Size(150, 22);
            dataInspectorToolStripMenuItem.Text = "Data Inspector";
            dataInspectorToolStripMenuItem.Click += dataInspectorToolStripMenuItem_Click;
            // 
            // iDEToolStripMenuItem
            // 
            iDEToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showLineNumbersToolStripMenuItem });
            iDEToolStripMenuItem.Name = "iDEToolStripMenuItem";
            iDEToolStripMenuItem.Size = new Size(150, 22);
            iDEToolStripMenuItem.Text = "IDE";
            // 
            // showLineNumbersToolStripMenuItem
            // 
            showLineNumbersToolStripMenuItem.Checked = true;
            showLineNumbersToolStripMenuItem.CheckOnClick = true;
            showLineNumbersToolStripMenuItem.CheckState = CheckState.Checked;
            showLineNumbersToolStripMenuItem.Name = "showLineNumbersToolStripMenuItem";
            showLineNumbersToolStripMenuItem.Size = new Size(180, 22);
            showLineNumbersToolStripMenuItem.Text = "Show Line Numbers";
            showLineNumbersToolStripMenuItem.Click += showLineNumbersToolStripMenuItem_Click;
            // 
            // runToolStripMenuItem
            // 
            runToolStripMenuItem.Name = "runToolStripMenuItem";
            runToolStripMenuItem.Size = new Size(40, 20);
            runToolStripMenuItem.Text = "Run";
            runToolStripMenuItem.Click += runToolStripMenuItem_Click;
            // 
            // outTxt
            // 
            outTxt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            outTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            outTxt.Location = new Point(8, 612);
            outTxt.Name = "outTxt";
            outTxt.ReadOnly = true;
            outTxt.ScrollBars = RichTextBoxScrollBars.Vertical;
            outTxt.Size = new Size(1243, 121);
            outTxt.TabIndex = 5;
            outTxt.Text = "";
            // 
            // hexLbl
            // 
            hexLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            hexLbl.AutoSize = true;
            hexLbl.Location = new Point(12, 738);
            hexLbl.Margin = new Padding(3);
            hexLbl.Name = "hexLbl";
            hexLbl.Size = new Size(31, 15);
            hexLbl.TabIndex = 6;
            hexLbl.Text = "Hex:";
            // 
            // textLbl
            // 
            textLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textLbl.AutoSize = true;
            textLbl.Location = new Point(235, 738);
            textLbl.Margin = new Padding(3);
            textLbl.Name = "textLbl";
            textLbl.Size = new Size(31, 15);
            textLbl.TabIndex = 9;
            textLbl.Text = "Text:";
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            progressBar1.Location = new Point(1124, 738);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(126, 15);
            progressBar1.TabIndex = 10;
            // 
            // progressLbl
            // 
            progressLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            progressLbl.Location = new Point(917, 738);
            progressLbl.Margin = new Padding(3);
            progressLbl.Name = "progressLbl";
            progressLbl.RightToLeft = RightToLeft.No;
            progressLbl.Size = new Size(201, 15);
            progressLbl.TabIndex = 11;
            progressLbl.Text = "Ready";
            progressLbl.TextAlign = ContentAlignment.TopRight;
            // 
            // tabCtrl
            // 
            tabCtrl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabCtrl.Location = new Point(12, 27);
            tabCtrl.Name = "tabCtrl";
            tabCtrl.SelectedIndex = 0;
            tabCtrl.Size = new Size(1234, 579);
            tabCtrl.TabIndex = 12;
            tabCtrl.SelectedIndexChanged += tabCtrl_SelectedIndexChanged;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1258, 761);
            Controls.Add(tabCtrl);
            Controls.Add(progressLbl);
            Controls.Add(progressBar1);
            Controls.Add(textLbl);
            Controls.Add(hexLbl);
            Controls.Add(outTxt);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(515, 255);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Flatbuffer Toolkit";
            Load += MainForm_Load;
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private RichTextBox outTxt;
        private ToolStripMenuItem saveSchemaAsToolStripMenuItem;
        private Label hexLbl;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem dataInspectorToolStripMenuItem;
        private ToolStripMenuItem iDEToolStripMenuItem;
        private ToolStripMenuItem showLineNumbersToolStripMenuItem;
        private Label textLbl;
        private ProgressBar progressBar1;
        private Label progressLbl;
        private ToolStripMenuItem runToolStripMenuItem;
        private TabControl tabCtrl;
    }
}
