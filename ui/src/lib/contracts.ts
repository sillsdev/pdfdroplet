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
  thumbnailBase64: string;
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

export type WorkspaceCommands =
  | "requestState"
  | "requestLayouts"
  | "requestPaperTargets"
  | "pickPdf"
  | "dropPdf"
  | "reloadPrevious"
  | "setLayout"
  | "setPaper"
  | "setMirror"
  | "setRightToLeft"
  | "setCropMarks";

export type WorkspaceEvents =
  | "stateChanged"
  | "layoutsChanged"
  | "generationStatus"
  | "generatedPdfReady";

export type EventPayloadMap = {
  stateChanged: WorkspaceState;
  layoutsChanged: LayoutMethodSummary[];
  generationStatus: GenerationStatus;
  generatedPdfReady: { path: string };
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
