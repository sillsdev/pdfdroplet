import type {
  EventPayloadMap,
  GenerationStatus,
  GenerationState,
  LayoutMethodSummary,
  PaperTargetInfo,
  RpcErrorPayload,
  RuntimeInfo,
  RuntimeMode,
  WorkspaceCommands,
  WorkspaceEvents,
  WorkspaceState,
} from "./contracts";

export type Listener<Event extends WorkspaceEvents> = (
  payload: EventPayloadMap[Event],
) => void;

export interface WorkspaceBridge {
  requestState(): Promise<WorkspaceState>;
  requestLayouts(): Promise<LayoutMethodSummary[]>;
  requestPaperTargets(): Promise<PaperTargetInfo[]>;
  getRuntimeInfo(): Promise<RuntimeInfo>;
  pickPdf(): Promise<WorkspaceState>;
  dropPdf(path: string): Promise<WorkspaceState>;
  reloadPrevious(): Promise<WorkspaceState>;
  setLayout(layoutId: string): Promise<WorkspaceState>;
  setPaper(paperId: string): Promise<WorkspaceState>;
  setMirror(enabled: boolean): Promise<WorkspaceState>;
  setRightToLeft(enabled: boolean): Promise<WorkspaceState>;
  setCropMarks(enabled: boolean): Promise<WorkspaceState>;
  on<Event extends WorkspaceEvents>(
    event: Event,
    listener: Listener<Event>,
  ): () => void;
}

type PendingRequest<Result> = {
  resolve: (value: Result | PromiseLike<Result>) => void;
  reject: (reason?: unknown) => void;
};

type EventMap = {
  [Event in WorkspaceEvents]: Set<Listener<Event>>;
};

interface WebViewHost {
  postMessage(message: unknown): void;
  addEventListener(
    type: "message",
    listener: (event: MessageEvent) => void,
  ): void;
  removeEventListener(
    type: "message",
    listener: (event: MessageEvent) => void,
  ): void;
}

declare global {
  interface Window {
    chrome?: {
      webview?: WebViewHost;
    };
  }
}

class BridgeError extends Error {
  readonly code: string;
  readonly details?: string;

  constructor(code: string, message: string, details?: string) {
    super(message);
    this.name = "BridgeError";
    this.code = code;
    this.details = details;
  }
}

class WebViewRuntimeBridge implements WorkspaceBridge {
  private readonly webview: WebViewHost;
  private readonly pending = new Map<string, PendingRequest<unknown>>();
  private readonly listeners: EventMap = {
    stateChanged: new Set(),
    layoutsChanged: new Set(),
    generationStatus: new Set(),
    generatedPdfReady: new Set(),
    externalDragState: new Set(),
    externalDrop: new Set(),
  };
  private nextRequestId = 1;

  constructor(host: WebViewHost) {
    this.webview = host;
    this.webview.addEventListener("message", this.handleMessage);
  }

