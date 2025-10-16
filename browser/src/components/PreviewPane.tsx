//import type { DragEvent } from "react";
import type { GenerationStatus } from "../lib/bridge";

// Note: we are currently not handling drag and drop events here.
// By letting the host application handle dropping, we can actually get
// the path to the file which is more useful than just the bytes.

export type PreviewPaneProps = {
  hasIncomingPdf: boolean;
  previewSrc: string;
  generationStatus: GenerationStatus | null;
  primaryMessage: string;
  isDragActive: boolean;
  isBootstrapping: boolean;
  onPickPdf: () => void;
};

export function PreviewPane({
  hasIncomingPdf,
  previewSrc,
  generationStatus,
  primaryMessage,
  isDragActive,
  isBootstrapping,
  onPickPdf,
}: PreviewPaneProps) {
  return (
    <section className="relative flex flex-1 min-h-0 items-stretch overflow-hidden rounded-2xl bg-white shadow-panel">
      {hasIncomingPdf && previewSrc ? (
        <iframe
          title="Booklet preview"
          src={`${previewSrc}#toolbar=0`}
          className="h-full w-full border-0 bg-slate-100"
          allow="accelerometer; clipboard-write; encrypted-media; picture-in-picture"
        />
      ) : (
        <div
          className={`flex w-full flex-1 flex-col items-center justify-center gap-4 border-2 border-dashed ${
            isDragActive
              ? "border-droplet-accent bg-droplet-accent/10"
              : "border-slate-300 bg-white/90"
          } p-10 text-center`}
          data-testid="drop-zone"
          //    onDragEnter={onDragEnter}
          // onDragOver={onDragOver}
          // onDragLeave={onDragLeave}
          // onDrop={onDrop}
        >
          <div className="space-y-1">
            <p className="text-xl font-semibold text-slate-800">
              {primaryMessage}
            </p>
            <p className="text-sm text-slate-500">
              To get started, drag a PDF file here or click this button:
            </p>
          </div>
          <button
            type="button"
            disabled={isBootstrapping}
            onClick={onPickPdf}
            className="rounded-lg bg-droplet-accent px-5 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-droplet-accent/90 disabled:cursor-not-allowed disabled:bg-slate-300"
          >
            Choose a PDF to open
          </button>
        </div>
      )}

      {generationStatus?.state === "working" && (
        <div className="pointer-events-none absolute inset-0 flex items-end justify-end bg-slate-900/10">
          <div className="m-4 rounded-lg bg-slate-900/80 px-4 py-2 text-xs font-medium text-white">
            {generationStatus.message || "Processing…"}
          </div>
        </div>
      )}
    </section>
  );
}
