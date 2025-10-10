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
			this._browser = new Microsoft.Web.WebView2.WinForms.WebView2();
			this.label1 = new System.Windows.Forms.Label();
			this._paperSizeCombo = new System.Windows.Forms.ComboBox();
			this._rightToLeft = new System.Windows.Forms.CheckBox();
			this._layoutChoices = new System.Windows.Forms.TableLayoutPanel();
			this._reloadPrevious = new System.Windows.Forms.LinkLabel();
			this._overBrowserPanel = new System.Windows.Forms.Panel();
			this._dragStatus = new System.Windows.Forms.Label();
			this._browseForPdf = new System.Windows.Forms.LinkLabel();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this._mirrorBox = new System.Windows.Forms.CheckBox();
			this._showCropMarks = new System.Windows.Forms.CheckBox();
			this._helpLink = new System.Windows.Forms.LinkLabel();
			this._mainLayout = new System.Windows.Forms.TableLayoutPanel();
			this._leftColumn = new System.Windows.Forms.TableLayoutPanel();
			this._contentPanel = new System.Windows.Forms.Panel();
			this._bottomBar = new System.Windows.Forms.TableLayoutPanel();
			this._bottomLeftFlow = new System.Windows.Forms.FlowLayoutPanel();
			this._bottomRightFlow = new System.Windows.Forms.FlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this._browser)).BeginInit();
			this._layoutChoices.SuspendLayout();
			this._overBrowserPanel.SuspendLayout();
			this._mainLayout.SuspendLayout();
			this._leftColumn.SuspendLayout();
			this._contentPanel.SuspendLayout();
			this._bottomBar.SuspendLayout();
			this._bottomLeftFlow.SuspendLayout();
			this._bottomRightFlow.SuspendLayout();
			this.SuspendLayout();
			// 
			// _browser
			// 
			this._browser.AllowExternalDrop = false;
			this._browser.CreationProperties = null;
			this._browser.DefaultBackgroundColor = System.Drawing.Color.White;
			this._browser.Dock = System.Windows.Forms.DockStyle.Fill;
			this._browser.Location = new System.Drawing.Point(0, 0);
			this._browser.Margin = new System.Windows.Forms.Padding(0);
			this._browser.Name = "_browser";
			this._browser.Size = new System.Drawing.Size(637, 552);
			this._browser.TabIndex = 0;
			this._browser.ZoomFactor = 1D;
			this._browser.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this._browser_NavigationCompleted);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Printer Paper Size";
			// 
			// _paperSizeCombo
			// 
			this._paperSizeCombo.Dock = System.Windows.Forms.DockStyle.Top;
			this._paperSizeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._paperSizeCombo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._paperSizeCombo.FormattingEnabled = true;
			this._paperSizeCombo.Location = new System.Drawing.Point(0, 17);
			this._paperSizeCombo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
			this._paperSizeCombo.Name = "_paperSizeCombo";
			this._paperSizeCombo.Size = new System.Drawing.Size(126, 21);
			this._paperSizeCombo.TabIndex = 3;
			this._paperSizeCombo.SelectedIndexChanged += new System.EventHandler(this._paperSizeCombo_SelectedIndexChanged);
			// 
			// _rightToLeft
			// 
			this._rightToLeft.AutoSize = true;
			this._rightToLeft.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._rightToLeft.Location = new System.Drawing.Point(0, 0);
			this._rightToLeft.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
			this._rightToLeft.Name = "_rightToLeft";
			this._rightToLeft.Size = new System.Drawing.Size(146, 17);
			this._rightToLeft.TabIndex = 22;
			this._rightToLeft.Text = "Right-to-Left Language";
			this._rightToLeft.UseVisualStyleBackColor = true;
			// 
			// _layoutChoices
			// 
			this._layoutChoices.AutoScroll = true;
			this._layoutChoices.ColumnCount = 1;
			this._layoutChoices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._layoutChoices.Dock = System.Windows.Forms.DockStyle.Fill;
			this._layoutChoices.Location = new System.Drawing.Point(0, 48);
			this._layoutChoices.Margin = new System.Windows.Forms.Padding(0);
			this._layoutChoices.Name = "_layoutChoices";
			this._layoutChoices.RowCount = 1;
			this._layoutChoices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._layoutChoices.Size = new System.Drawing.Size(126, 520);
			this._layoutChoices.TabIndex = 24;
			this._layoutChoices.VisibleChanged += new System.EventHandler(this._layoutChoices_VisibleChanged);
			// 
			// _reloadPrevious
			// 
			this._reloadPrevious.AutoSize = true;
			this._reloadPrevious.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._reloadPrevious.Location = new System.Drawing.Point(318, 2);
			this._reloadPrevious.Margin = new System.Windows.Forms.Padding(12, 2, 0, 0);
			this._reloadPrevious.Name = "_reloadPrevious";
			this._reloadPrevious.Size = new System.Drawing.Size(82, 13);
			this._reloadPrevious.TabIndex = 27;
			this._reloadPrevious.TabStop = true;
			this._reloadPrevious.Text = "Open Previous";
			this._reloadPrevious.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._reloadPrevious_LinkClicked_1);
			// 
			// _overBrowserPanel
			// 
			this._overBrowserPanel.Controls.Add(this._dragStatus);
			this._overBrowserPanel.Controls.Add(this._browseForPdf);
			this._overBrowserPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._overBrowserPanel.Location = new System.Drawing.Point(0, 0);
			this._overBrowserPanel.Margin = new System.Windows.Forms.Padding(0);
			this._overBrowserPanel.Name = "_overBrowserPanel";
			this._overBrowserPanel.Size = new System.Drawing.Size(637, 552);
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
			this._browseForPdf.Text = "Choose a PDF to open";
			this._browseForPdf.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._linkChooseFile_LinkClicked);
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkLabel1.Location = new System.Drawing.Point(0, 2);
			this.linkLabel1.Margin = new System.Windows.Forms.Padding(0, 2, 12, 0);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(131, 13);
			this.linkLabel1.TabIndex = 27;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Choose a PDF to open";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._linkChooseFile_LinkClicked);
			// 
			// _mirrorBox
			// 
			this._mirrorBox.AutoSize = true;
			this._mirrorBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._mirrorBox.Location = new System.Drawing.Point(158, 0);
			this._mirrorBox.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
			this._mirrorBox.Name = "_mirrorBox";
			this._mirrorBox.Size = new System.Drawing.Size(58, 17);
			this._mirrorBox.TabIndex = 30;
			this._mirrorBox.Text = "Mirror";
			this._mirrorBox.UseVisualStyleBackColor = true;
			// 
			// _showCropMarks
			// 
			this._showCropMarks.AutoSize = true;
			this._showCropMarks.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._showCropMarks.Location = new System.Drawing.Point(228, 0);
			this._showCropMarks.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
			this._showCropMarks.Name = "_showCropMarks";
			this._showCropMarks.Size = new System.Drawing.Size(85, 17);
			this._showCropMarks.TabIndex = 31;
			this._showCropMarks.Text = "Crop Marks";
			this._showCropMarks.UseVisualStyleBackColor = true;
			// 
			// _helpLink
			// 
			this._helpLink.AutoSize = true;
			this._helpLink.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._helpLink.Location = new System.Drawing.Point(143, 2);
			this._helpLink.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this._helpLink.Name = "_helpLink";
			this._helpLink.Size = new System.Drawing.Size(31, 13);
			this._helpLink.TabIndex = 32;
			this._helpLink.TabStop = true;
			this._helpLink.Text = "Help";
			this._helpLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnInstructionsLinkClicked);
			// 
			// _mainLayout
			// 
			this._mainLayout.ColumnCount = 2;
			this._mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this._mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._mainLayout.Controls.Add(this._leftColumn, 0, 0);
			this._mainLayout.Controls.Add(this._contentPanel, 1, 0);
			this._mainLayout.Controls.Add(this._bottomBar, 0, 1);
			this._mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this._mainLayout.Location = new System.Drawing.Point(0, 0);
			this._mainLayout.Margin = new System.Windows.Forms.Padding(0);
			this._mainLayout.Name = "_mainLayout";
			this._mainLayout.RowCount = 2;
			this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			this._mainLayout.Size = new System.Drawing.Size(777, 618);
			this._mainLayout.TabIndex = 33;
			this._mainLayout.SetColumnSpan(this._bottomBar, 2);
			// 
			// _leftColumn
			// 
			this._leftColumn.ColumnCount = 1;
			this._leftColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._leftColumn.Controls.Add(this.label1, 0, 0);
			this._leftColumn.Controls.Add(this._paperSizeCombo, 0, 1);
			this._leftColumn.Controls.Add(this._layoutChoices, 0, 2);
			this._leftColumn.Dock = System.Windows.Forms.DockStyle.Fill;
			this._leftColumn.Location = new System.Drawing.Point(0, 0);
			this._leftColumn.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this._leftColumn.Name = "_leftColumn";
			this._leftColumn.RowCount = 3;
			this._leftColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			this._leftColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			this._leftColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._leftColumn.Size = new System.Drawing.Size(140, 565);
			this._leftColumn.TabIndex = 34;
			// 
			// _contentPanel
			// 
			this._contentPanel.Controls.Add(this._overBrowserPanel);
			this._contentPanel.Controls.Add(this._browser);
			this._contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._contentPanel.Location = new System.Drawing.Point(140, 0);
			this._contentPanel.Margin = new System.Windows.Forms.Padding(0);
			this._contentPanel.Name = "_contentPanel";
			this._contentPanel.Size = new System.Drawing.Size(637, 565);
			this._contentPanel.TabIndex = 35;
			// 
			// _bottomBar
			// 
			this._bottomBar.AutoSize = true;
			this._bottomBar.ColumnCount = 2;
			this._bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
			this._bottomBar.Controls.Add(this._bottomLeftFlow, 0, 0);
			this._bottomBar.Controls.Add(this._bottomRightFlow, 1, 0);
			this._bottomBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this._bottomBar.Location = new System.Drawing.Point(0, 565);
			this._bottomBar.Margin = new System.Windows.Forms.Padding(0);
			this._bottomBar.Name = "_bottomBar";
			this._bottomBar.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
			this._bottomBar.RowCount = 1;
			this._bottomBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			this._bottomBar.Size = new System.Drawing.Size(777, 53);
			this._bottomBar.TabIndex = 36;
			// 
			// _bottomLeftFlow
			// 
			this._bottomLeftFlow.AutoSize = true;
			this._bottomLeftFlow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._bottomLeftFlow.Controls.Add(this._rightToLeft);
			this._bottomLeftFlow.Controls.Add(this._mirrorBox);
			this._bottomLeftFlow.Controls.Add(this._showCropMarks);
			this._bottomLeftFlow.Controls.Add(this._reloadPrevious);
			this._bottomLeftFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this._bottomLeftFlow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
			this._bottomLeftFlow.Location = new System.Drawing.Point(0, 4);
			this._bottomLeftFlow.Margin = new System.Windows.Forms.Padding(0);
			this._bottomLeftFlow.Name = "_bottomLeftFlow";
			this._bottomLeftFlow.Size = new System.Drawing.Size(624, 45);
			this._bottomLeftFlow.TabIndex = 0;
			this._bottomLeftFlow.WrapContents = false;
			// 
			// _bottomRightFlow
			// 
			this._bottomRightFlow.AutoSize = true;
			this._bottomRightFlow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._bottomRightFlow.Controls.Add(this.linkLabel1);
			this._bottomRightFlow.Controls.Add(this._helpLink);
			this._bottomRightFlow.Dock = System.Windows.Forms.DockStyle.Right;
			this._bottomRightFlow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
			this._bottomRightFlow.Location = new System.Drawing.Point(633, 4);
			this._bottomRightFlow.Margin = new System.Windows.Forms.Padding(0);
			this._bottomRightFlow.Name = "_bottomRightFlow";
			this._bottomRightFlow.Size = new System.Drawing.Size(132, 45);
			this._bottomRightFlow.TabIndex = 1;
			this._bottomRightFlow.WrapContents = false;
			// 
			// WorkspaceControl
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.Controls.Add(this._mainLayout);
			this.Name = "WorkspaceControl";
			this.Size = new System.Drawing.Size(777, 618);
			this.Load += new System.EventHandler(this.OnLoad);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
			this.DragLeave += new System.EventHandler(this.OnDragLeave);
			this._layoutChoices.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._browser)).EndInit();
			this._overBrowserPanel.ResumeLayout(false);
			this._overBrowserPanel.PerformLayout();
			this._mainLayout.ResumeLayout(false);
			this._mainLayout.PerformLayout();
			this._leftColumn.ResumeLayout(false);
			this._leftColumn.PerformLayout();
			this._contentPanel.ResumeLayout(false);
			this._bottomBar.ResumeLayout(false);
			this._bottomBar.PerformLayout();
			this._bottomLeftFlow.ResumeLayout(false);
			this._bottomLeftFlow.PerformLayout();
			this._bottomRightFlow.ResumeLayout(false);
			this._bottomRightFlow.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Microsoft.Web.WebView2.WinForms.WebView2 _browser;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox _paperSizeCombo;
		private System.Windows.Forms.CheckBox _rightToLeft;
		private System.Windows.Forms.TableLayoutPanel _layoutChoices;
		private System.Windows.Forms.LinkLabel _reloadPrevious;
		private System.Windows.Forms.Panel _overBrowserPanel;
		private System.Windows.Forms.Label _dragStatus;
		private System.Windows.Forms.LinkLabel _browseForPdf;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.CheckBox _mirrorBox;
		private System.Windows.Forms.CheckBox _showCropMarks;
		private System.Windows.Forms.LinkLabel _helpLink;
		private System.Windows.Forms.TableLayoutPanel _mainLayout;
		private System.Windows.Forms.TableLayoutPanel _leftColumn;
		private System.Windows.Forms.Panel _contentPanel;
		private System.Windows.Forms.TableLayoutPanel _bottomBar;
		private System.Windows.Forms.FlowLayoutPanel _bottomLeftFlow;
		private System.Windows.Forms.FlowLayoutPanel _bottomRightFlow;
	}
}
