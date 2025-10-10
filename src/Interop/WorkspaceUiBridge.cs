using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfDroplet.LayoutMethods;
using PdfDroplet.Properties;
using PdfSharp.Drawing;

namespace PdfDroplet.Interop
{
    internal class WorkspaceUiBridge : IWorkspaceUiBridge
    {
        private const int ThumbnailMaxHeight = 80;
        private const int ThumbnailMaxWidth = 70;

        private readonly WorkspaceControl _workspaceControl;
        private readonly WorkSpaceViewModel _viewModel;
        private readonly IWin32Window _ownerWindow;
        private readonly FieldInfo _incomingPathField;
        private readonly FieldInfo _generatedPdfField;
        private readonly FieldInfo _paperWidthField;
        private readonly FieldInfo _paperHeightField;
        private readonly string _previewRoot;
        private WorkspaceState _lastKnownState;
        private IReadOnlyList<LayoutMethodSummary> _lastKnownLayouts = Array.Empty<LayoutMethodSummary>();

        public WorkspaceUiBridge(WorkspaceControl workspaceControl, WorkSpaceViewModel viewModel, IWin32Window ownerWindow)
        {
            _workspaceControl = workspaceControl ?? throw new ArgumentNullException(nameof(workspaceControl));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _ownerWindow = ownerWindow ?? throw new ArgumentNullException(nameof(ownerWindow));

            _incomingPathField = typeof(WorkSpaceViewModel)
                .GetField("_incomingPath", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Unable to access _incomingPath field on WorkSpaceViewModel.");

            _generatedPdfField = typeof(WorkSpaceViewModel)
                .GetField("_pathToCurrentlyDisplayedPdf", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Unable to access _pathToCurrentlyDisplayedPdf field on WorkSpaceViewModel.");

            _paperWidthField = typeof(PaperTarget)
                .GetField("_width", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Unable to access _width field on PaperTarget.");

            _paperHeightField = typeof(PaperTarget)
                .GetField("_height", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Unable to access _height field on PaperTarget.");

            _previewRoot = Path.GetFullPath(WorkSpaceViewModel.GetPreviewDirectory());
        }

        public event EventHandler<WorkspaceStateChangedEventArgs> WorkspaceStateChanged;
        public event EventHandler<LayoutChoicesChangedEventArgs> LayoutChoicesChanged;
        public event EventHandler<GenerationStatusChangedEventArgs> GenerationStatusChanged;
        public event EventHandler<GeneratedPdfReadyEventArgs> GeneratedPdfReady;

        public Task<WorkspaceState> GetWorkspaceStateAsync(CancellationToken cancellationToken = default)
        {
            var state = BuildWorkspaceState();
            _lastKnownState = state;
            EnsureLayoutsCached();
            return Task.FromResult(state);
        }

        public Task<IReadOnlyList<LayoutMethodSummary>> GetLayoutChoicesAsync(CancellationToken cancellationToken = default)
        {
            var layouts = BuildLayoutSummaries();
            _lastKnownLayouts = layouts;

            return Task.FromResult((IReadOnlyList<LayoutMethodSummary>)layouts);
        }

        public Task<IReadOnlyList<PaperTargetInfo>> GetPaperTargetsAsync(CancellationToken cancellationToken = default)
        {
            var targets = _viewModel
                .PaperChoices
                .Select(CreatePaperTargetInfo)
                .ToList()
                .AsReadOnly();

            return Task.FromResult((IReadOnlyList<PaperTargetInfo>)targets);
        }

        public async Task<WorkspaceState> PickPdfAsync(CancellationToken cancellationToken = default)
        {
            return await ExecuteWorkspaceActionAsync(async () =>
            {
                using (var dialog = new OpenFileDialog())
                {
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
                        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }

                    var dialogResult = dialog.ShowDialog(_ownerWindow);
                    if (dialogResult != DialogResult.OK)
                        return await GetWorkspaceStateAsync(cancellationToken).ConfigureAwait(false);

                    _viewModel.SetPath(dialog.FileName);
                    return await NotifyStateChangedAsync().ConfigureAwait(false);
                }
            }, "Opening PDF…").ConfigureAwait(false);
        }

        public Task<WorkspaceState> DropPdfAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path must be provided", nameof(path));

            return ExecuteWorkspaceActionAsync(() =>
            {
                _viewModel.SetPath(path);
                return NotifyStateChangedAsync();
            }, "Processing dropped PDF…");
        }

        public Task<WorkspaceState> ReloadPreviousAsync(CancellationToken cancellationToken = default)
        {
            return ExecuteWorkspaceActionAsync(() =>
            {
                if (!string.IsNullOrEmpty(Settings.Default.PreviousIncomingPath) && File.Exists(Settings.Default.PreviousIncomingPath))
                {
                    _viewModel.ReloadPrevious();
                }

                return NotifyStateChangedAsync();
            }, "Loading previous PDF…");
        }

        public Task<WorkspaceState> SetLayoutAsync(string layoutId, CancellationToken cancellationToken = default)
        {
            var layout = _viewModel
                .GetLayoutChoices()
                .FirstOrDefault(l => string.Equals(l.GetType().FullName, layoutId, StringComparison.Ordinal));

            if (layout == null)
                throw new ArgumentException($"Unknown layout id '{layoutId}'", nameof(layoutId));

            return ExecuteWorkspaceActionAsync(() =>
            {
                _viewModel.SetLayoutMethod(layout);
                return NotifyStateChangedAsync();
            }, "Updating layout…");
        }

        public Task<WorkspaceState> SetPaperTargetAsync(string paperId, CancellationToken cancellationToken = default)
        {
            var target = _viewModel
                .PaperChoices
                .FirstOrDefault(p => string.Equals(p.Name, paperId, StringComparison.OrdinalIgnoreCase));

            if (target == null)
                throw new ArgumentException($"Unknown paper target '{paperId}'", nameof(paperId));

            return ExecuteWorkspaceActionAsync(() =>
            {
                _viewModel.SetPaperTarget(target);
                return NotifyStateChangedAsync();
            }, "Switching paper size…");
        }

        public Task<WorkspaceState> SetMirrorAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            return ExecuteWorkspaceActionAsync(() =>
            {
                _viewModel.SetMirror(enabled);
                return NotifyStateChangedAsync();
            }, enabled ? "Mirroring pages…" : "Updating booklet…");
        }

        public Task<WorkspaceState> SetRightToLeftAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            return ExecuteWorkspaceActionAsync(() =>
            {
                _viewModel.SetRightToLeft(enabled);
                return NotifyStateChangedAsync();
            }, enabled ? "Applying right-to-left layout…" : "Updating text flow…");
        }

