using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Threading;
using SIL.Reporting;
using PdfDroplet.LayoutMethods;
using PdfDroplet.Properties;
using PdfSharp;
using PdfSharp.Drawing;

namespace PdfDroplet
{
    class WorkSpaceViewModel
    {
        private WorkspaceControl _view;
        private string _incomingPath;
        private XPdfForm _inputPdf;
        private string _pathToCurrentlyDisplayedPdf;
        private const int PreviewHistoryLimit = 2;
        private readonly List<string> _generatedPreviewPaths = new List<string>();

        public WorkSpaceViewModel(WorkspaceControl workspaceControl)
        {
            _view = workspaceControl;
            //default to whatever the printer's default is
            PrinterSettings printer = new System.Drawing.Printing.PrinterSettings();
            PaperTarget = new PaperTarget(printer.DefaultPageSettings.PaperSize.PaperName, printer.DefaultPageSettings.PaperSize);
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
            catch (Exception)
            {
                return true;
            }
        }

        public void SetRightToLeft(bool rightToLeft)
        {
            Settings.Default.RightToLeft = rightToLeft;
            Settings.Default.Save();
            SetLayoutMethod(SelectedMethod);
        }


        public void ShowCropMarks(bool showCropMarks)
        {
            Settings.Default.ShowCropMarks = showCropMarks;
            Settings.Default.Save();

            SetLayoutMethod(SelectedMethod);//cause to re-do it with this setting
        }

        public void SetMirror(bool doMirror)
        {
            Settings.Default.Mirror = doMirror;
            //Settings.Default.Save();
            SetLayoutMethod(SelectedMethod);

            //not sure I want to save it with it on, just yet
            //Settings.Default.Mirror = false;
        }

        public IEnumerable<LayoutMethod> GetLayoutChoices()
        {
            yield return new NullLayoutMethod();
            yield return new SideFoldBookletLayouter();
            yield return new CalendarLayouter();
            yield return new CutLandscapeLayout();
            yield return new SideFold4UpBookletLayouter();
            yield return new SideFold4UpSingleBookletLayouter();
            yield return new Folded8Up8PageBookletLayouter();
            yield return new Square6UpBookletLayouter();
        }

        internal XPdfForm InputPdf => _inputPdf;

        public bool ShowBrowser
        {
            get { return !string.IsNullOrEmpty(_incomingPath) && File.Exists(_incomingPath); }
        }


        public void SetPath(string path)
        {
            _incomingPath = path;
            _inputPdf = OpenDocumentForPdfSharp(_incomingPath);
            SetLayoutMethod(new NullLayoutMethod());
        }

        //        public void Print()
        //        {
        //            try
        //            {
        //                PdfSharp.Pdf.PdfDocument x;
        //                PdfFilePrinter y = new PdfFilePrinter(_pathToCurrentlyDisplayedPdf);
        //                PdfFilePrinter.AdobeReaderPath = 
        //                y.Print();
        //            }
        //            catch(Exception e)
        //            {
        //                ErrorReport.NotifyUserOfProblem(e, "Could not print");
        //            }
        //        }

        public void SetLayoutMethod(LayoutMethod method)
        {
            SelectedMethod = method;
            if (HaveIncomingPdf)
            {
                ContinueConversionAndNavigation();
            }
            if (!string.IsNullOrEmpty(_incomingPath) && Settings.Default.PreviousIncomingPath != _incomingPath)
            {
                Settings.Default.PreviousIncomingPath = _incomingPath;
                Settings.Default.Save();
            }
        }

        public bool HaveIncomingPdf
        {
            get { return !string.IsNullOrEmpty(_incomingPath) && File.Exists(_incomingPath); }
        }

        public string PathToDisplayInBrowser { get; private set; }
        public PaperTarget PaperTarget { get; private set; }
        public void SetPaperTarget(PaperTarget target)
        {
            if (PaperTarget.Name != target.Name)
            {
                PaperTarget = target;
                SetLayoutMethod(SelectedMethod);
            }
        }

