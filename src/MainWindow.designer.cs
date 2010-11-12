namespace PdfDroplet
{
    partial class MainWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this._tabControl = new System.Windows.Forms.TabControl();
            this._convertPage = new System.Windows.Forms.TabPage();
            this._shrinkPageButton = new System.Windows.Forms.RadioButton();
            this._preservePageSizeButton = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this._linkChooseFile = new System.Windows.Forms.LinkLabel();
            this._labelConvertPrevious = new System.Windows.Forms.Label();
            this._linkConvertPrevious = new System.Windows.Forms.LinkLabel();
            this._dragStatus = new System.Windows.Forms.Label();
            this._labelDragDirections = new System.Windows.Forms.Label();
            this._instructionsPage = new System.Windows.Forms.TabPage();
            this._instructionsBrowser = new System.Windows.Forms.WebBrowser();
            this._bookletPage = new System.Windows.Forms.TabPage();
            this._resultingFileLink = new System.Windows.Forms.LinkLabel();
            this._browserForPdf = new System.Windows.Forms.WebBrowser();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this._rightToLeft = new System.Windows.Forms.CheckBox();
            this._tabControl.SuspendLayout();
            this._convertPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this._instructionsPage.SuspendLayout();
            this._bookletPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // linkLabel2
            // 
            this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(21, 528);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(97, 13);
            this.linkLabel2.TabIndex = 0;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "About PdfDroplet...";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // _tabControl
            // 
            this._tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tabControl.Controls.Add(this._convertPage);
            this._tabControl.Controls.Add(this._instructionsPage);
            this._tabControl.Controls.Add(this._bookletPage);
            this._tabControl.Location = new System.Drawing.Point(8, 9);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(701, 508);
            this._tabControl.TabIndex = 7;
            this._tabControl.SelectedIndexChanged += new System.EventHandler(this._tabControl_SelectedIndexChanged);
            this._tabControl.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
            this._tabControl.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this._tabControl.DragLeave += new System.EventHandler(this.OnDragLeave);
            // 
            // _convertPage
            // 
            this._convertPage.Controls.Add(this._shrinkPageButton);
            this._convertPage.Controls.Add(this._preservePageSizeButton);
            this._convertPage.Controls.Add(this.pictureBox1);
            this._convertPage.Controls.Add(this._linkChooseFile);
            this._convertPage.Controls.Add(this._labelConvertPrevious);
            this._convertPage.Controls.Add(this._linkConvertPrevious);
            this._convertPage.Controls.Add(this._dragStatus);
            this._convertPage.Controls.Add(this._labelDragDirections);
            this._convertPage.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._convertPage.Location = new System.Drawing.Point(4, 22);
            this._convertPage.Name = "_convertPage";
            this._convertPage.Padding = new System.Windows.Forms.Padding(3);
            this._convertPage.Size = new System.Drawing.Size(693, 482);
            this._convertPage.TabIndex = 0;
            this._convertPage.Text = "Convert";
            this._convertPage.UseVisualStyleBackColor = true;
            this._convertPage.Resize += new System.EventHandler(this.tabPage1_Resize);
            // 
            // _shrinkPageButton
            // 
            this._shrinkPageButton.AutoSize = true;
            this._shrinkPageButton.Location = new System.Drawing.Point(39, 53);
            this._shrinkPageButton.Name = "_shrinkPageButton";
            this._shrinkPageButton.Size = new System.Drawing.Size(504, 25);
            this._shrinkPageButton.TabIndex = 20;
            this._shrinkPageButton.TabStop = true;
            this._shrinkPageButton.Text = "Shrink pages by 50% (to print on paper the same size as the original)";
            this._shrinkPageButton.UseVisualStyleBackColor = true;
            // 
            // _preservePageSizeButton
            // 
            this._preservePageSizeButton.AutoSize = true;
            this._preservePageSizeButton.Location = new System.Drawing.Point(39, 28);
            this._preservePageSizeButton.Name = "_preservePageSizeButton";
            this._preservePageSizeButton.Size = new System.Drawing.Size(523, 25);
            this._preservePageSizeButton.TabIndex = 19;
            this._preservePageSizeButton.TabStop = true;
            this._preservePageSizeButton.Text = "Keep original size (to print on paper that is twice the size of the original)";
            this._preservePageSizeButton.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(475, 326);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(189, 138);
            this.pictureBox1.TabIndex = 16;
            this.pictureBox1.TabStop = false;
            // 
            // _linkChooseFile
            // 
            this._linkChooseFile.AutoSize = true;
            this._linkChooseFile.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._linkChooseFile.Location = new System.Drawing.Point(34, 105);
            this._linkChooseFile.Name = "_linkChooseFile";
            this._linkChooseFile.Size = new System.Drawing.Size(205, 21);
            this._linkChooseFile.TabIndex = 0;
            this._linkChooseFile.TabStop = true;
            this._linkChooseFile.Text = "Choose a PDF file to convert";
            this._linkChooseFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._linkChooseFile_LinkClicked);
            // 
            // _labelConvertPrevious
            // 
            this._labelConvertPrevious.AutoSize = true;
            this._labelConvertPrevious.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelConvertPrevious.Location = new System.Drawing.Point(34, 161);
            this._labelConvertPrevious.Name = "_labelConvertPrevious";
            this._labelConvertPrevious.Size = new System.Drawing.Size(25, 21);
            this._labelConvertPrevious.TabIndex = 6;
            this._labelConvertPrevious.Text = "or";
            // 
            // _linkConvertPrevious
            // 
            this._linkConvertPrevious.AutoSize = true;
            this._linkConvertPrevious.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._linkConvertPrevious.Location = new System.Drawing.Point(56, 161);
            this._linkConvertPrevious.Name = "_linkConvertPrevious";
            this._linkConvertPrevious.Size = new System.Drawing.Size(133, 21);
            this._linkConvertPrevious.TabIndex = 2;
            this._linkConvertPrevious.TabStop = true;
            this._linkConvertPrevious.Text = "Convert {0} again.";
            this._linkConvertPrevious.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._linkConvertPrevious_LinkClicked);
            // 
            // _dragStatus
            // 
            this._dragStatus.AutoSize = true;
            this._dragStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._dragStatus.Location = new System.Drawing.Point(33, 198);
            this._dragStatus.Name = "_dragStatus";
            this._dragStatus.Size = new System.Drawing.Size(117, 21);
            this._dragStatus.TabIndex = 7;
            this._dragStatus.Text = "Drag Message";
            // 
            // _labelDragDirections
            // 
            this._labelDragDirections.AutoSize = true;
            this._labelDragDirections.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelDragDirections.Location = new System.Drawing.Point(34, 132);
            this._labelDragDirections.Name = "_labelDragDirections";
            this._labelDragDirections.Size = new System.Drawing.Size(250, 21);
            this._labelDragDirections.TabIndex = 1;
            this._labelDragDirections.Text = "or drop a pdf file onto this window";
            // 
            // _instructionsPage
            // 
            this._instructionsPage.Controls.Add(this._instructionsBrowser);
            this._instructionsPage.Location = new System.Drawing.Point(4, 22);
            this._instructionsPage.Name = "_instructionsPage";
            this._instructionsPage.Size = new System.Drawing.Size(686, 494);
            this._instructionsPage.TabIndex = 2;
            this._instructionsPage.Text = "Instructions";
            this._instructionsPage.UseVisualStyleBackColor = true;
            // 
            // _instructionsBrowser
            // 
            this._instructionsBrowser.AllowNavigation = false;
            this._instructionsBrowser.AllowWebBrowserDrop = false;
            this._instructionsBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this._instructionsBrowser.Location = new System.Drawing.Point(0, 0);
            this._instructionsBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this._instructionsBrowser.Name = "_instructionsBrowser";
            this._instructionsBrowser.Size = new System.Drawing.Size(686, 494);
            this._instructionsBrowser.TabIndex = 0;
            this._instructionsBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // _bookletPage
            // 
            this._bookletPage.Controls.Add(this._resultingFileLink);
            this._bookletPage.Controls.Add(this._browserForPdf);
            this._bookletPage.Location = new System.Drawing.Point(4, 22);
            this._bookletPage.Name = "_bookletPage";
            this._bookletPage.Padding = new System.Windows.Forms.Padding(3);
            this._bookletPage.Size = new System.Drawing.Size(686, 494);
            this._bookletPage.TabIndex = 1;
            this._bookletPage.Text = "Booklet";
            this._bookletPage.UseVisualStyleBackColor = true;
            // 
            // _resultingFileLink
            // 
            this._resultingFileLink.AutoSize = true;
            this._resultingFileLink.Location = new System.Drawing.Point(9, 3);
            this._resultingFileLink.Name = "_resultingFileLink";
            this._resultingFileLink.Size = new System.Drawing.Size(45, 13);
            this._resultingFileLink.TabIndex = 9;
            this._resultingFileLink.TabStop = true;
            this._resultingFileLink.Text = "Results:";
            this._resultingFileLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._resultingFileLink_LinkClicked);
            // 
            // _browserForPdf
            // 
            this._browserForPdf.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._browserForPdf.Location = new System.Drawing.Point(3, 36);
            this._browserForPdf.MinimumSize = new System.Drawing.Size(20, 20);
            this._browserForPdf.Name = "_browserForPdf";
            this._browserForPdf.Size = new System.Drawing.Size(683, 458);
            this._browserForPdf.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // _rightToLeft
            // 
            this._rightToLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._rightToLeft.AutoSize = true;
            this._rightToLeft.Location = new System.Drawing.Point(448, 524);
            this._rightToLeft.Name = "_rightToLeft";
            this._rightToLeft.Size = new System.Drawing.Size(228, 17);
            this._rightToLeft.TabIndex = 8;
            this._rightToLeft.Text = "Layout booklet for Right-to-Left Languages";
            this._rightToLeft.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 550);
            this.Controls.Add(this._rightToLeft);
            this.Controls.Add(this._tabControl);
            this.Controls.Add(this.linkLabel2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "PdfDroplet 0.1";
            this.Load += new System.EventHandler(this.OnLoad);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this.DragLeave += new System.EventHandler(this.OnDragLeave);
            this._tabControl.ResumeLayout(false);
            this._convertPage.ResumeLayout(false);
            this._convertPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this._instructionsPage.ResumeLayout(false);
            this._bookletPage.ResumeLayout(false);
            this._bookletPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _convertPage;
        private System.Windows.Forms.LinkLabel _linkChooseFile;
        private System.Windows.Forms.LinkLabel _linkConvertPrevious;
        private System.Windows.Forms.Label _labelDragDirections;
        private System.Windows.Forms.TabPage _bookletPage;
        private System.Windows.Forms.Label _dragStatus;
        private System.Windows.Forms.Label _labelConvertPrevious;
        private System.Windows.Forms.WebBrowser _browserForPdf;
        private System.Windows.Forms.LinkLabel _resultingFileLink;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabPage _instructionsPage;
        private System.Windows.Forms.WebBrowser _instructionsBrowser;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RadioButton _shrinkPageButton;
        private System.Windows.Forms.RadioButton _preservePageSizeButton;
        private System.Windows.Forms.CheckBox _rightToLeft;
    }
}