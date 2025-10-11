import type { LayoutMethodSummary, PaperTargetInfo } from "../lib/bridge";

function LayoutThumbnail({ summary }: { summary: LayoutMethodSummary }) {
  if (summary.thumbnailImage) {
    return (
      <img
        src={`/images/${summary.thumbnailImage}`}
        alt={`${summary.displayName} thumbnail`}
        className="h-16 w-16 rounded-lg   object-contain"
      />
    );
  }

  return (
    <div className="flex h-16 w-16 items-center justify-center rounded-lg border border-slate-200 bg-white text-xs font-semibold text-slate-400">
      Preview
    </div>
  );
}

export type LayoutChooserProps = {
  paperTargets: PaperTargetInfo[];
  selectedPaperId: string;
  onSelectPaper: (paperId: string) => void;
  layouts: LayoutMethodSummary[];
  selectedLayoutId: string;
  onSelectLayout: (layoutId: string) => void;
  disabled?: boolean;
};

export function LayoutChooser({
  paperTargets,
  selectedPaperId,
  onSelectPaper,
  layouts,
  selectedLayoutId,
  onSelectLayout,
  disabled = false,
}: LayoutChooserProps) {
  return (
    <aside className="flex h-full w-60 flex-none flex-col overflow-hidden rounded-2xl bg-white p-4 shadow-panel">
      <label className="text-xs font-semibold uppercase tracking-wide text-slate-500">
        Printer Paper Size
      </label>
      <select
        className="mt-2 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 shadow-sm focus:border-droplet-accent focus:outline-none focus:ring-2 focus:ring-droplet-accent/20"
        value={selectedPaperId}
        onChange={(event) => onSelectPaper(event.target.value)}
        disabled={disabled}
      >
        {paperTargets.map((paper) => (
          <option key={paper.id} value={paper.id}>
            {paper.displayName}
          </option>
        ))}
      </select>

      <div className="mt-4 flex-1 overflow-y-auto pr-1">
        <ul className="space-y-2">
          {layouts.map((layout) => {
            const isSelected = layout.id === selectedLayoutId;
            return (
              <li key={layout.id}>
                <button
                  className={`group flex w-full flex-col items-stretch rounded-xl border p-3 text-left transition ${
                    isSelected
                      ? "border-droplet-accent bg-droplet-accent/10"
                      : "border-transparent bg-slate-50 hover:border-slate-200 hover:bg-slate-100"
                  } ${
                    layout.isEnabled
                      ? "cursor-pointer"
                      : "cursor-not-allowed opacity-60"
                  }`}
                  disabled={!layout.isEnabled || disabled}
                  onClick={() => onSelectLayout(layout.id)}
                >
                  <div className="flex items-start gap-3">
                    <LayoutThumbnail summary={layout} />
                    <div>
                      <p className="text-sm font-semibold text-slate-800">
                        {layout.displayName}
                      </p>
                    </div>
                  </div>
                </button>
              </li>
            );
          })}
        </ul>
      </div>
    </aside>
  );
}
