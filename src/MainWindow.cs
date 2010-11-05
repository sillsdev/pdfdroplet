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
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

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
            //webBrowser1.Navigate(FileLocator.GetFileDistributedWithApplication("about.htm"));
            timer1.Enabled = showAbout;
            _instructionsBrowser.Navigate(FileLocator.GetFileDistributedWithApplication("instructions.htm"));
            _browserForPdf.Navigated +=new WebBrowserNavigatedEventHandler((x,y)=>_stillNavigating = false);
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

        private bool Convert(string path)
        {
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
            ConvertUsingPdfSharp(path, _resultingPdfPath);
            
            _browserForPdf.Navigate(_resultingPdfPath);
    
            return true;
        }

        private void ConvertUsingPdfSharp(string inputPath, string outputPath)
        {
            PdfDocument outputDocument = new PdfDocument();

            // Show single pages
            // (Note: one page contains two pages from the source document.
            //  If the number of pages of the source document can not be
            //  divided by 4, the first pages of the output document will
            //  each contain only one page from the source document.)
            outputDocument.PageLayout = PdfPageLayout.SinglePage;

            XGraphics gfx;

            // Open the external document as XPdfForm object
            XPdfForm form = OpenDocumentForPdfSharp(inputPath);
            // Determine width and height
            double extWidth = form.PixelWidth;
            double extHeight = form.PixelHeight;

            int inputPages = form.PageCount;
            int sheets = inputPages/4;
            if (sheets*4 < inputPages)
                sheets += 1;
            int allpages = 4*sheets;
            int vacats = allpages - inputPages;

            for (int idx = 1; idx <= sheets; idx += 1)
            {
                // Front page of a sheet:
                // Add a new page to the output document
                PdfPage page = outputDocument.AddPage();
                page.Orientation = PageOrientation.Landscape;
                page.Width = 2*extWidth;
                page.Height = extHeight;
                double width = page.Width;
                double height = page.Height;

                gfx = XGraphics.FromPdfPage(page);

                // Skip if left side has to remain blank
                XRect box;
                if (vacats > 0)
                    vacats -= 1;
                else
                {
                    // Set page number (which is one-based) for left side
                    form.PageNumber = allpages + 2*(1 - idx);
                    box = new XRect(0, 0, width/2, height);
                    // Draw the page identified by the page number like an image
                    gfx.DrawImage(form, box);
                }

                // Set page number (which is one-based) for right side
                form.PageNumber = 2*idx - 1;
                box = new XRect(width/2, 0, width/2, height);
                // Draw the page identified by the page number like an image
                gfx.DrawImage(form, box);

                // Back page of a sheet
                page = outputDocument.AddPage();
                page.Orientation = PageOrientation.Landscape;
                page.Width = 2*extWidth;
                page.Height = extHeight;

                gfx = XGraphics.FromPdfPage(page);

                if(2*idx <= form.PageCount) //prevent asking for page 2 with a single page document (JH Oct 2010)
                {
                    // Set page number (which is one-based) for left side
                    form.PageNumber = 2*idx;
                    box = new XRect(0, 0, width/2, height);
                    // Draw the page identified by the page number like an image
                    gfx.DrawImage(form, box);
                }

                // Skip if right side has to remain blank
                if (vacats > 0)
                    vacats -= 1;
                else
                {
                    // Set page number (which is one-based) for right side
                    form.PageNumber = allpages + 1 - 2*idx;
                    box = new XRect(width/2, 0, width/2, height);
                    // Draw the page identified by the page number like an image
                    gfx.DrawImage(form, box);
                }
            }

             outputDocument.Save(outputPath);
        }

        /// <summary>
        /// from http://forum.pdfsharp.net/viewtopic.php?p=2069
        /// Get a version of the document which pdfsharp can open, downgrading if necessary
        /// </summary>
        static public XPdfForm OpenDocumentForPdfSharp(string path)
        {
            try 
            {
                var form= XPdfForm.FromFile(path);
                //this causes it to notice if can't actually read it
                int dummy = form.PixelWidth;
                return form;
            }
            catch (PdfSharp.Pdf.IO.PdfReaderException) 
            {
                //workaround if pdfsharp doesnt dupport this pdf
                return XPdfForm.FromFile(WritePdf1pt4Version(path));
            }
        }


        /// <summary>
        /// from http://forum.pdfsharp.net/viewtopic.php?p=2069
        /// uses itextsharp to convert any pdf to 1.4 compatible pdf
        /// </summary>
        static private string WritePdf1pt4Version(string inputPath)
        {
            var tempFileName = Path.GetTempFileName();
            File.Delete(tempFileName);
            string outputPath = tempFileName+ ".pdf";

            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(inputPath);

            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // step 1: creation of a document-object
            iTextSharp.text.Document document = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            // step 2: we create a writer that listens to the document
            iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));
            //write pdf that pdfsharp can understand
            writer.SetPdfVersion(iTextSharp.text.pdf.PdfWriter.PDF_VERSION_1_4);
            // step 3: we open the document
            document.Open();
            iTextSharp.text.pdf.PdfContentByte cb = writer.DirectContent;
            iTextSharp.text.pdf.PdfImportedPage page;

            int rotation;

            int i = 0;
            while (i < n)
            {
                i++;
                document.SetPageSize(reader.GetPageSizeWithRotation(i));
                document.NewPage();
                page = writer.GetImportedPage(reader, i);
                rotation = reader.GetPageRotation(i);
                if (rotation == 90 || rotation == 270)
                {
                    cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                }
                else
                {
                    cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                }
            }
            // step 5: we close the document
            document.Close();
            return outputPath;
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

//        private void _instructionsBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
//        {
//            if(_instructionsBrowser.Document == null)
//            {
//                _instructionsBrowser.Navigate(FileLocator.GetFileDistributedWithApplication("instructions.htm"));
//            }
//        }

    }
}