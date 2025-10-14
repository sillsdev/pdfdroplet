using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Threading;
using SIL.Reporting;
using DotImpose.LayoutMethods;
using PdfDroplet.Properties;
using PdfSharp;
using PdfSharp.Drawing;

namespace PdfDroplet
{
    /// <summary>
    /// This is the state of the program.
    /// </summary>
    class DocumentViewModel
    {
        
        private string _incomingPath;
        private XPdfForm _inputPdf;
        private string _pathToCurrentlyDisplayedPdf;
        private const int PreviewHistoryLimit = 2;
        private readonly List<string> _generatedPreviewPaths = new List<string>();

        public DocumentViewModel()
        {
            //default to whatever the printer's default is
            PrinterSettings printer = new System.Drawing.Printing.PrinterSettings();
            PaperTarget = MapPrinterPaperSizeToTarget(printer.DefaultPageSettings.PaperSize);
        }

        private PaperTarget MapPrinterPaperSizeToTarget(System.Drawing.Printing.PaperSize printerPaperSize)
        {
            // Map the printer's paper size name to a PdfSharp PageSize
            var paperName = printerPaperSize.PaperName;
            
            // Try to match common paper size names (case-insensitive)
            if (paperName.IndexOf("A4", StringComparison.OrdinalIgnoreCase) >= 0)
                return new PaperTarget("A4", PageSize.A4);
            if (paperName.IndexOf("A3", StringComparison.OrdinalIgnoreCase) >= 0)
                return new PaperTarget("A3", PageSize.A3);
            if (paperName.IndexOf("Letter", StringComparison.OrdinalIgnoreCase) >= 0)
                return new PaperTarget("Letter", PageSize.Letter);
            if (paperName.IndexOf("Legal", StringComparison.OrdinalIgnoreCase) >= 0)
                return new PaperTarget("Legal", PageSize.Legal);
            if (paperName.IndexOf("Foolscap", StringComparison.OrdinalIgnoreCase) >= 0)
                return new PaperTarget("Foolscap", PageSize.Foolscap);
            
            // Default to A4 if we can't match the printer's paper size
            return new PaperTarget("A4", PageSize.A4);
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
            Console.WriteLine($"[viewmodel] SetPath invoked with '{path}' (exists={File.Exists(path)})");
            _incomingPath = path;
            _inputPdf = OpenDocumentForPdfSharp(_incomingPath);
            Console.WriteLine("[viewmodel] Input PDF loaded successfully");
            SetLayoutMethod(new NullLayoutMethod());
        }

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
            Console.WriteLine("[viewmodel] ContinueConversionAndNavigation starting");
            if (IsAlreadyOpenElsewhere(_incomingPath))
            {
                Console.WriteLine("[viewmodel] Detected file already open elsewhere");
                ErrorReport.NotifyUserOfProblem("That file appears to be open. First close it, then try again.");
                return;
            }

            var outputPath = CreatePreviewOutputPath();

            try
            {
                Console.WriteLine($"[viewmodel] Laying out PDF '{_incomingPath}' to '{outputPath}'");
                SelectedMethod.Layout(
                    _inputPdf,
                    _incomingPath,
                    outputPath,
                    PaperTarget,
                    Settings.Default.RightToLeft,
                    Settings.Default.ShowCropMarks);

                _pathToCurrentlyDisplayedPdf = outputPath;
                TrackGeneratedPreview(outputPath);
                Console.WriteLine("[viewmodel] Layout completed successfully");
            }
            catch (Exception error)
            {
                Console.WriteLine($"[viewmodel] Layout failed: {error.Message}\n{error}");
                TryDeletePreview(outputPath);
                ErrorReport.NotifyUserOfProblem(error, "PdfBooklet was unable to convert that file.");
            }
        }



        /// <summary>
        /// Open a PDF document for use with PdfSharp
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
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to open PDF file '{Path.GetFileName(path)}'. " +
                    "The file may be corrupted, encrypted, or in an unsupported format. " +
                    "Please ensure the PDF is valid and try again.", ex);
            }
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