  requestState(): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("requestState");
  }

  requestLayouts(): Promise<LayoutMethodSummary[]> {
    return this.invoke<LayoutMethodSummary[]>("requestLayouts");
  }

  requestPaperTargets(): Promise<PaperTargetInfo[]> {
    return this.invoke<PaperTargetInfo[]>("requestPaperTargets");
  }

  getRuntimeInfo(): Promise<RuntimeInfo> {
    return this.invoke<RuntimeInfo>("getRuntimeInfo");
  }

  pickPdf(): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("pickPdf");
  }

  dropPdf(path: string): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("dropPdf", { path });
  }

  reloadPrevious(): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("reloadPrevious");
  }

  setLayout(layoutId: string): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("setLayout", { layoutId });
  }

  setPaper(paperId: string): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("setPaper", { paperId });
  }

  setMirror(enabled: boolean): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("setMirror", { enabled });
  }

  setRightToLeft(enabled: boolean): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("setRightToLeft", { enabled });
  }

  setCropMarks(enabled: boolean): Promise<WorkspaceState> {
    return this.invoke<WorkspaceState>("setCropMarks", { enabled });
  }

  on<Event extends WorkspaceEvents>(
    event: Event,
    listener: Listener<Event>,
  ): () => void {
    const bucket = this.listeners[event] as Set<Listener<Event>>;
    bucket.add(listener);
    return () => bucket.delete(listener);
  }

  private invoke<Result>(
    method: WorkspaceCommands,
    params?: Record<string, unknown>,
  ): Promise<Result> {
    const id = (this.nextRequestId++).toString();
    const envelope = {
      type: "request" as const,
      id,
      method,
      params,
    };

    return new Promise<Result>((resolve, reject) => {
      this.pending.set(id, { resolve, reject } as PendingRequest<unknown>);
      this.webview.postMessage(envelope);
    });
  }

  private handleMessage = (event: MessageEvent) => {
    const data = event.data;
    if (!data || typeof data !== "object") {
      return;
    }

    if (data.type === "response" && typeof data.id === "string") {
      const pending = this.pending.get(data.id);
      if (!pending) {
        return;
      }

      this.pending.delete(data.id);
      const errorPayload: RpcErrorPayload | undefined = data.error;
      if (errorPayload) {
        pending.reject(
          new BridgeError(
            errorPayload.code,
            errorPayload.message,
            errorPayload.details,
          ),
        );
      } else {
        pending.resolve(data.result as unknown);
      }
      return;
    }

    if (data.type === "event" && typeof data.event === "string") {
      if (data.event === "bridgeReady") {
        // no-op: bridge readiness is implicit when this class is constructed
        return;
      }

      const listeners = this.listeners[data.event as WorkspaceEvents];
      if (!listeners || listeners.size === 0) {
        return;
      }

      for (const listener of listeners) {
        listener(data.payload);
      }
    }
  };
}

const stubLayouts: LayoutMethodSummary[] = [
  {
    id: "side-fold",
    displayName: "Side-Fold Booklet",
    thumbnailImage: "sideFoldBooklet.svg",
    isEnabled: true,
    isOrientationSensitive: true,
  },
  {
    id: "calendar",
    displayName: "Calendar Style",
    thumbnailImage: "calendar.svg",
    isEnabled: true,
    isOrientationSensitive: false,
  },
];

const stubPaperTargets: PaperTargetInfo[] = [
  {
    id: "letter",
    displayName: "Letter (8½×11 in)",
    widthPoints: 612,
    heightPoints: 792,
  },
  {
    id: "legal",
    displayName: "Legal (8½×14 in)",
    widthPoints: 612,
    heightPoints: 1008,
  },
  {
    id: "a4",
    displayName: "A4 (210×297 mm)",
    widthPoints: 595.28,
    heightPoints: 841.89,
  },
];

class DevStubBridge implements WorkspaceBridge {
  private state: WorkspaceState = {
    hasIncomingPdf: false,
    incomingPath: "",
    selectedLayoutId: stubLayouts[0].id,
    selectedPaperId: stubPaperTargets[0].id,
    mirror: false,
    rightToLeft: false,
    showCropMarks: false,
    generatedPdfPath: "",
    canReloadPrevious: false,
    previousIncomingFilename: "",
  };

  private readonly listeners: EventMap = {
    stateChanged: new Set(),
    layoutsChanged: new Set(),
    generationStatus: new Set(),
    generatedPdfReady: new Set(),
    externalDragState: new Set(),
    externalDrop: new Set(),
  };

  async requestState(): Promise<WorkspaceState> {
    return this.state;
  }

  async requestLayouts(): Promise<LayoutMethodSummary[]> {
    return stubLayouts;
  }

  async requestPaperTargets(): Promise<PaperTargetInfo[]> {
    return stubPaperTargets;
  }

  async getRuntimeInfo(): Promise<RuntimeInfo> {
    return {
      mode: "stub",
      isDevMode: true,
      version: "0.0.0-dev",
    };
  }

  async pickPdf(): Promise<WorkspaceState> {
    console.info("[dev-bridge] pickPdf invoked (no backend available)");
    throw new BridgeError(
      "NO_BACKEND",
      "🔧 Developer Mode: PDF file picking requires the .NET backend. Run 'dotnet run' from the dotnet/ directory to test with the full application.",
      "The stub bridge cannot open file dialogs. Use the full .NET application to test this feature.",
    );
  }

