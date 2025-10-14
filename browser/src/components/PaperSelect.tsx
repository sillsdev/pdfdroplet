import {
  Listbox,
  ListboxButton,
  ListboxOption,
  ListboxOptions,
} from "@headlessui/react";
import type { PaperTargetInfo } from "../lib/bridge";

export type PaperSelectProps = {
  paperTargets: PaperTargetInfo[];
  selectedPaperId: string;
  onSelectPaper: (paperId: string) => void;
  disabled?: boolean;
};

export function PaperSelect({
  paperTargets,
  selectedPaperId,
  onSelectPaper,
  disabled = false,
}: PaperSelectProps) {
  const selectedPaper = paperTargets.find((p) => p.id === selectedPaperId);

  return (
    <Listbox
      value={selectedPaperId}
      onChange={onSelectPaper}
      disabled={disabled}
    >
      <div className="relative">
        <ListboxButton
          className={`relative w-full cursor-default rounded-lg border border-slate-200 bg-white px-3 py-2 pr-10 text-left text-sm font-medium shadow-sm focus:border-droplet-accent focus:outline-none focus:ring-2 focus:ring-droplet-accent/20 ${
            disabled
              ? "cursor-not-allowed bg-slate-50 text-slate-400"
              : "text-slate-700"
          }`}
        >
          <span className="block truncate">
            {selectedPaper?.displayName || "Select paper size"}
          </span>
          <span className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2">
            <svg
              className={`h-5 w-5 ${disabled ? "text-slate-400" : "text-slate-500"}`}
              viewBox="0 0 20 20"
              fill="currentColor"
              aria-hidden="true"
            >
              <path
                fillRule="evenodd"
                d="M10 3a.75.75 0 01.55.24l3.25 3.5a.75.75 0 11-1.1 1.02L10 4.852 7.3 7.76a.75.75 0 01-1.1-1.02l3.25-3.5A.75.75 0 0110 3zm-3.76 9.2a.75.75 0 011.06.04l2.7 2.908 2.7-2.908a.75.75 0 111.1 1.02l-3.25 3.5a.75.75 0 01-1.1 0l-3.25-3.5a.75.75 0 01.04-1.06z"
                clipRule="evenodd"
              />
            </svg>
          </span>
        </ListboxButton>

        <ListboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-lg border border-slate-200 bg-white py-1 text-sm shadow-lg focus:outline-none">
          {paperTargets.map((paper) => (
            <ListboxOption
              key={paper.id}
              value={paper.id}
              className="group relative cursor-default select-none px-3 py-2 data-[focus]:bg-droplet-accent data-[focus]:text-white"
            >
              <span className="block truncate font-medium group-data-[selected]:font-semibold">
                {paper.displayName}
              </span>
              <span className="absolute inset-y-0 right-0 hidden items-center pr-3 group-data-[selected]:flex group-data-[focus]:text-white">
                <svg
                  className="h-5 w-5"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  aria-hidden="true"
                >
                  <path
                    fillRule="evenodd"
                    d="M16.704 4.153a.75.75 0 01.143 1.052l-8 10.5a.75.75 0 01-1.127.075l-4.5-4.5a.75.75 0 011.06-1.06l3.894 3.893 7.48-9.817a.75.75 0 011.05-.143z"
                    clipRule="evenodd"
                  />
                </svg>
              </span>
            </ListboxOption>
          ))}
        </ListboxOptions>
      </div>
    </Listbox>
  );
}
