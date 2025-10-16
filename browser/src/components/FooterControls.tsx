import type { WorkspaceState, GenerationStatus } from "../lib/bridge";

export type FooterControlsProps = {
  workspaceState: WorkspaceState | null;
  controlsDisabled: boolean;
  isBootstrapping: boolean;
  generationStatus: GenerationStatus | null;
  lastSavedPdfPath: string | null;
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
  generationStatus,
  lastSavedPdfPath,
  onToggleRtl,
  onToggleMirror,
  onToggleCropMarks,
  onSaveBooklet,
  // onReloadPrevious,
  onPickPdf,
  onShowAbout,
  onShowHelp,
}: FooterControlsProps) {
  // Disable Save Booklet button when:
  // - No workspace state or bootstrapping
  // - "Original" layout is selected (no booklet to save)
  // - No PDF has been generated yet
  // - Currently generating a booklet
  // - The current generated PDF has already been saved
  const isOriginalLayout = workspaceState?.selectedLayoutId === "original";
  const hasGeneratedPdf = Boolean(workspaceState?.generatedPdfPath);
  const isGenerating = generationStatus?.state === "working";
  const currentPdfPath = workspaceState?.generatedPdfPath;
  const alreadySaved = Boolean(
    currentPdfPath && lastSavedPdfPath && currentPdfPath === lastSavedPdfPath,
  );

  const saveDisabled =
    controlsDisabled ||
    isBootstrapping ||
    isOriginalLayout ||
    !hasGeneratedPdf ||
    isGenerating ||
    alreadySaved;

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
          className="inline-flex items-center gap-1.5 rounded px-3 py-1 text-sm font-medium transition-colors enabled:text-white disabled:bg-transparent disabled:text-slate-400"
          style={{ backgroundColor: saveDisabled ? "transparent" : "#2342DA" }}
          onMouseEnter={(e) =>
            !saveDisabled && (e.currentTarget.style.backgroundColor = "#1c35ae")
          }
          onMouseLeave={(e) =>
            !saveDisabled && (e.currentTarget.style.backgroundColor = "#2342DA")
          }
          disabled={saveDisabled}
          data-testid="save-button"
          title="Save booklet"
        >
          <img
            src="/images/save.svg"
            alt=""
            className={`h-4 w-4 ${saveDisabled ? "opacity-40" : "brightness-0 invert"}`}
          />
          Save...
        </button>
      </div>
    </footer>
  );
}