  async dropPdf(path: string): Promise<WorkspaceState> {
    console.info(
      "[dev-bridge] dropPdf invoked with",
      path,
      "(no backend available)",
    );
    throw new BridgeError(
      "NO_BACKEND",
      "🔧 Developer Mode: PDF processing requires the .NET backend. Run 'dotnet run' from the dotnet/ directory to test with the full application.",
      `The stub bridge cannot process PDF files. Attempted to process: ${path}`,
    );
  }

  async reloadPrevious(): Promise<WorkspaceState> {
    console.info("[dev-bridge] reloadPrevious invoked (no backend available)");
    throw new BridgeError(
      "NO_BACKEND",
      "🔧 Developer Mode: Reloading previous PDF requires the .NET backend. Run 'dotnet run' from the dotnet/ directory to test with the full application.",
      "The stub bridge has no previous PDF state to reload.",
    );
  }

  async setLayout(layoutId: string): Promise<WorkspaceState> {
    this.publishStatus("working", "Updating layout…");
    this.state = { ...this.state, selectedLayoutId: layoutId };
    this.emit("stateChanged", this.state);
    this.publishStatus("success", "Preview updated.");
    return this.state;
  }

  async setPaper(paperId: string): Promise<WorkspaceState> {
    this.publishStatus("working", "Switching paper size…");
    this.state = { ...this.state, selectedPaperId: paperId };
    this.emit("stateChanged", this.state);
    this.publishStatus("success", "Preview updated.");
    return this.state;
  }

  async setMirror(enabled: boolean): Promise<WorkspaceState> {
    this.publishStatus(
      "working",
      enabled ? "Mirroring pages…" : "Updating booklet…",
    );
    this.state = { ...this.state, mirror: enabled };
    this.emit("stateChanged", this.state);
    this.publishStatus("success", "Preview updated.");
    return this.state;
  }

  async setRightToLeft(enabled: boolean): Promise<WorkspaceState> {
    this.publishStatus(
      "working",
      enabled ? "Applying right-to-left layout…" : "Updating text flow…",
    );
    this.state = { ...this.state, rightToLeft: enabled };
    this.emit("stateChanged", this.state);
    this.publishStatus("success", "Preview updated.");
    return this.state;
  }

  async setCropMarks(enabled: boolean): Promise<WorkspaceState> {
    this.publishStatus(
      "working",
      enabled ? "Adding crop marks…" : "Removing crop marks…",
    );
    this.state = { ...this.state, showCropMarks: enabled };
    this.emit("stateChanged", this.state);
    this.publishStatus("success", "Preview updated.");
    return this.state;
  }

  on<Event extends WorkspaceEvents>(
    event: Event,
    listener: Listener<Event>,
  ): () => void {
    const bucket = this.listeners[event] as Set<Listener<Event>>;
    bucket.add(listener);
    return () => bucket.delete(listener);
  }

  private emit<Event extends WorkspaceEvents>(
    event: Event,
    payload: EventPayloadMap[Event],
  ) {
    const bucket = this.listeners[event] as Set<Listener<Event>>;
    for (const listener of bucket) {
      listener(payload);
    }
  }

  private publishStatus(state: GenerationState, message: string) {
    const status: GenerationStatus = {
      state,
      message,
      error: null,
    };

    this.emit("generationStatus", status);
  }
}

function createWorkspaceBridge(): WorkspaceBridge {
  const webview = window.chrome?.webview;
  if (webview) {
    return new WebViewRuntimeBridge(webview);
  }

  if (import.meta.env.DEV) {
    console.warn("[dev-bridge] WebView2 host not detected; using stub bridge.");
  }

  return new DevStubBridge();
}

export const bridge: WorkspaceBridge = createWorkspaceBridge();

export type {
  WorkspaceState,
  LayoutMethodSummary,
  PaperTargetInfo,
  GenerationStatus,
  RuntimeInfo,
  RuntimeMode,
};
