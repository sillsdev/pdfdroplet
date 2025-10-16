export type GenerationState = "idle" | "working" | "success" | "error";

export type GenerationError = {
  message: string;
  details?: string;
};

export type GenerationStatus = {
  state: GenerationState;
  message: string;
  error: GenerationError | null;
};

export type LayoutMethodSummary = {
  id: string;
  displayName: string;
  thumbnailImage: string;
  isEnabled: boolean;
  isOrientationSensitive: boolean;
};

export type PaperTargetInfo = {
  id: string;
  displayName: string;
  widthPoints: number;
  heightPoints: number;
};

export type WorkspaceState = {
  hasIncomingPdf: boolean;
  incomingPath: string;
  selectedLayoutId: string;
  selectedPaperId: string;
  mirror: boolean;
  rightToLeft: boolean;
  showCropMarks: boolean;
  generatedPdfPath: string;
  canReloadPrevious: boolean;
  previousIncomingFilename: string;
};

export type RuntimeMode = "bundle" | "devServer" | "stub";

export type RuntimeInfo = {
  mode: RuntimeMode;
  isDevMode: boolean;
  version: string;
};

export type SaveResult = {
  success: boolean;
  savedPath: string;
};

export type WorkspaceCommands =
  | "requestState"
  | "requestLayouts"
  | "requestPaperTargets"
  | "getRuntimeInfo"
  | "pickPdf"
  | "dropPdf"
  | "reloadPrevious"
  | "setLayout"
  | "setPaper"
  | "setMirror"
  | "setRightToLeft"
  | "setCropMarks"
  | "saveBooklet";

export type WorkspaceEvents =
  | "stateChanged"
  | "layoutsChanged"
  | "generationStatus"
  | "generatedPdfReady"
  | "externalDragState"
  | "externalDrop";

export type EventPayloadMap = {
  stateChanged: WorkspaceState;
  layoutsChanged: LayoutMethodSummary[];
  generationStatus: GenerationStatus;
  generatedPdfReady: { path: string };
  externalDragState: { isActive: boolean };
  externalDrop: { path: string | null; formats: string[] };
};

export type RpcErrorPayload = {
  code: string;
  message: string;
  details?: string;
};

export type RpcResponseEnvelope<Result = unknown> = {
  type: "response";
  id: string;
  result?: Result;
  error?: RpcErrorPayload;
};

export type RpcEventEnvelope<Event extends WorkspaceEvents = WorkspaceEvents> =
  {
    type: "event";
    event: Event;
    payload: EventPayloadMap[Event];
  };

export type RpcRequestEnvelope<Params = unknown> = {
  type: "request";
  id: string;
  method: WorkspaceCommands;
  params?: Params;
};
