import type { WorkspaceState } from "../lib/bridge";

export type FooterControlsProps = {
  workspaceState: WorkspaceState | null;
  controlsDisabled: boolean;
  isBootstrapping: boolean;
  onToggleRtl: (enabled: boolean) => void;
  onToggleMirror: (enabled: boolean) => void;
  onToggleCropMarks: (enabled: boolean) => void;
  onReloadPrevious: () => void;
  onPickPdf: () => void;
  onShowAbout: () => void;
  onShowHelp: () => void;
};

export function FooterControls({
  workspaceState,
  controlsDisabled,
  isBootstrapping,
  onToggleRtl,
  onToggleMirror,
  onToggleCropMarks,
  onReloadPrevious,
  onPickPdf,
  onShowAbout,
  onShowHelp,
}: FooterControlsProps) {
  return (
    <footer className="flex flex-col gap-3 rounded-2xl bg-white p-4 shadow-panel md:flex-row md:items-center md:justify-between">
      <div className="flex flex-wrap items-center gap-4 text-sm text-slate-700">
        <label className="inline-flex items-center gap-2">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
            checked={workspaceState?.rightToLeft ?? false}
            disabled={controlsDisabled}
            onChange={(event) => onToggleRtl(event.target.checked)}
          />
          <span>Right-to-Left Language</span>
        </label>
        <label className="inline-flex items-center gap-2">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
            checked={workspaceState?.mirror ?? false}
            disabled={controlsDisabled}
            onChange={(event) => onToggleMirror(event.target.checked)}
          />
          <span>Mirror</span>
        </label>
        <label className="inline-flex items-center gap-2">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
            checked={workspaceState?.showCropMarks ?? false}
            disabled={controlsDisabled}
            onChange={(event) => onToggleCropMarks(event.target.checked)}
          />
          <span>Crop Marks</span>
        </label>
        {workspaceState?.canReloadPrevious && (
          <button
            type="button"
            onClick={onReloadPrevious}
            className="text-sm font-semibold text-droplet-accent underline-offset-4 hover:underline disabled:text-slate-400"
            disabled={isBootstrapping}
          >
            Open Previous ({workspaceState.previousIncomingFilename})
          </button>
        )}
      </div>

      <div className="flex items-center gap-4 text-sm">
        <button
          type="button"
          onClick={onPickPdf}
          className="text-droplet-accent underline-offset-4 hover:underline disabled:text-slate-400"
          disabled={isBootstrapping}
        >
          Choose a PDF to open
        </button>
        <button
          type="button"
          onClick={onShowHelp}
          className="underline-offset-4 hover:text-droplet-accent hover:underline"
          data-testid="help-button"
        >
          Help
        </button>
        <button
          type="button"
          onClick={onShowAbout}
          className="underline-offset-4 hover:text-droplet-accent hover:underline"
          data-testid="about-button"
        >
          About
        </button>
      </div>
    </footer>
  );
}
