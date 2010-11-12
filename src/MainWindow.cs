using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Palaso.IO;
using Palaso.Reporting;
using PdfDroplet.Properties;


namespace PdfDroplet
{
    public partial class MainWindow : Form
    {
        private string _resultingPdfPath;

        public MainWindow(bool showAbout)
        {
            InitializeComponent();
            Font = SystemFonts.DialogFont;

             SetWindowText();
            timer1.Enabled = showAbout;
            _instructionsBrowser.Navigate(FileLocator.GetFileDistributedWithApplication("instructions.htm"));
            _browserForPdf.Navigated +=new WebBrowserNavigatedEventHandler((x,y)=>_stillNavigating = false);

            _preservePageSizeButton.Checked = true; //default if no user settings
            _shrinkPageButton.Checked = false;
            _rightToLeft.Checked = Settings.Default.RightToLeft;

            switch (Settings.Default.PaperTargetChoice)
            {
                case DoublePaperTarget.StaticName:
                    _preservePageSizeButton.Checked = true;
                    break;
                case SameSizePaperTarget.StaticName:
                    _shrinkPageButton.Checked = true;
                    break;
            }
        }

      

        private void SetWindowText()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Text = string.Format("Pdf Droplet: {0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
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
                
            if (IsAlreadyOpenElsewhere(path))
            {
                _dragStatus.ForeColor = Color.Red;
                _dragStatus.Text = "That file appears to already be open in some other program. First close it, then drag it here.";
                _dragStatus.Visible = true;
                e.Effect = DragDropEffects.None; 
            }
            else if (Path.GetExtension(path).ToLower()==".pdf")
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

        private bool IsAlreadyOpenElsewhere(string path)
        {
            try
            {
                using (FileStream strm = File.OpenRead(path))
                {
                    strm.Close();
                    return false;
                }
            }
            catch (Exception e)
            {
                return true;
            }
        }

        private void OnDragLeave(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            this.BackColor = SystemColors.Control;
            UpdateConvertTabDisplay();
            UpdateReviewTapDisplay();
        }

        private void UpdateConvertTabDisplay()
        {
            _dragStatus.Visible = false;   
            UpdateLinkForConvertingPrevious();
            _dragStatus.MaximumSize = new Size(_convertPage.Width - 70, _dragStatus.MaximumSize.Height);
        }


        private void UpdateLinkForConvertingPrevious()
        {
            bool doShowIt = !string.IsNullOrEmpty(Settings.Default.PreviousIncomingPath)
                            && File.Exists(Settings.Default.PreviousIncomingPath);
            
            //nb: this doesn't work if the tab that holds this link isn't the selected one
            //when this code runs
            _labelConvertPrevious.Visible = doShowIt;
            _linkConvertPrevious.Visible = doShowIt;
            if (doShowIt)
            {
                _linkConvertPrevious.Text =
                    String.Format("Convert {0} again.", Path.GetFileName(Settings.Default.PreviousIncomingPath));
            }
        }

        private void UpdateReviewTapDisplay()
        {
           // _output.Visible = _fileLoaded;
            _resultingFileLink.Visible = File.Exists(_resultingPdfPath);

            if (_resultingPdfPath != null)
            {
                _resultingFileLink.Text = _resultingPdfPath;
            }
        }

        private bool _stillNavigating;

        private PaperTarget ChosenPaperChoice
        {
            get
            {
                if (_shrinkPageButton.Checked)
                    return new SameSizePaperTarget();
                else
                {
                    return new DoublePaperTarget();
                }
            }
        }

        private bool Convert(string path)
        {
            Settings.Default.RightToLeft = _rightToLeft.Checked;
            Settings.Default.PaperTargetChoice = ChosenPaperChoice.Name;
            Settings.Default.Save();

            //avoid the situation where we try to over-write but can't
            if (_browserForPdf.Url !=null  && _browserForPdf.Url.AbsolutePath.Contains("pdf"))
            {
                _stillNavigating = true;
                _browserForPdf.Navigate("about:blank"); //stop holding on to the previous output
                while(_stillNavigating)
                {
                    Application.DoEvents();//let the navigation happen
                }
                Thread.Sleep(1000);//the waiting for navigate doesn't seem to be enough
            }

            if (IsAlreadyOpenElsewhere(path))
            {
                ErrorReport.NotifyUserOfProblem("That file appears to be open. First close it, then try again.");
                return false;
            }
            try
            {
                RunConverter(path);

                if (Settings.Default.PreviousIncomingPath != path)
                {
                    Settings.Default.PreviousIncomingPath = path;
                }
            }
            catch (Exception error)
            {
                ErrorReport.NotifyUserOfProblem(error,"PdfBooket was unable to convert that file.");
                return false;
            }
            UpdateDisplay();
            return true;
        }

        private bool RunConverter(string path)
        {
            _resultingPdfPath =  Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path) + "-booklet.pdf");
            var converter = new Converter();
            converter.Convert(path, _resultingPdfPath, ChosenPaperChoice, _rightToLeft.Checked);
            
            _browserForPdf.Navigate(_resultingPdfPath);
    
            return true;
        }

       
    

        private void OnDragDrop(object sender, DragEventArgs e)
        {
          string path = GetPathFromDropEvent(e);

           if (String.IsNullOrEmpty(path))
           {
               return;
           }
            _dragStatus.Text = "Converting...";
           _dragStatus.Invalidate();
           // Extract string from first array element
           // (ignore all files except first if number of files are dropped).
           if (Convert(path))
           {
               _tabControl.SelectedTab = _bookletPage;
           }
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

        private void OnLoad(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

   

        private void _linkConvertPrevious_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Convert(Settings.Default.PreviousIncomingPath))
            {
                _tabControl.SelectedTab = _bookletPage;
            }
        }

        private void tabPage1_Resize(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

//        private void _linkConvertAndSave_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
//        {
//            if (Convert(Settings.Default.PreviousIncomingPath))
//            {
//                Save(Settings.Default.PreviousSavePath);
//                _labelDone.Visible = true;
//            }
//        }

 
        private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
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
            if(Convert(dialog.FileName))
            {
                _tabControl.SelectedTab = _bookletPage;
            }
        }

        private void _useDropletRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void _resultingFileLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(_resultingPdfPath);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using(var dlg = new AboutBox1())
            {
                dlg.ShowDialog();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            using (var dlg = new AboutBox1())
            {
                dlg.ShowDialog();
            }
        }
    }
}