        public LayoutMethod SelectedMethod
        {
            get; private set;
        }

        public IEnumerable<PaperTarget> PaperChoices
        {
            get
            {
                yield return new PaperTarget("A4", PageSize.A4);
                yield return new PaperTarget("A3", PageSize.A3);
                yield return new PaperTarget("Letter", PageSize.Letter);
                yield return new PaperTarget("Legal", PageSize.Legal);
                yield return new PaperTarget("Foolscap", PageSize.Foolscap);
                //                yield return new SameSizePaperTarget();
                //                yield return new DoublePaperTarget();
            }
        }



        private void ContinueConversionAndNavigation()
        {
            if (IsAlreadyOpenElsewhere(_incomingPath))
            {
                ErrorReport.NotifyUserOfProblem("That file appears to be open. First close it, then try again.");
                return;
            }

            var outputPath = CreatePreviewOutputPath();

            try
            {
                SelectedMethod.Layout(
                    _inputPdf,
                    _incomingPath,
                    outputPath,
                    PaperTarget,
                    Settings.Default.RightToLeft,
                    Settings.Default.ShowCropMarks);

                _pathToCurrentlyDisplayedPdf = outputPath;
                TrackGeneratedPreview(outputPath);
            }
            catch (Exception error)
            {
                TryDeletePreview(outputPath);
                ErrorReport.NotifyUserOfProblem(error, "PdfBooklet was unable to convert that file.");
            }
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

        internal static string GetPreviewDirectory()
        {
            var previewsDirectory = Path.Combine(Path.GetTempPath(), "PdfDroplet", "Previews");
            Directory.CreateDirectory(previewsDirectory);
            return previewsDirectory;
        }

        private string CreatePreviewOutputPath()
        {
            var baseName = SanitizeFileNameSegment(Path.GetFileNameWithoutExtension(_incomingPath));
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            var previewsDirectory = GetPreviewDirectory();
            return Path.Combine(previewsDirectory, $"{baseName}-{timestamp}-booklet.pdf");
        }

        private static string SanitizeFileNameSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "PdfDroplet";
            }

            var characters = value.Trim().ToCharArray();
            var invalidChars = Path.GetInvalidFileNameChars();
            var hasAlphaNumeric = false;

            for (int i = 0; i < characters.Length; i++)
            {
                var ch = characters[i];
                if (Array.IndexOf(invalidChars, ch) >= 0)
                {
                    characters[i] = '_';
                }
                else if (!char.IsWhiteSpace(ch))
                {
                    hasAlphaNumeric = true;
                }
            }

            var sanitized = new string(characters).Trim('_');
            return string.IsNullOrEmpty(sanitized) || !hasAlphaNumeric ? "PdfDroplet" : sanitized;
        }

        private void TrackGeneratedPreview(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            _generatedPreviewPaths.Add(path);

            while (_generatedPreviewPaths.Count > PreviewHistoryLimit)
            {
                var obsolete = _generatedPreviewPaths[0];
                _generatedPreviewPaths.RemoveAt(0);
                TryDeletePreview(obsolete);
            }
        }

        private void TryDeletePreview(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                DeleteFileThatMayBeInUse(path);
            }
            catch
            {
                // Ignore cleanup failures; temp files will eventually be reclaimed.
            }
        }

        public void DisposeGeneratedPreviews()
        {
            foreach (var path in _generatedPreviewPaths)
            {
                TryDeletePreview(path);
            }

            _generatedPreviewPaths.Clear();
        }

        public void Load()
        {
            // PaperTarget = PaperChoices.First();
            //  ReloadPrevious();
        }

        public void ReloadPrevious()
        {
            SetPath(Settings.Default.PreviousIncomingPath);
        }

        public static bool DeleteFileThatMayBeInUse(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        File.Delete(path);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }
}
