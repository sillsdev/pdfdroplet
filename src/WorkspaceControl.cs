using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DesktopAnalytics;
using Microsoft.Web.WebView2.Core;
using PdfDroplet.LayoutMethods;
using PdfDroplet.Properties;
using SIL.IO;

namespace PdfDroplet
{
    public partial class WorkspaceControl : UserControl
    {
        private WorkSpaceViewModel _model;
        private bool _alreadyLoaded;
        private Action _callWhenNavigated;
        private string _urlWeWereShowing;

        public WorkspaceControl()
        {
            InitializeComponent();
            _model = new WorkSpaceViewModel(this);
            OnDragLeave(null, null);
            _overBrowserPanel.Bounds = _browser.Bounds;
            _mirrorBox.Checked = Settings.Default.Mirror;
            _rightToLeft.Checked = Settings.Default.RightToLeft;
            _showCropMarks.Checked = Settings.Default.ShowCropMarks;

            // Add 10px padding to left, right, and bottom
            this.Padding = new Padding(10, 0, 10, 10);

            //important to do this after the above settings
            this._mirrorBox.CheckedChanged += new System.EventHandler(this.OnMirrorBox_CheckedChanged);
            this._rightToLeft.CheckedChanged += new System.EventHandler(this.OnRightToLeft_CheckedChanged);
            this._showCropMarks.CheckedChanged += new System.EventHandler(this.OnShowCropMarks_CheckedChanged);

            // Initialize WebView2
            InitializeWebView2Async();
        }

