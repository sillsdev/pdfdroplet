using System;
using System.Collections.Generic;

namespace PdfDroplet.Interop
{
    internal record LayoutMethodSummary(
        string Id,
        string DisplayName,
        string ThumbnailImage,
        bool IsEnabled,
        bool IsOrientationSensitive
    );

    internal record PaperTargetInfo(
        string Id,
        string DisplayName,
        double WidthPoints,
        double HeightPoints
    );

    internal enum GenerationState
    {
        Idle,
        Working,
        Success,
        Error
    }

    internal record GenerationStatus(
        GenerationState State,
        string Message,
        GenerationError Error = null
    );

    internal record GenerationError(string Message, string Details = null);

    internal record WorkspaceState(
        bool HasIncomingPdf,
        string IncomingPath,
        string SelectedLayoutId,
        string SelectedPaperId,
        bool Mirror,
        bool RightToLeft,
        bool ShowCropMarks,
        string GeneratedPdfPath,
        bool CanReloadPrevious,
        string PreviousIncomingFilename
    );

    internal class WorkspaceStateChangedEventArgs : EventArgs
    {
        public WorkspaceStateChangedEventArgs(WorkspaceState state)
        {
            State = state;
        }

        public WorkspaceState State { get; }
    }

    internal class LayoutChoicesChangedEventArgs : EventArgs
    {
        public LayoutChoicesChangedEventArgs(IReadOnlyList<LayoutMethodSummary> layouts)
        {
            Layouts = layouts;
        }

        public IReadOnlyList<LayoutMethodSummary> Layouts { get; }
    }

    internal class GenerationStatusChangedEventArgs : EventArgs
    {
        public GenerationStatusChangedEventArgs(GenerationStatus status)
        {
            Status = status;
        }

        public GenerationStatus Status { get; }
    }

    internal class GeneratedPdfReadyEventArgs : EventArgs
    {
        public GeneratedPdfReadyEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