        public Task<WorkspaceState> SetCropMarksAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            return ExecuteWorkspaceActionAsync(() =>
            {
                _viewModel.ShowCropMarks(enabled);
                return NotifyStateChangedAsync();
            }, enabled ? "Adding crop marks…" : "Removing crop marks…");
        }

        private WorkspaceState BuildWorkspaceState()
        {
            var incomingPath = _incomingPathField.GetValue(_viewModel) as string;
            var generatedPath = _generatedPdfField.GetValue(_viewModel) as string;
            var generatedClientPath = MapPreviewPathToClientPath(generatedPath);
            var paperTarget = _viewModel.PaperTarget;
            var previousPath = Settings.Default.PreviousIncomingPath;

            return new WorkspaceState(
                _viewModel.HaveIncomingPdf,
                incomingPath ?? string.Empty,
                _viewModel.SelectedMethod == null ? string.Empty : _viewModel.SelectedMethod.GetType().FullName,
                paperTarget == null ? string.Empty : paperTarget.Name,
                Settings.Default.Mirror,
                Settings.Default.RightToLeft,
                Settings.Default.ShowCropMarks,
                generatedClientPath,
                !string.IsNullOrEmpty(previousPath) && File.Exists(previousPath),
                string.IsNullOrEmpty(previousPath) ? string.Empty : Path.GetFileName(previousPath));
        }

        private Task<WorkspaceState> NotifyStateChangedAsync()
        {
            var state = BuildWorkspaceState();
            WorkspaceStateChanged?.Invoke(this, new WorkspaceStateChangedEventArgs(state));
            EnsureLayoutsCached();
            EnsurePdfReadyEvent(state);
            _lastKnownState = state;
            return Task.FromResult(state);
        }

        private LayoutMethodSummary CreateLayoutSummary(LayoutMethod method)
        {
            var id = method.GetType().FullName;
            var displayName = method.ToString();
            var isEnabled = _viewModel.HaveIncomingPdf && method.GetIsEnabled(_viewModel.InputPdf);
            var isOrientationSensitive = method.ImageIsSensitiveToOrientation;
            var imageOrientation = LayoutMethod.IsLandscape(_viewModel.InputPdf);

            string thumbnail = string.Empty;
            using (var originalImage = method.GetImage(imageOrientation))
            using (var resizedImage = ResizeImageForBridge(originalImage))
            using (var memoryStream = new MemoryStream())
            {
                resizedImage.Save(memoryStream, ImageFormat.Png);
                thumbnail = Convert.ToBase64String(memoryStream.ToArray());
            }

            return new LayoutMethodSummary(
                id,
                displayName,
                thumbnail,
                isEnabled,
                isOrientationSensitive);
        }