        private async void InitializeWebView2Async()
        {
            try
            {
                // Set user data folder to a writable location in AppData
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PdfDroplet", "WebView2");

                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await _browser.EnsureCoreWebView2Async(environment);

                // Configure PDF toolbar settings
                _browser.CoreWebView2.Settings.HiddenPdfToolbarItems =
                    CoreWebView2PdfToolbarItems.Print 
                    | CoreWebView2PdfToolbarItems.Rotate 
                    | CoreWebView2PdfToolbarItems.Save 
                    | CoreWebView2PdfToolbarItems.SaveAs
                    | CoreWebView2PdfToolbarItems.FullScreen
                    | CoreWebView2PdfToolbarItems.MoreSettings; 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView2: {ex.Message}\n\nPlease ensure WebView2 Runtime is installed.",
                    "WebView2 Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void OnLoad(object sender, EventArgs e)
        {
            if (_alreadyLoaded)
                return;
            _alreadyLoaded = true;
            PopulateLayoutList();
            UpdateDisplay();
            _model.Load();

            _paperSizeCombo.Items.Clear();
            foreach (PaperTarget paperChoice in _model.PaperChoices)
            {
                _paperSizeCombo.Items.Add(paperChoice);
                if (_model.PaperTarget.Name == paperChoice.Name)
                {
                    _paperSizeCombo.SelectedItem = paperChoice;
                }
            }
        }

        public void UpdateDisplay()
        {
            /*_showCropMarks.Enabled=*/
            _mirrorBox.Enabled = _paperSizeCombo.Enabled = _model.SelectedMethod != null &&
          _model.SelectedMethod.GetType() != typeof(NullLayoutMethod);


            _overBrowserPanel.Visible = !_model.ShowBrowser;
            foreach (Button button in _layoutChoices.Controls)
            {
                var method = ((LayoutMethod)button.Tag);
                if (method.ImageIsSensitiveToOrientation)
                {
                    var originalImage = method.GetImage(LayoutMethod.IsLandscape(_model.InputPdf));
                    var resizedImage = ResizeImageForButton(originalImage);
                    originalImage.Dispose();

                    // Dispose old image if it exists
                    if (button.Image != null)
                    {
                        button.Image.Dispose();
                    }

                    button.Image = resizedImage;
                }
                button.Enabled = _model.HaveIncomingPdf && method.GetIsEnabled(_model.InputPdf);
                button.FlatAppearance.BorderSize = _model.SelectedMethod != null && method.GetType() == _model.SelectedMethod.GetType() ? 2 : 0;
            }
            SetupPreviousLink();
        }

        private void SetupPreviousLink()
        {
            _reloadPrevious.Visible = false;
            if (!string.IsNullOrEmpty(Settings.Default.PreviousIncomingPath) && File.Exists(Settings.Default.PreviousIncomingPath))
            {
                _reloadPrevious.Text = "Reopen " + Path.GetFileName(Settings.Default.PreviousIncomingPath);
                _reloadPrevious.Visible = true;
            }
        }

        private Image ResizeImageForButton(Image originalImage)
        {
            // Reserve space for text (approximately 30-40 pixels) and some padding
            int maxImageHeight = 80;
            int maxImageWidth = 70;
            // Calculate scale factors for both dimensions
            double scaleWidth = (double)maxImageWidth / originalImage.Width;
            double scaleHeight = (double)maxImageHeight / originalImage.Height;

            // Use the smaller scale factor to ensure the image fits within both constraints
            // This will make the image as large as possible while staying within the bounds
            double scale = Math.Min(scaleWidth, scaleHeight);

            int newWidth = (int)(originalImage.Width * scale);
            int newHeight = (int)(originalImage.Height * scale);

            // Create resized image
            var resizedImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        private void PopulateLayoutList()
        {
            _layoutChoices.BackColor = Color.White;
            _layoutChoices.Controls.Clear();
            _layoutChoices.RowCount = 0;
            _layoutChoices.RowStyles.Clear();
            foreach (LayoutMethod choice in _model.GetLayoutChoices())
            {
                var button = new Button();
                button.Tag = choice;
                button.Text = choice.ToString();

                // Get the original image and resize it to fit within the button
                var originalImage = choice.GetImage(LayoutMethod.IsLandscape(_model.InputPdf));
                var resizedImage = ResizeImageForButton(originalImage);
                originalImage.Dispose();

                button.Image = resizedImage;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                // item.Enabled = choice.GetIsEnabled(_model.IsLandscape);
                button.Click += new EventHandler((object sender, EventArgs e) => OnLayoutButtonClick((LayoutMethod)((Button)sender).Tag));
                button.Height = 130;
                button.Width = 80;// _layoutChoices.GetColumnWidths()[0] - 20;
                button.TextImageRelation = TextImageRelation.ImageAboveText;
                button.TextAlign = ContentAlignment.BottomCenter; // Align text to bottom
                button.ImageAlign = ContentAlignment.TopCenter; // Align image to top
                button.TabIndex = _layoutChoices.RowCount;
                _layoutChoices.Controls.Add(button);
                _layoutChoices.RowCount++;
                _layoutChoices.RowStyles.Add(new RowStyle(SizeType.Absolute, button.Height));

            }
        }

        void OnLayoutButtonClick(LayoutMethod method)
        {
            _model.SetLayoutMethod(method);
        }



        private void OnDragDrop(object sender, DragEventArgs e)
        {
            OnDragLeave(null, null);
            _overBrowserPanel.Visible = false;
            _model.SetPath(GetPathFromDropEvent(e));
        }

        private string GetPathFromDropEvent(DragEventArgs e)
        {
            Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
            if (a == null)
            {
                return null;
            }

            return a.GetValue(0).ToString();
        }


        private void OnDragEnter(object sender, DragEventArgs e)
        {
            _dragStatus.ForeColor = Color.Black;

            e.Effect = DragDropEffects.None;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string path = GetPathFromDropEvent(e);
            if (string.IsNullOrEmpty(path))
                return;

            if (_model.IsAlreadyOpenElsewhere(path))
            {
                _dragStatus.ForeColor = Color.Red;
                _dragStatus.Text = "That file appears to already be open in some other program. First close it, then drag it here.";
                _overBrowserPanel.Visible = true;
                e.Effect = DragDropEffects.None;
            }
            else if (Path.GetExtension(path).ToLower() == ".pdf")
            {
                _dragStatus.Text = "Looks good, drop it.";
                _overBrowserPanel.Visible = true;

                this.BackColor = Color.LightBlue;
                //this._dragStatus.BackColor = this.BackColor;
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                _dragStatus.ForeColor = Color.Red;
                _dragStatus.Text = "That file doesn't end in '.pdf'";
                _overBrowserPanel.Visible = true;
                e.Effect = DragDropEffects.None;
            }

        }

        private void OnDragLeave(object sender, EventArgs e)
        {
            _dragStatus.ForeColor = Color.Black;
            this.BackColor = SystemColors.Control;
            _dragStatus.Text = "Drag a PDF document here";
            _overBrowserPanel.Visible = !_model.ShowBrowser;
        }


        private void _linkChooseFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckPathExists = true;
            dialog.CheckFileExists = true;
            dialog.AddExtension = true;
            dialog.Filter = "PDF|*.pdf";

            if (!string.IsNullOrEmpty(Settings.Default.PreviousIncomingPath)
                && Directory.Exists(Path.GetDirectoryName(Settings.Default.PreviousIncomingPath)))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(Settings.Default.PreviousIncomingPath);
            }
            else
            {
                dialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            if (DialogResult.OK != dialog.ShowDialog())
            {
                return;
            }
            _model.SetPath(dialog.FileName);
        }

        private void OnInstructionsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var dlg = new InstructionsDialogBox())
            {
                dlg.ShowDialog();
            }
            Analytics.Track("Show Instructions");
        }

