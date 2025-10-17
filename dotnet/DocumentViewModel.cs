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
        private static readonly IReadOnlyList<PaperTemplate> s_paperTemplates = CreatePaperTemplates();

        public DocumentViewModel()
        {
            //default to whatever the printer's default is
            PrinterSettings printer = new System.Drawing.Printing.PrinterSettings();
            PaperTarget = MapPrinterPaperSizeToTarget(printer.DefaultPageSettings.PaperSize)
                           ?? (s_paperTemplates.Count > 0 ? s_paperTemplates[0].Target : null);
        }

        private PaperTarget MapPrinterPaperSizeToTarget(System.Drawing.Printing.PaperSize printerPaperSize)
        {
            // Map the printer's paper size name to a PdfSharp PageSize
            var paperName = printerPaperSize.PaperName;

            foreach (var template in s_paperTemplates)
            {
                if (paperName.IndexOf(template.Target.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return template.Target;
                }
            }

            // Default to A4 if we can't match the printer's paper size
            foreach (var template in s_paperTemplates)
            {
                if (string.Equals(template.Target.Name, "A4", StringComparison.OrdinalIgnoreCase))
                {
                    return template.Target;
                }
            }

            return s_paperTemplates.Count > 0 ? s_paperTemplates[0].Target : null;
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
            // Don't persist crop marks setting - it's transient per document
            // Settings.Default.Save();

            SetLayoutMethod(SelectedMethod);//cause to re-do it with this setting
        }

        public void SetMirror(bool doMirror)
        {
            Settings.Default.Mirror = doMirror;
            // Don't persist mirror setting - it's transient per document
            // Settings.Default.Save();

            SetLayoutMethod(SelectedMethod);
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

        internal string IncomingPath => _incomingPath ?? string.Empty;

        internal string CurrentPreviewPath => _pathToCurrentlyDisplayedPdf ?? string.Empty;

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

            // Reset transient settings when loading a new PDF
            Settings.Default.Mirror = false;
            Settings.Default.ShowCropMarks = false;
            // Note: We don't call Settings.Default.Save() here to avoid persisting these resets

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
            if (target == null)
            {
                return;
            }

            var resolvedTarget = ResolvePaperTarget(target) ?? target;

            if (PaperTarget == null || !string.Equals(PaperTarget.Name, resolvedTarget.Name, StringComparison.Ordinal))
            {
                PaperTarget = resolvedTarget;
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
                foreach (var template in s_paperTemplates)
                {
                    yield return template.Target;
                }
            }
        }

        internal (double WidthPoints, double HeightPoints) GetPaperTargetDimensions(PaperTarget target)
        {
            var template = FindTemplate(target);
            if (template != null)
            {
                return (template.Size.Width, template.Size.Height);
            }

            return (0d, 0d);
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

        private sealed class PaperTemplate
        {
            public PaperTemplate(PaperTarget target, XSize size)
            {
                Target = target;
                Size = size;
            }

            public PaperTarget Target { get; }

            public XSize Size { get; }
        }

        private static IReadOnlyList<PaperTemplate> CreatePaperTemplates()
        {
            var templates = new List<PaperTemplate>
            {
                CreatePaperTemplate("A4", PageSize.A4),
                CreatePaperTemplate("A3", PageSize.A3),
                CreatePaperTemplate("Letter", PageSize.Letter),
                CreatePaperTemplate("Legal", PageSize.Legal),
                CreatePaperTemplate("Foolscap", PageSize.Foolscap)
            };

            return templates.AsReadOnly();
        }

        private static PaperTemplate FindTemplate(PaperTarget target)
        {
            if (target == null)
            {
                return null;
            }

            foreach (var template in s_paperTemplates)
            {
                if (ReferenceEquals(template.Target, target) || string.Equals(template.Target.Name, target.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return template;
                }
            }

            return null;
        }

        private static PaperTarget ResolvePaperTarget(PaperTarget candidate)
        {
            var template = FindTemplate(candidate);
            return template?.Target;
        }

        private static PaperTemplate CreatePaperTemplate(string name, PageSize pageSize)
        {
            var target = new PaperTarget(name, pageSize);
            var size = PageSizeConverter.ToSize(pageSize);
            return new PaperTemplate(target, new XSize(size.Width, size.Height));
        }
    }
}
