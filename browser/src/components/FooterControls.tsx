import type { WorkspaceState } from "../lib/bridge";

export type FooterControlsProps = {
  workspaceState: WorkspaceState | null;
  controlsDisabled: boolean;
  isBootstrapping: boolean;
  onToggleRtl: (enabled: boolean) => void;
  onToggleMirror: (enabled: boolean) => void;
  onToggleCropMarks: (enabled: boolean) => void;
  onSaveBooklet: () => void;
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
  onSaveBooklet,
  // onReloadPrevious,
  onPickPdf,
  onShowAbout,
  onShowHelp,
}: FooterControlsProps) {
  return (
    <footer className="flex flex-col gap-3 px-4 pb-0 md:flex-row md:justify-between">
      <div className="flex flex-wrap items-center gap-4 text-sm text-slate-700">
        <label className="inline-flex items-center gap-2">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
            checked={workspaceState?.rightToLeft ?? false}
            disabled={controlsDisabled}
            onChange={(event) => onToggleRtl(event.target.checked)}
          />
          <span className={controlsDisabled ? "text-slate-400" : ""}>
            Right-to-Left Language
          </span>
        </label>
        <label className="inline-flex items-center gap-2">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
            checked={workspaceState?.mirror ?? false}
            disabled={controlsDisabled}
            onChange={(event) => onToggleMirror(event.target.checked)}
          />
          <span className={controlsDisabled ? "text-slate-400" : ""}>
            Mirror
          </span>
        </label>
        <label className="inline-flex items-center gap-2">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-slate-300 text-droplet-accent focus:ring-droplet-accent"
            checked={workspaceState?.showCropMarks ?? false}
            disabled={controlsDisabled}
            onChange={(event) => onToggleCropMarks(event.target.checked)}
          />
          <span className={controlsDisabled ? "text-slate-400" : ""}>
            Crop Marks
          </span>
        </label>
        {/* {workspaceState?.canReloadPrevious && (
          <button
            type="button"
            onClick={onReloadPrevious}
            className="text-sm font-semibold text-droplet-accent underline-offset-4 hover:underline disabled:text-slate-400"
            disabled={isBootstrapping}
          >
            Open Previous ({workspaceState.previousIncomingFilename})
          </button>
        )} */}
      </div>

      <div className="flex items-center gap-2 text-sm">
        <button
          type="button"
          onClick={onSaveBooklet}
          className="rounded bg-droplet-accent px-4 py-1.5 font-semibold text-white transition-colors hover:bg-droplet-accent/90 disabled:bg-slate-300 disabled:text-slate-500"
          disabled={controlsDisabled || isBootstrapping}
          data-testid="save-button"
        >
          Save Booklet...
        </button>
        <button
          type="button"
          onClick={onPickPdf}
          className="hidden rounded px-3 py-1.5 transition-colors hover:bg-slate-100 hover:text-droplet-accent"
          disabled={isBootstrapping}
        >
          Open
        </button>
        <button
          type="button"
          onClick={onShowHelp}
          className="rounded px-3 py-1.5 transition-colors hover:bg-slate-100 hover:text-droplet-accent"
          data-testid="help-button"
        >
          Help
        </button>
        <button
          type="button"
          onClick={onShowAbout}
          className="rounded px-3 py-1.5 transition-colors hover:bg-slate-100 hover:text-droplet-accent"
          data-testid="about-button"
        >
          About
        </button>
      </div>
    </footer>
  );
}
