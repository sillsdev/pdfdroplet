using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PdfDroplet.Interop
{
    internal interface IWorkspaceUiBridge
    {
        event EventHandler<WorkspaceStateChangedEventArgs> WorkspaceStateChanged;
        event EventHandler<LayoutChoicesChangedEventArgs> LayoutChoicesChanged;
        event EventHandler<GenerationStatusChangedEventArgs> GenerationStatusChanged;
        event EventHandler<GeneratedPdfReadyEventArgs> GeneratedPdfReady;

        Task<WorkspaceState> GetWorkspaceStateAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LayoutMethodSummary>> GetLayoutChoicesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaperTargetInfo>> GetPaperTargetsAsync(CancellationToken cancellationToken = default);
        Task<RuntimeInfo> GetRuntimeInfoAsync(CancellationToken cancellationToken = default);

        Task<WorkspaceState> PickPdfAsync(CancellationToken cancellationToken = default);
        Task<WorkspaceState> DropPdfAsync(string path, CancellationToken cancellationToken = default);
        Task<WorkspaceState> ReloadPreviousAsync(CancellationToken cancellationToken = default);

        Task<WorkspaceState> SetLayoutAsync(string layoutId, CancellationToken cancellationToken = default);
        Task<WorkspaceState> SetPaperTargetAsync(string paperId, CancellationToken cancellationToken = default);

        Task<WorkspaceState> SetMirrorAsync(bool enabled, CancellationToken cancellationToken = default);
        Task<WorkspaceState> SetRightToLeftAsync(bool enabled, CancellationToken cancellationToken = default);
        Task<WorkspaceState> SetCropMarksAsync(bool enabled, CancellationToken cancellationToken = default);
    }
}
