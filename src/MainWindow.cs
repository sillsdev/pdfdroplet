using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Palaso.IO;

namespace PdfDroplet
{
    public partial class MainWindow : Form
    {
        private const string _busyMessage = "That file appears to already be open, probably in Excel. First close it, then drag it here.";
        private bool _fileLoaded=false;
        private string _resultingPdfPath;

        public MainWindow()
        {
            InitializeComponent();
            Font = SystemFonts.DialogFont;
            _useAcrobatRadio.Font = new Font(SystemFonts.DialogFont.FontFamily, 10);
            _useDropletRadio.Font = _useAcrobatRadio.Font;
            _useAcrobatInstead.Font = _useAcrobatRadio.Font; 
            SetWindowText();
            webBrowser1.Navigate(FileLocator.GetFileDistributedWithApplication("about.htm"));
        }



        private void SetWindowText()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Text = string.Format("Pdf Droplet: {0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings1.Default.Save();
            base.OnClosing(e);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string path = GetPathFromDropEvent(e);
            if (string.IsNullOrEmpty(path))
                return;
                
            if (IsAlreadyOpenElsewhere(path))
            {

                _dragStatus.Text = _busyMessage;
                _dragStatus.Visible = true;
                return;
            }

            this.BackColor = Color.Cornsilk;
            //this._dragStatus.BackColor = this.BackColor;
            e.Effect = DragDropEffects.Copy;
            
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
            _useAcrobatInstead.Visible = _useAcrobatRadio.Checked;
            _dragStatus.Visible = false;
           // _linkChooseFile.Enabled = false;           
            UpdateLinkForConvertingPrevious();
            UpdateLinkForConvertAndSave();
            _dragStatus.MaximumSize = new Size(_convertPage.Width - 70, _dragStatus.MaximumSize.Height);
        }

        private void UpdateLinkForConvertAndSave()
        {
            _labelDone.Visible = false;
            _linkConvertAndSave.Visible = _linkConvertPrevious.Visible 
                                          && !string.IsNullOrEmpty(Settings1.Default.PreviousSavePath)
                                          && Directory.Exists(Path.GetDirectoryName(Settings1.Default.PreviousSheetPath));

            if (_linkConvertAndSave.Visible)
            {
                _linkConvertAndSave.Text =
                    String.Format("Convert {0} to {1} without asking any questions.", Path.GetFileName(Settings1.Default.PreviousSheetPath), Path.GetFileName(Settings1.Default.PreviousSavePath));
            }
            _labelOrForConvertAndSave.Visible = _linkConvertAndSave.Visible;
        }

        private void UpdateLinkForConvertingPrevious()
        {
            bool doShowIt = !string.IsNullOrEmpty(Settings1.Default.PreviousSheetPath)
                            && File.Exists(Settings1.Default.PreviousSheetPath);
            
            //nb: this doesn't work if the tab that holds this link isn't the selected one
            //when this code runs
            _linkConvertPrevious.Visible = doShowIt;
            if (doShowIt)
            {
                _linkConvertPrevious.Text =
                    String.Format("Convert {0}", Path.GetFileName(Settings1.Default.PreviousSheetPath));
            }
            _labelOrForConvert.Visible = _linkConvertPrevious.Visible;
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

        private bool Convert(string path)
        {
            if (IsAlreadyOpenElsewhere(path))
            {
                MessageBox.Show("That file appears to be open. First close it, then try again.","Whoops", MessageBoxButtons.OK);
                return false;
            }
            try
            {
                _fileLoaded = RunConverter(path);

                if (Settings1.Default.PreviousSheetPath != path)
                {
                    Settings1.Default.PreviousSheetPath = path;
                    //the source has changed, so don't assume we want to go to the same target
                    Settings1.Default.PreviousSavePath  = null;

                }
            }
            catch (Exception error)
            {
                MessageBox.Show("PdfBooket was unable to convert that file.\r\n"+error.Message, "Whoops", MessageBoxButtons.OK);
                return false;
            }
            UpdateDisplay();
            return true;
        }

        private bool RunConverter(string path)
        {
            var temp = Path.GetTempFileName();
            File.Copy(path, temp, true);
            var info = new ProcessStartInfo();
            info.WorkingDirectory = Path.GetDirectoryName(path);
            info.FileName = FileLocator.GetFileDistributedWithApplication("pdfbklt.exe");
            info.Arguments = '"'+temp+'"';
            info.CreateNoWindow = true;
            var x = System.Diagnostics.Process.Start(info);
            x.WaitForExit(20*1000);
            if(x.ExitCode > 0)
            {
                webBrowser1.DocumentText = x.StandardError.ReadToEnd();
            }
            else
            {
                _resultingPdfPath =  Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path) + "-booklet.pdf");
                File.Copy(temp, _resultingPdfPath, true);
                webBrowser1.Navigate(_resultingPdfPath);
            }
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
               _tabControl.SelectedTab = _reviewPage;
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
            if (Convert(Settings1.Default.PreviousSheetPath))
            {
                _tabControl.SelectedTab = _reviewPage;
            }
        }

        private void tabPage1_Resize(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

//        private void _linkConvertAndSave_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
//        {
//            if (Convert(Settings1.Default.PreviousSheetPath))
//            {
//                Save(Settings1.Default.PreviousSavePath);
//                _labelDone.Visible = true;
//            }
//        }

        private void MainWindow_Deactivate(object sender, EventArgs e)
        {
            //clear this once they switch to another app
            _labelDone.Visible = false;
        }

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

            if (!string.IsNullOrEmpty(Settings1.Default.PreviousSheetPath)
                && Directory.Exists(Path.GetDirectoryName(Settings1.Default.PreviousSheetPath)))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(Settings1.Default.PreviousSheetPath);              
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
                _tabControl.SelectedTab = _reviewPage;
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
    }
}