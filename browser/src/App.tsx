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
import { AboutDialog } from "./components/AboutDialog";
import { HelpDialog } from "./components/HelpDialog";
import {
  bridge,
  type GenerationStatus,
  type LayoutMethodSummary,
  type PaperTargetInfo,
  type WorkspaceState,
} from "./lib/bridge";
import { extractDroppedPath } from "./lib/pathHandling";

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
  const [isAboutDialogOpen, setIsAboutDialogOpen] = useState(false);
  const [isHelpDialogOpen, setIsHelpDialogOpen] = useState(false);

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

  const processDroppedPdf = useCallback(
    (
      rawPath: string | null | undefined,
      context: { source: "dom" | "host"; formats?: string[] } = {
        source: "dom",
      }
    ) => {
      const descriptor = context.source === "host" ? "[drop][host]" : "[drop]";

      if (!rawPath) {
        console.warn(
          `${descriptor} No usable path was resolved from the drop payload`,
          context.formats ?? []
        );
        setErrorMessage(
          "We couldn't read the dropped file path. Try dropping a PDF from File Explorer or use the Choose button."
        );
        return;
      }

      const normalized = rawPath.trim();
      if (!normalized) {
        console.warn(`${descriptor} Dropped path contained only whitespace.`);
        setErrorMessage(
          "We couldn't read the dropped file path. Try dropping a PDF from File Explorer or use the Choose button."
        );
        return;
      }

      if (!normalized.toLowerCase().endsWith(".pdf")) {
        console.warn(
          `${descriptor} Dropped file did not have a .pdf extension`,
          {
            path: normalized,
            formats: context.formats,
          }
        );
        setErrorMessage("That file must have a .pdf extension.");
        return;
      }

      console.info(`${descriptor} Initiating dropPdf workspace command`, {
        path: normalized,
        formats: context.formats,
      });
      setErrorMessage(null);
      void runCommand(() => bridge.dropPdf(normalized));
    },
    [runCommand]
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

      const dataTransfer = event.dataTransfer;
      const types = Array.from(dataTransfer?.types ?? []);

      console.groupCollapsed(
        `[drop] handleDrop captured: types=${
          types.length > 0 ? types.join(", ") : "(none)"
        }`
      );

      try {
        if (dataTransfer) {
          const files = dataTransfer.files;
          if (files && files.length > 0) {
            const summaries = Array.from(files).map((file) => ({
              name: file.name,
              size: file.size,
              type: file.type,
            }));
            console.info("[drop] handleDrop files metadata", summaries);
          } else {
            console.info("[drop] handleDrop received no File entries");
          }

          console.info("[drop] handleDrop dropEffect/effectAllowed", {
            dropEffect: dataTransfer.dropEffect,
            effectAllowed: dataTransfer.effectAllowed,
          });
        }

        const path = extractDroppedPath(event);
        console.info("[drop] handleDrop extracted path", path);

        processDroppedPdf(path, { source: "dom", formats: types });
      } finally {
        console.groupEnd();
      }
    },
    [processDroppedPdf]
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

  const handleShowAbout = useCallback(() => {
    setIsAboutDialogOpen(true);
  }, []);

  const handleCloseAbout = useCallback(() => {
    setIsAboutDialogOpen(false);
  }, []);

  const handleShowHelp = useCallback(() => {
    setIsHelpDialogOpen(true);
  }, []);

  const handleCloseHelp = useCallback(() => {
    setIsHelpDialogOpen(false);
  }, []);

  useEffect(() => {
    const unsubscribeDragState = bridge.on(
      "externalDragState",
      ({ isActive }) => {
        setIsDragActive(isActive);
      }
    );

    const unsubscribeExternalDrop = bridge.on(
      "externalDrop",
      ({ path, formats }) => {
        console.groupCollapsed(
          `[drop][host] externalDrop received: path=${path ?? "(none)"}`
        );
        console.info("[drop][host] available formats", formats);
        console.groupEnd();
        processDroppedPdf(path, { source: "host", formats });
      }
    );

    return () => {
      unsubscribeDragState();
      unsubscribeExternalDrop();
    };
  }, [processDroppedPdf]);

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
          onShowAbout={handleShowAbout}
          onShowHelp={handleShowHelp}
        />
      </div>

      <AboutDialog isOpen={isAboutDialogOpen} onClose={handleCloseAbout} />
      <HelpDialog isOpen={isHelpDialogOpen} onClose={handleCloseHelp} />
    </div>
  );
}

export default App;
