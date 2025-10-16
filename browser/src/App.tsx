import { useCallback, useEffect, useMemo, useState } from "react";
import { LayoutChooser } from "./components/LayoutChooser";
import { FooterControls } from "./components/FooterControls";
import { PreviewPane } from "./components/PreviewPane";
import { AboutDialog } from "./components/AboutDialog";
import { HelpDialog } from "./components/HelpDialog";
import { DevModeIndicator } from "./components/DevModeIndicator";
import {
  bridge,
  type GenerationStatus,
  type LayoutMethodSummary,
  type PaperTargetInfo,
  type RuntimeInfo,
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

function App() {
  const [workspaceState, setWorkspaceState] = useState<WorkspaceState | null>(
    null,
  );
  const [layouts, setLayouts] = useState<LayoutMethodSummary[]>([]);
  const [paperTargets, setPaperTargets] = useState<PaperTargetInfo[]>([]);
  const [generationStatus, setGenerationStatus] =
    useState<GenerationStatus | null>(null);
  const [isBootstrapping, setIsBootstrapping] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isErrorFading, setIsErrorFading] = useState(false);
  const [isDragActive, setIsDragActive] = useState(false);
  const [isAboutDialogOpen, setIsAboutDialogOpen] = useState(false);
  const [isHelpDialogOpen, setIsHelpDialogOpen] = useState(false);
  const [runtimeInfo, setRuntimeInfo] = useState<RuntimeInfo | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function bootstrap() {
      try {
        const [state, layoutSummaries, paperSummaries, runtime] =
          await Promise.all([
            bridge.requestState(),
            bridge.requestLayouts(),
            bridge.requestPaperTargets(),
            bridge.getRuntimeInfo(),
          ]);

        if (!isMounted) {
          return;
        }

        setWorkspaceState(state);
        setLayouts(layoutSummaries);
        setPaperTargets(paperSummaries);
        setRuntimeInfo(runtime);
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
        current ? { ...current, generatedPdfPath: path } : current,
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
            "An unexpected error occurred while processing the command.",
          );
        }
      }
    },
    [],
  );

  const processDroppedPdf = useCallback(
    (
      rawPath: string | null | undefined,
      context: { source: "dom" | "host"; formats?: string[] } = {
        source: "dom",
      },
    ) => {
      const descriptor = context.source === "host" ? "[drop][host]" : "[drop]";

      if (!rawPath) {
        console.warn(
          `${descriptor} No usable path was resolved from the drop payload`,
          context.formats ?? [],
        );
        setErrorMessage(
          "We couldn't read the dropped file path. Try dropping a PDF from File Explorer or use the Choose button.",
        );
        return;
      }

      const normalized = rawPath.trim();
      if (!normalized) {
        console.warn(`${descriptor} Dropped path contained only whitespace.`);
        setErrorMessage(
          "We couldn't read the dropped file path. Try dropping a PDF from File Explorer or use the Choose button.",
        );
        return;
      }

      if (!normalized.toLowerCase().endsWith(".pdf")) {
        console.warn(
          `${descriptor} Dropped file did not have a .pdf extension`,
          {
            path: normalized,
            formats: context.formats,
          },
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
    [runCommand],
  );

  const handlePickPdf = useCallback(() => {
    if (runtimeInfo?.mode === "stub") {
      setErrorMessage(
        "🔧 Developer Mode: The .NET backend is not running. PDF file picking requires the full application. " +
          "To test with the full backend, run from Visual Studio or 'dotnet run' from the dotnet/ directory.",
      );
      return;
    }
    return runCommand(() => bridge.pickPdf());
  }, [runCommand, runtimeInfo]);

  const handleReloadPrevious = useCallback(() => {
    if (runtimeInfo?.mode === "stub") {
      setErrorMessage(
        "🔧 Developer Mode: The .NET backend is not running. Reloading previous PDF requires the full application. " +
          "To test with the full backend, run from Visual Studio or 'dotnet run' from the dotnet/ directory.",
      );
      return;
    }
    return runCommand(() => bridge.reloadPrevious());
  }, [runCommand, runtimeInfo]);

  const handleSelectLayout = useCallback(
    (layoutId: string) => runCommand(() => bridge.setLayout(layoutId)),
    [runCommand],
  );

  const handleSelectPaper = useCallback(
    (paperId: string) => runCommand(() => bridge.setPaper(paperId)),
    [runCommand],
  );

  const handleToggleMirror = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setMirror(enabled)),
    [runCommand],
  );

  const handleToggleRtl = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setRightToLeft(enabled)),
    [runCommand],
  );

  const handleToggleCropMarks = useCallback(
    (enabled: boolean) => runCommand(() => bridge.setCropMarks(enabled)),
    [runCommand],
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
    if (!errorMessage) {
      setIsErrorFading(false);
      return;
    }

    setIsErrorFading(false);
    const fadeTimer = setTimeout(() => {
      setIsErrorFading(true);
    }, 2000);

    const clearTimer = setTimeout(() => {
      setErrorMessage(null);
      setIsErrorFading(false);
    }, 2500);

    return () => {
      clearTimeout(fadeTimer);
      clearTimeout(clearTimer);
    };
  }, [errorMessage]);

  useEffect(() => {
    const unsubscribeDragState = bridge.on(
      "externalDragState",
      ({ isActive }) => {
        setIsDragActive(isActive);
      },
    );

    const unsubscribeExternalDrop = bridge.on(
      "externalDrop",
      ({ path, formats }) => {
        console.groupCollapsed(
          `[drop][host] externalDrop received: path=${path ?? "(none)"}`,
        );
        console.info("[drop][host] available formats", formats);
        console.groupEnd();
        processDroppedPdf(path, { source: "host", formats });
      },
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
      <DevModeIndicator />
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
        />

        {errorMessage && (
          <div
            className={`flex-none rounded-xl border border-red-200 bg-red-50 px-4 py-2 text-sm text-red-700 transition-opacity duration-500 ${
              isErrorFading ? "opacity-0" : "opacity-100"
            }`}
          >
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
