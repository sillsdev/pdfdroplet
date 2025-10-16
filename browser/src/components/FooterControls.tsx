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
  // Disable Save Booklet button when "Original" layout is selected
  const isOriginalLayout = workspaceState?.selectedLayoutId === "original";
  const saveDisabled = controlsDisabled || isBootstrapping || isOriginalLayout;

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
        <button
          type="button"
          onClick={onSaveBooklet}
          className="inline-flex items-center gap-1.5 rounded bg-droplet-accent px-3 py-1 text-sm font-medium text-white transition-colors hover:bg-droplet-accent/90 disabled:bg-slate-300 disabled:text-slate-500"
          disabled={saveDisabled}
          data-testid="save-button"
          title="Save booklet"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 16 16"
            fill="currentColor"
            className="h-4 w-4"
          >
            <path
              fillRule="evenodd"
              d="M14 4.5V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2h5.5L14 4.5ZM4 1a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h-2A1.5 1.5 0 0 1 9.5 3V1H4Zm5.646 5.646a.5.5 0 0 1 .708 0l2 2a.5.5 0 0 1 0 .708l-2 2a.5.5 0 0 1-.708-.708L10.793 9.5H6.5a.5.5 0 0 1 0-1h4.293L9.646 7.354a.5.5 0 0 1 0-.708Z"
              clipRule="evenodd"
            />
          </svg>
          Save...
        </button>
      </div>
    </footer>
  );
}