        private PaperTargetInfo CreatePaperTargetInfo(PaperTarget target)
        {
            var widthUnit = (XUnit)_paperWidthField.GetValue(target);
            var heightUnit = (XUnit)_paperHeightField.GetValue(target);

            return new PaperTargetInfo(
                target.Name,
                target.Name,
                widthUnit.Point,
                heightUnit.Point);
        }

        private void EnsureLayoutsCached()
        {
            var layouts = BuildLayoutSummaries();

            if (!LayoutsEquivalent(_lastKnownLayouts, layouts))
            {
                _lastKnownLayouts = layouts;
                LayoutChoicesChanged?.Invoke(this, new LayoutChoicesChangedEventArgs(layouts));
            }
        }

        private void EnsurePdfReadyEvent(WorkspaceState state)
        {
            if (string.IsNullOrWhiteSpace(state.GeneratedPdfPath))
            {
                return;
            }

            if (_lastKnownState != null && string.Equals(_lastKnownState.GeneratedPdfPath, state.GeneratedPdfPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GeneratedPdfReady?.Invoke(this, new GeneratedPdfReadyEventArgs(state.GeneratedPdfPath));
        }

        private string MapPreviewPathToClientPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            try
            {
                var fullPath = Path.GetFullPath(path);

                var relative = Path.GetRelativePath(_previewRoot, fullPath);
                if (string.IsNullOrEmpty(relative) || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) || relative.Equals("..", StringComparison.Ordinal))
                {
                    return string.Empty;
                }

                var segments = relative
                    .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(Uri.EscapeDataString);
                var joined = string.Join("/", segments);

                return string.IsNullOrEmpty(joined)
                    ? string.Empty
                    : $"https://{WorkspaceControl.PreviewVirtualHostName}/{joined}";
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<WorkspaceState> ExecuteWorkspaceActionAsync(Func<Task<WorkspaceState>> operation, string progressMessage)
        {
            PublishGenerationStatus(GenerationState.Working, progressMessage);

            try
            {
                var state = await operation().ConfigureAwait(false);

                var successMessage = state.HasIncomingPdf
                    ? "Preview updated."
                    : "Ready.";

                PublishGenerationStatus(GenerationState.Success, successMessage);
                return state;
            }
            catch (Exception ex)
            {
                PublishGenerationStatus(GenerationState.Error, ex.Message, new GenerationError(ex.Message, ex.ToString()));
                throw;
            }
        }

        private IReadOnlyList<LayoutMethodSummary> BuildLayoutSummaries()
        {
            return _viewModel
                .GetLayoutChoices()
                .Select(CreateLayoutSummary)
                .ToList()
                .AsReadOnly();
        }

        private static bool LayoutsEquivalent(IReadOnlyList<LayoutMethodSummary> previous, IReadOnlyList<LayoutMethodSummary> current)
        {
            if (ReferenceEquals(previous, current))
            {
                return true;
            }

            if (previous == null || current == null)
            {
                return false;
            }

            if (previous.Count != current.Count)
            {
                return false;
            }

            for (int i = 0; i < current.Count; i++)
            {
                var left = previous[i];
                var right = current[i];

                if (!string.Equals(left.Id, right.Id, StringComparison.Ordinal))
                {
                    return false;
                }

                if (!string.Equals(left.DisplayName, right.DisplayName, StringComparison.Ordinal))
                {
                    return false;
                }

                if (!string.Equals(left.ThumbnailBase64, right.ThumbnailBase64, StringComparison.Ordinal))
                {
                    return false;
                }

                if (left.IsEnabled != right.IsEnabled || left.IsOrientationSensitive != right.IsOrientationSensitive)
                {
                    return false;
                }
            }

            return true;
        }

        private void PublishGenerationStatus(GenerationState state, string message, GenerationError error = null)
        {
            GenerationStatusChanged?.Invoke(this, new GenerationStatusChangedEventArgs(new GenerationStatus(state, message, error)));
        }

        private static Bitmap ResizeImageForBridge(Image originalImage)
        {
            var scaleWidth = (double)ThumbnailMaxWidth / originalImage.Width;
            var scaleHeight = (double)ThumbnailMaxHeight / originalImage.Height;
            var scale = Math.Min(scaleWidth, scaleHeight);

            var newWidth = Math.Max(1, (int)(originalImage.Width * scale));
            var newHeight = Math.Max(1, (int)(originalImage.Height * scale));

            var resizedImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }
    }
}
