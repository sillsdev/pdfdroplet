import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type DragEvent,
} from "react";
import { LayoutChooser } from "./components/LayoutChooser";
import { FooterControls } from "./components/FooterControls";
import { PreviewPane } from "./components/PreviewPane";
import {
  bridge,
  type GenerationStatus,
  type LayoutMethodSummary,
  type PaperTargetInfo,
  type WorkspaceState,
} from "./lib/bridge";

function toPreviewSrc(path: string | null | undefined) {
  if (!path) {
    return "";
  }

  const trimmed = path.trim();
  if (!trimmed) {
    return "";
  }

  if (/^[a-z]+:\/\//i.test(trimmed)) {
    return trimmed;
  }

  const normalized = trimmed.replace(/\\/g, "/").replace(/^\/+/, "");
  return `file:///${normalized}`;
}

function normalizeWindowsPath(path: string) {
  if (!path) {
    return "";
  }

  const trimmed = path.trim();
  if (!trimmed) {
    return "";
  }

  return trimmed.replace(/\//g, "\\");
}

function tryParseFileUri(value: string | null | undefined) {
  if (!value) {
    return null;
  }

  const trimmed = value.trim();
  if (!trimmed) {
    return null;
  }

  try {
    const url = new URL(trimmed);
    if (url.protocol !== "file:") {
      return null;
    }

    const decodedPath = decodeURIComponent(url.pathname || "");
    if (url.hostname) {
      const host = url.hostname;
      const uncPath = decodedPath.replace(/\//g, "\\");
      return `\\\\${host}${uncPath}`;
    }

    const withoutLeadingSlash = decodedPath.startsWith("/")
      ? decodedPath.slice(1)
      : decodedPath;
    return normalizeWindowsPath(withoutLeadingSlash);
  } catch (error) {
    console.debug("Failed to parse file URI", value, error);
    return null;
  }
}

function extractDroppedPath(event: DragEvent<HTMLDivElement>) {
  const parseCandidate = (candidate: string | null | undefined) => {
    if (!candidate) {
      return null;
    }

    const trimmed = candidate.replace(/\0/g, "").trim();
    if (!trimmed) {
      return null;
    }

    const uriParsed = tryParseFileUri(trimmed);
    if (uriParsed) {
      return uriParsed;
    }

    if (/^[a-zA-Z]:\\/.test(trimmed) || trimmed.startsWith("\\\\")) {
      return normalizeWindowsPath(trimmed);
    }

    return null;
  };

  const file = event.dataTransfer.files?.[0];
  const explicitPath = (file as unknown as { path?: string })?.path;
  if (explicitPath) {
    return normalizeWindowsPath(explicitPath);
  }

  const fileDrop = event.dataTransfer.getData("FileDrop");
  if (fileDrop) {
    const lines = fileDrop
      .split(/\0|\r?\n/)
      .map((line) => line.trim())
      .filter((line) => line.length > 0);

    for (const line of lines) {
      const parsed = parseCandidate(line);
      if (parsed) {
        return parsed;
      }
    }
  }

  const uriList = event.dataTransfer.getData("text/uri-list");
  if (uriList) {
    const lines = uriList
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter((line) => line.length > 0 && !line.startsWith("#"));

    for (const line of lines) {
      const parsed = parseCandidate(line);
      if (parsed) {
        return parsed;
      }
    }
  }

  const plainText = event.dataTransfer.getData("text/plain");
  const parsedPlain = parseCandidate(plainText);
  if (parsedPlain) {
    return parsedPlain;
  }

  return null;
}

function App() {
  const [workspaceState, setWorkspaceState] = useState<WorkspaceState | null>(
    null
  );
  const [layouts, setLayouts] = useState<LayoutMethodSummary[]>([]);
  const [paperTargets, setPaperTargets] = useState<PaperTargetInfo[]>([]);
  const [generationStatus, setGenerationStatus] =
    useState<GenerationStatus | null>(null);
  const [isBootstrapping, setIsBootstrapping] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isDragActive, setIsDragActive] = useState(false);

  useEffect(() => {
    let isMounted = true;

    async function bootstrap() {
      try {
        const [state, layoutSummaries, paperSummaries] = await Promise.all([
          bridge.requestState(),
          bridge.requestLayouts(),
          bridge.requestPaperTargets(),
        ]);

        if (!isMounted) {
          return;
        }

        setWorkspaceState(state);
        setLayouts(layoutSummaries);
        setPaperTargets(paperSummaries);
      } catch (error) {
        console.error("Failed to bootstrap workspace bridge", error);
        if (error instanceof Error) {
          setErrorMessage(error.message);
        } else {
          setErrorMessage("Unable to load workspace information.");
        }
      } finally {
        if (isMounted) {
          setIsBootstrapping(false);
        }
      }
    }

    bootstrap();

    const unsubscribeState = bridge.on("stateChanged", (state) => {
      setWorkspaceState(state);
    });
    const unsubscribeLayouts = bridge.on("layoutsChanged", (list) => {
      setLayouts(list);
    });
    const unsubscribeGeneration = bridge.on("generationStatus", (status) => {
      setGenerationStatus(status);
    });
    const unsubscribePdfReady = bridge.on("generatedPdfReady", ({ path }) => {
      setWorkspaceState((current) =>
        current ? { ...current, generatedPdfPath: path } : current
      );
    });

    return () => {
      isMounted = false;
      unsubscribeState();
      unsubscribeLayouts();
      unsubscribeGeneration();
      unsubscribePdfReady();
    };
  }, []);

  const runCommand = useCallback(
    async (command: () => Promise<WorkspaceState>) => {
      try {
        const updated = await command();
        setWorkspaceState(updated);
        setErrorMessage(null);
      } catch (error) {
        console.error("Workspace command failed", error);
        if (error instanceof Error) {
          setErrorMessage(error.message);
        } else {
          setErrorMessage(
            "An unexpected error occurred while processing the command."
          );
        }
      }
    },
    []
  );

  const handlePickPdf = useCallback(
    () => runCommand(() => bridge.pickPdf()),
    [runCommand]
  );

  const handleReloadPrevious = useCallback(
    () => runCommand(() => bridge.reloadPrevious()),
    [runCommand]
  );

  const handleDrop = useCallback(
    (event: DragEvent<HTMLDivElement>) => {
      event.preventDefault();
      setIsDragActive(false);
      const path = extractDroppedPath(event);

      if (!path) {
        setErrorMessage(
          "We couldn't read the dropped file path. Try dropping a PDF from File Explorer or use the Choose button."
        );
        return;
      }

      if (!path.toLowerCase().endsWith(".pdf")) {
        setErrorMessage("That file must have a .pdf extension.");
        return;
      }

      setErrorMessage(null);
      void runCommand(() => bridge.dropPdf(path));
    },
    [runCommand]
  );

  const handleDragEnter = useCallback((event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setIsDragActive(true);
  }, []);

  const handleDragOver = useCallback((event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    event.dataTransfer.dropEffect = "copy";
  }, []);

  const handleDragLeave = useCallback((event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setIsDragActive(false);
  }, []);

  const handleSelectLayout = useCallback(
    (layoutId: string) => runCommand(() => bridge.setLayout(layoutId)),
    [runCommand]
  );

  const handleSelectPaper = useCallback(
    (paperId: string) => runCommand(() => bridge.setPaper(paperId)),
    [runCommand]
  );

  const handleToggleMirror = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setMirror(enabled)),
    [runCommand]
  );

  const handleToggleRtl = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setRightToLeft(enabled)),
    [runCommand]
  );

  const handleToggleCropMarks = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setCropMarks(enabled)),
    [runCommand]
  );

  const selectedPaperId =
    workspaceState?.selectedPaperId ?? paperTargets[0]?.id ?? "";
  const selectedLayoutId = workspaceState?.selectedLayoutId ?? "";
  const hasIncomingPdf = Boolean(workspaceState?.hasIncomingPdf);
  const controlsDisabled = !workspaceState || !workspaceState.hasIncomingPdf;
  const previewSrc = useMemo(() => {
    if (generationStatus?.state === "working") {
      return "";
    }

    return toPreviewSrc(workspaceState?.generatedPdfPath);
  }, [generationStatus?.state, workspaceState?.generatedPdfPath]);
  const primaryMessage = useMemo(() => {
    if (generationStatus?.state === "working") {
      return generationStatus.message || "Preparing your booklet…";
    }

    return isDragActive
      ? "Release to drop your PDF"
      : "Drag a PDF document here";
  }, [generationStatus, isDragActive]);

  return (
    <div className="flex h-screen w-full gap-6 overflow-hidden bg-droplet-background p-6 text-droplet-primary">
      <LayoutChooser
        paperTargets={paperTargets}
        selectedPaperId={selectedPaperId}
        onSelectPaper={handleSelectPaper}
        layouts={layouts}
        selectedLayoutId={selectedLayoutId}
        onSelectLayout={handleSelectLayout}
        disabled={isBootstrapping}
      />

      <div className="flex flex-1 min-h-0 flex-col gap-4 overflow-hidden">
        <PreviewPane
          hasIncomingPdf={hasIncomingPdf}
          previewSrc={previewSrc}
          generationStatus={generationStatus}
          primaryMessage={primaryMessage}
          isDragActive={isDragActive}
          isBootstrapping={isBootstrapping}
          onPickPdf={handlePickPdf}
          onDrop={handleDrop}
          onDragEnter={handleDragEnter}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
        />

        {errorMessage && (
          <div className="flex-none rounded-xl border border-red-200 bg-red-50 px-4 py-2 text-sm text-red-700">
            {errorMessage}
          </div>
        )}

        <FooterControls
          workspaceState={workspaceState}
          controlsDisabled={controlsDisabled}
          isBootstrapping={isBootstrapping}
          onToggleRtl={handleToggleRtl}
          onToggleMirror={handleToggleMirror}
          onToggleCropMarks={handleToggleCropMarks}
          onReloadPrevious={handleReloadPrevious}
          onPickPdf={handlePickPdf}
        />
      </div>
    </div>
  );
}

export default App;
