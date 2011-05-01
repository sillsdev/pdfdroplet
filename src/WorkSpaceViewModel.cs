using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Palaso.Reporting;
using PdfDroplet.Properties;
using PdfSharp.Drawing;

namespace PdfDroplet
{
    class WorkSpaceViewModel
    {
        private WorkspaceControl _view;
        private string _incomingPath;
        private LayoutMethod _layoutMethod;
        private XPdfForm _inputPdf;

        public WorkSpaceViewModel(WorkspaceControl workspaceControl)
        {
            _view = workspaceControl;
            PaperTarget = new DoublePaperTarget();
        }

        public bool IsAlreadyOpenElsewhere(string path)
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

        public void SetRightToLeft(bool rightToLeft)
        {
            Settings.Default.RightToLeft = rightToLeft;
            Settings.Default.Save();   
        }

        public IEnumerable<LayoutMethod> GetLayoutChoices()
        {
            yield return new NullLayoutMethod();
            yield return new SideFoldBooklet();
            yield return new CalendarLayouter();
            yield return new CutLandscapeLayout();
        }

        public bool IsLandscape { get { return _inputPdf != null && _inputPdf.PixelWidth > _inputPdf.PixelHeight; } }

        public bool ShowBrowser
        {
            get { return !string.IsNullOrEmpty(_incomingPath) && File.Exists(_incomingPath); }
        }

        public void LoadPrevious()
        {
            SetPath(Settings.Default.PreviousIncomingPath);
        }

        public void SetPath(string path)
        {
            _incomingPath = path;
            _inputPdf = OpenDocumentForPdfSharp(_incomingPath);
            SetLayoutMethod(new NullLayoutMethod());
        }

        public void SetLayoutMethod(LayoutMethod method)
        {
            _layoutMethod = method;
            if (HaveIncomingPdf)
            {
                if (method is NullLayoutMethod)
                {
                    _view.Navigate(_incomingPath);
                }
                else
                {
                    Convert();
                }
            }
            _view.UpdateDisplay();
        }

        protected bool HaveIncomingPdf  
        {
            get { return !string.IsNullOrEmpty(_incomingPath) && File.Exists(_incomingPath); }
        }

        public string PathToDisplayInBrowser {get; private set;}
        protected PaperTarget PaperTarget { get; set; }

        private void Convert()
        {
            //avoid the situation where we try to over-write but can't
            _view.ClearBrowser(Convert2);
        }
        private void Convert2()
        {
            if (IsAlreadyOpenElsewhere(_incomingPath))
            {
                ErrorReport.NotifyUserOfProblem("That file appears to be open. First close it, then try again.");
            }
            try
            {
                var outPath = Path.Combine(Path.GetDirectoryName(_incomingPath), Path.GetFileNameWithoutExtension(_incomingPath) + "-booklet.pdf");

                _layoutMethod.Layout(_incomingPath, outPath, PaperTarget, Settings.Default.RightToLeft, _inputPdf);
                 _view.Navigate(outPath);      

                if (Settings.Default.PreviousIncomingPath != _incomingPath)
                {
                    Settings.Default.PreviousIncomingPath = _incomingPath;
                }
            }
            catch (Exception error)
            {
                ErrorReport.NotifyUserOfProblem(error, "PdfBooket was unable to convert that file.");

            }
            _view.UpdateDisplay();
         
        }



        /// <summary>
        /// from http://forum.pdfsharp.net/viewtopic.php?p=2069
        /// Get a version of the document which pdfsharp can open, downgrading if necessary
        /// </summary>
        static private XPdfForm OpenDocumentForPdfSharp(string path)
        {
            try
            {
                var form = XPdfForm.FromFile(path);
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
            string outputPath = tempFileName + ".pdf";

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

        public void Load()
        {
           ReloadPrevious();
        }

        public void ReloadPrevious()
        {
            SetPath(Settings.Default.PreviousIncomingPath);
        }
    }
}
