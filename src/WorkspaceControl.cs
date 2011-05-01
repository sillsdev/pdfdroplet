using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
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
            _dragStatus.Text = "Drag a PDF document here";
        }

        private void OnRightToLeft_CheckedChanged(object sender, EventArgs e)
        {
            _model.SetRightToLeft(_rightToLeft.Checked);
        }


        private void OnLoad(object sender, EventArgs e)
        {
            if (_alreadyLoaded)
                return;
            _alreadyLoaded = true;
            UpdateDisplay();
            _model.Load();
        }

        public void UpdateDisplay()
        {
            _browser.Visible = _model.ShowBrowser;
            _dragStatus.Visible = !_browser.Visible;

            this.BackColor = SystemColors.Control;
            //UpdateConvertTabDisplay();
            //UpdateReviewTapDisplay();
            //foreach (var control in _layoutChoices.Controls)

            _layoutChoices.Controls.Clear();
            _layoutChoices.RowCount=0;
            _layoutChoices.RowStyles.Clear();

            //_layoutChoices.RowCount = 2;
            foreach (LayoutMethod choice in _model.GetLayoutChoices())
            {
                var item = new Button();
                item.Tag = choice;
                item.Text = choice.ToString();
                item.Image = choice.Image;
                item.FlatStyle = FlatStyle.Flat;
                item.FlatAppearance.BorderSize = 0;
                item.Enabled = choice.GetIsEnabled(_model.IsLandscape);
                item.Click +=new EventHandler((object sender, EventArgs e)=>OnLayoutButtonClick((LayoutMethod)((Button)sender).Tag));
                item.Height = 100;
                item.Width = 80;// _layoutChoices.GetColumnWidths()[0] - 20;
                item.TextImageRelation = TextImageRelation.ImageAboveText;
                item.TabIndex = _layoutChoices.RowCount;
                _layoutChoices.Controls.Add(item); 
                _layoutChoices.RowCount++;
                _layoutChoices.RowStyles.Add(new RowStyle(SizeType.Absolute,  item.Height));
                
            }
        }

        void OnLayoutButtonClick(LayoutMethod method)
        {
            _model.SetLayoutMethod(method);
        }

     

        private void OnDragDrop(object sender, DragEventArgs e)
        {
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
                _dragStatus.Visible = true;
                e.Effect = DragDropEffects.None;
            }
            else if (Path.GetExtension(path).ToLower() == ".pdf")
            {
                _dragStatus.Text = "Looks good, drop it.";
                _dragStatus.Visible = true;

                this.BackColor = Color.LightBlue;
                //this._dragStatus.BackColor = this.BackColor;
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                _dragStatus.ForeColor = Color.Red;
                _dragStatus.Text = "That file doesn't end in '.pdf'";
                _dragStatus.Visible = true;
                e.Effect = DragDropEffects.None;
            }

        }


        private void OnDragLeave(object sender, EventArgs e)
        {
            UpdateDisplay();
        }



        private void UpdateConvertTabDisplay()
        {
//            _dragStatus.Visible = false;
//            UpdateLinkForConvertingPrevious();
//            _dragStatus.MaximumSize = new Size(_convertPage.Width - 70, _dragStatus.MaximumSize.Height);
        }


        private void UpdateLinkForConvertingPrevious()
        {
//            bool doShowIt = !string.IsNullOrEmpty(Settings.Default.PreviousIncomingPath)
//                            && File.Exists(Settings.Default.PreviousIncomingPath);
//
            //nb: this doesn't work if the tab that holds this link isn't the selected one
            //when this code runs
//            _labelConvertPrevious.Visible = doShowIt;
//            _linkConvertPrevious.Visible = doShowIt;
//            if (doShowIt)
//            {
//                _linkConvertPrevious.Text =
//                    String.Format("Convert {0} again.", Path.GetFileName(Settings.Default.PreviousIncomingPath));
//            }
        }


        //        private void _linkConvertAndSave_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //        {
        //            if (Convert(Settings.Default.PreviousIncomingPath))
        //            {
        //                Save(Settings.Default.PreviousSavePath);
        //                _labelDone.Visible = true;
        //            }
        //        }


//        private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            UpdateDisplay();
//        }

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
            using (var dlg = new AboutBox1())
            {
                dlg.ShowDialog();
            }
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

        public void ClearBrowser(Action callWhenDone)
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
    }
}