        private void _layoutChoices_VisibleChanged(object sender, EventArgs e)
        {
            //the Load event never fires, so...
            if (Visible && !DesignMode)
                OnLoad(null, null);
        }

        public void ClearThenContinue(Action callWhenDone)
        {
            _callWhenNavigated = callWhenDone;
            if (_browser.CoreWebView2 != null && _browser.Source != null && _browser.Source.AbsolutePath.Contains("pdf"))
            {
                _urlWeWereShowing = _browser.Source.AbsolutePath;
                _browser.CoreWebView2.Navigate("about:blank"); //stop holding on to the previous output
            }
            else
            {
                CallBackNow();
            }
        }

        private void CallBackNow()
        {
            if (_callWhenNavigated != null)
            {
                var copy = _callWhenNavigated;
                _callWhenNavigated = null;
                copy();
            }
        }

        public void Navigate(string path)
        {
            //WebView2 can display PDFs directly without Acrobat
            //PDF viewing parameters are not needed as WebView2 uses its own PDF viewer
            if (_browser.CoreWebView2 != null)
            {
                _browser.CoreWebView2.Navigate(path);
            }
        }

        private void _reloadPrevious_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _model.ReloadPrevious();
        }

        private void _browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_callWhenNavigated == null)
                return;
            DateTime end = DateTime.Now.AddSeconds(3);
            while (!CanWrite(_urlWeWereShowing) && DateTime.Now < end)
            {
                Thread.Sleep(100);
            }

            CallBackNow();
        }

        public bool CanWrite(string path)
        {
            try
            {
                using (FileStream strm = File.OpenWrite(path))
                {
                    strm.Close();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void _paperSizeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_paperSizeCombo.SelectedItem != null)
                _model.SetPaperTarget(_paperSizeCombo.SelectedItem as PaperTarget);
        }

        private void _printLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // _model.Print();
        }


        private void OnRightToLeft_CheckedChanged(object sender, EventArgs e)
        {
            _model.SetRightToLeft(_rightToLeft.Checked);
        }

        private void OnMirrorBox_CheckedChanged(object sender, EventArgs e)
        {
            _model.SetMirror(_mirrorBox.Checked);
        }

        private void OnShowCropMarks_CheckedChanged(object sender, EventArgs e)
        {
            _model.ShowCropMarks(_showCropMarks.Checked);
        }


    }
}
