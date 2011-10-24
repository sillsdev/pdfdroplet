namespace PdfDroplet
{
    partial class WorkspaceControl
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
            this._browser = new System.Windows.Forms.WebBrowser();
            this.label1 = new System.Windows.Forms.Label();
            this._paperSizeCombo = new System.Windows.Forms.ComboBox();
            this._rightToLeft = new System.Windows.Forms.CheckBox();
            this._layoutChoices = new System.Windows.Forms.TableLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this._reloadPrevious = new System.Windows.Forms.LinkLabel();
            this._aboutLink = new System.Windows.Forms.LinkLabel();
            this._overBrowserPanel = new System.Windows.Forms.Panel();
            this._dragStatus = new System.Windows.Forms.Label();
            this._browseForPdf = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this._mirrorBox = new System.Windows.Forms.CheckBox();
            this._layoutChoices.SuspendLayout();
            this._overBrowserPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _browser
            // 
            this._browser.AllowWebBrowserDrop = false;
            this._browser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._browser.IsWebBrowserContextMenuEnabled = false;
            this._browser.Location = new System.Drawing.Point(126, 0);
            this._browser.MinimumSize = new System.Drawing.Size(20, 20);
            this._browser.Name = "_browser";
            this._browser.Size = new System.Drawing.Size(535, 595);
            this._browser.TabIndex = 0;
            this._browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this._browser_DocumentCompleted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Printer Paper Size";
            // 
            // _paperSizeCombo
            // 
            this._paperSizeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._paperSizeCombo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._paperSizeCombo.FormattingEnabled = true;
            this._paperSizeCombo.Location = new System.Drawing.Point(10, 26);
            this._paperSizeCombo.Name = "_paperSizeCombo";
            this._paperSizeCombo.Size = new System.Drawing.Size(110, 21);
            this._paperSizeCombo.TabIndex = 3;
            this._paperSizeCombo.SelectedIndexChanged += new System.EventHandler(this._paperSizeCombo_SelectedIndexChanged);
            // 
            // _rightToLeft
            // 
            this._rightToLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._rightToLeft.AutoSize = true;
            this._rightToLeft.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._rightToLeft.Location = new System.Drawing.Point(7, 601);
            this._rightToLeft.Name = "_rightToLeft";
            this._rightToLeft.Size = new System.Drawing.Size(146, 17);
            this._rightToLeft.TabIndex = 22;
            this._rightToLeft.Text = "Right-to-Left Language";
            this._rightToLeft.UseVisualStyleBackColor = true;
            this._rightToLeft.CheckedChanged += new System.EventHandler(this.OnRightToLeft_CheckedChanged);
            // 
            // _layoutChoices
            // 
            this._layoutChoices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._layoutChoices.AutoScroll = true;
            this._layoutChoices.ColumnCount = 1;
            this._layoutChoices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._layoutChoices.Controls.Add(this.button1, 0, 0);
            this._layoutChoices.Location = new System.Drawing.Point(7, 57);
            this._layoutChoices.Name = "_layoutChoices";
            this._layoutChoices.RowCount = 3;
            this._layoutChoices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this._layoutChoices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this._layoutChoices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this._layoutChoices.Size = new System.Drawing.Size(113, 538);
            this._layoutChoices.TabIndex = 24;
            this._layoutChoices.VisibleChanged += new System.EventHandler(this._layoutChoices_VisibleChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // _reloadPrevious
            // 
            this._reloadPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._reloadPrevious.AutoSize = true;
            this._reloadPrevious.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._reloadPrevious.Location = new System.Drawing.Point(272, 605);
            this._reloadPrevious.Name = "_reloadPrevious";
            this._reloadPrevious.Size = new System.Drawing.Size(82, 13);
            this._reloadPrevious.TabIndex = 27;
            this._reloadPrevious.TabStop = true;
            this._reloadPrevious.Text = "Open Previous";
            this._reloadPrevious.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._reloadPrevious_LinkClicked_1);
            // 
            // _aboutLink
            // 
            this._aboutLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._aboutLink.AutoSize = true;
            this._aboutLink.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._aboutLink.Location = new System.Drawing.Point(559, 602);
            this._aboutLink.Name = "_aboutLink";
            this._aboutLink.Size = new System.Drawing.Size(107, 13);
            this._aboutLink.TabIndex = 28;
            this._aboutLink.TabStop = true;
            this._aboutLink.Text = "About PdfDroplet...";
            this._aboutLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAboutLinkClicked);
            // 
            // _overBrowserPanel
            // 
            this._overBrowserPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._overBrowserPanel.Controls.Add(this._dragStatus);
            this._overBrowserPanel.Controls.Add(this._browseForPdf);
            this._overBrowserPanel.Location = new System.Drawing.Point(148, 2);
            this._overBrowserPanel.Name = "_overBrowserPanel";
            this._overBrowserPanel.Size = new System.Drawing.Size(403, 331);
            this._overBrowserPanel.TabIndex = 29;
            // 
            // _dragStatus
            // 
            this._dragStatus.AutoSize = true;
            this._dragStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._dragStatus.Location = new System.Drawing.Point(42, 40);
            this._dragStatus.Name = "_dragStatus";
            this._dragStatus.Size = new System.Drawing.Size(117, 21);
            this._dragStatus.TabIndex = 24;
            this._dragStatus.Text = "Drag Message";
            // 
            // _browseForPdf
            // 
            this._browseForPdf.AutoSize = true;
            this._browseForPdf.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._browseForPdf.Location = new System.Drawing.Point(43, 88);
            this._browseForPdf.Name = "_browseForPdf";
            this._browseForPdf.Size = new System.Drawing.Size(172, 21);
            this._browseForPdf.TabIndex = 27;
            this._browseForPdf.TabStop = true;
            this._browseForPdf.Text = "Choose a PDF to open...";
            this._browseForPdf.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._linkChooseFile_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.Location = new System.Drawing.Point(407, 603);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(131, 13);
            this.linkLabel1.TabIndex = 27;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Choose a PDF to open...";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._linkChooseFile_LinkClicked);
            // 
            // _mirrorBox
            // 
            this._mirrorBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._mirrorBox.AutoSize = true;
            this._mirrorBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._mirrorBox.Location = new System.Drawing.Point(173, 603);
            this._mirrorBox.Name = "_mirrorBox";
            this._mirrorBox.Size = new System.Drawing.Size(58, 17);
            this._mirrorBox.TabIndex = 30;
            this._mirrorBox.Text = "Mirror";
            this._mirrorBox.UseVisualStyleBackColor = true;
            this._mirrorBox.CheckedChanged += new System.EventHandler(this.OnMirrorBox_CheckedChanged);
            // 
            // WorkspaceControl
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.Controls.Add(this._mirrorBox);
            this.Controls.Add(this._overBrowserPanel);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this._aboutLink);
            this.Controls.Add(this._reloadPrevious);
            this.Controls.Add(this._layoutChoices);
            this.Controls.Add(this._rightToLeft);
            this.Controls.Add(this._paperSizeCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._browser);
            this.Name = "WorkspaceControl";
            this.Size = new System.Drawing.Size(664, 618);
            this.Load += new System.EventHandler(this.OnLoad);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this.DragLeave += new System.EventHandler(this.OnDragLeave);
            this._layoutChoices.ResumeLayout(false);
            this._overBrowserPanel.ResumeLayout(false);
            this._overBrowserPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser _browser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _paperSizeCombo;
        private System.Windows.Forms.CheckBox _rightToLeft;
        private System.Windows.Forms.TableLayoutPanel _layoutChoices;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.LinkLabel _reloadPrevious;
        private System.Windows.Forms.LinkLabel _aboutLink;
        private System.Windows.Forms.Panel _overBrowserPanel;
        private System.Windows.Forms.Label _dragStatus;
        private System.Windows.Forms.LinkLabel _browseForPdf;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox _mirrorBox;
    }
}
