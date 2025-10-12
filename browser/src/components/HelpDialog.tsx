import { useEffect, useState } from "react";
import ReactMarkdown from "react-markdown";
import { Modal } from "./Modal";

export type HelpDialogProps = {
  isOpen: boolean;
  onClose: () => void;
};

export function HelpDialog({ isOpen, onClose }: HelpDialogProps) {
  const [helpContent, setHelpContent] = useState<string>("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    async function loadHelpContent() {
      try {
        setIsLoading(true);
        setError(null);
        const response = await fetch("/help/help.md");
        if (!response.ok) {
          throw new Error(`Failed to load help: ${response.statusText}`);
        }
        const markdown = await response.text();
        setHelpContent(markdown);
      } catch (err) {
        console.error("Error loading help content:", err);
        setError(err instanceof Error ? err.message : "Failed to load help content");
      } finally {
        setIsLoading(false);
      }
    }

    loadHelpContent();
  }, [isOpen]);

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Help">
      <div className="help-content" data-testid="help-content">
        {isLoading && (
          <div className="flex items-center justify-center py-8">
            <p className="text-slate-600">Loading help content...</p>
          </div>
        )}
        {error && (
          <div className="rounded-lg bg-red-50 p-4 text-red-700">
            <p>{error}</p>
          </div>
        )}
        {!isLoading && !error && (
          <div className="prose prose-slate max-w-none">
            <ReactMarkdown
              components={{
                img: ({ ...props }) => (
                  <img
                    {...props}
                    className="float-left mr-6 mb-4 w-[75px] h-auto"
                    loading="lazy"
                  />
                ),
                h1: ({ ...props }) => (
                  <h2 {...props} className="clear-left mb-4 text-xl font-bold" />
                ),
                h2: ({ ...props }) => (
                  <h3 {...props} className="clear-left mt-6 text-lg font-semibold" />
                ),
              }}
            >
              {helpContent}
            </ReactMarkdown>
          </div>
        )}
      </div>
    </Modal>
  );
}
