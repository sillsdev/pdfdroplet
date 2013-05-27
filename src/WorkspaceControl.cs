using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DesktopAnalytics;
using PdfDroplet.LayoutMethods;
using PdfDroplet.Properties;

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
            //important to do this after the above settings
            this._mirrorBox.CheckedChanged += new System.EventHandler(this.OnMirrorBox_CheckedChanged);
            this._rightToLeft.CheckedChanged += new System.EventHandler(this.OnRightToLeft_CheckedChanged);
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
                if(_model.PaperTarget.Name == paperChoice.Name)
                {
                    _paperSizeCombo.SelectedItem = paperChoice;
                }
            }
        }

        public void UpdateDisplay()
        {
            _mirrorBox.Enabled = _paperSizeCombo.Enabled = _model.SelectedMethod != null &&
                                      _model.SelectedMethod.GetType() != typeof(NullLayoutMethod);

            
            _overBrowserPanel.Visible = !_model.ShowBrowser;
            foreach (Button button in _layoutChoices.Controls)
            {
                var method = ((LayoutMethod)button.Tag);
                if(method.ImageIsSensitiveToOrientation)
                {
                    button.Image = method.GetImage(_model.IsLandscape);
                }
                button.Enabled = _model.HaveIncomingPdf && method.GetIsEnabled(_model.IsLandscape);
                button.FlatAppearance.BorderSize = _model.SelectedMethod!=null && method.GetType() == _model.SelectedMethod.GetType() ? 2 : 0;
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

        private void PopulateLayoutList()
        {
            this.BackColor = SystemColors.Control;
            _layoutChoices.Controls.Clear();
            _layoutChoices.RowCount = 0;
            _layoutChoices.RowStyles.Clear();
            foreach (LayoutMethod choice in _model.GetLayoutChoices())
            {
                var button = new Button();
                button.Tag = choice;
                button.Text = choice.ToString();
                button.Image = choice.GetImage(_model.IsLandscape);
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
               // item.Enabled = choice.GetIsEnabled(_model.IsLandscape);
                button.Click += new EventHandler((object sender, EventArgs e) => OnLayoutButtonClick((LayoutMethod)((Button)sender).Tag));
                button.Height = 100;
                button.Width = 80;// _layoutChoices.GetColumnWidths()[0] - 20;
                button.TextImageRelation = TextImageRelation.ImageAboveText;
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
            OnDragLeave(null,null);
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


        private void OnAboutLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
			using (var dlg = new AboutBox())
            {
                dlg.ShowDialog();
            }
			Analytics.Track("Show Instructions");
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
            if(Visible && !DesignMode)
                OnLoad(null,null);
        }
//
//        private void timer1_Tick(object sender, EventArgs e)
//        {
//            timer1.Enabled = false;
//            using (var dlg = new AboutBox1())
//            {
//                dlg.ShowDialog();
//            }
//        }

        public void ClearThenContinue(Action callWhenDone)
        {
            _callWhenNavigated = callWhenDone;
            if (_browser.Url != null && _browser.Url.AbsolutePath.Contains("pdf"))
            {
                _urlWeWereShowing = _browser.Url.AbsolutePath;
                _browser.Navigate("about:blank"); //stop holding on to the previous output
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
            //http://wwwimages.adobe.com/www.adobe.com/content/dam/Adobe/en/devnet/acrobat/pdfs/pdf_open_parameters.pdf
            path += "#view=Fit&navpanes=0&pagemode=thumbs&toolbar=1";
            _browser.Navigate(path);
        }

        private void _reloadPrevious_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _model.ReloadPrevious();
        }

        private void _browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
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
            catch (Exception e)
            {
                return false;
            }
        }

        private void _paperSizeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(_paperSizeCombo.SelectedItem!=null)
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


    }
}
