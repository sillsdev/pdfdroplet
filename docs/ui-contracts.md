# React UI ↔ PdfDroplet Contracts

This document captures the data shapes and workflows the new React UI must support based on the existing WinForms implementation (`WorkspaceControl`, `WorkSpaceViewModel`, and layout classes).

## Core Concepts

| Concept | Description | Current Source |
| --- | --- | --- |
| Incoming PDF | The source PDF file dropped/selected by the user. | `WorkSpaceViewModel._incomingPath` |
| Layout Method | Strategy that rearranges pages when building the booklet (e.g., side-fold, calendar). | `PdfDroplet.LayoutMethods.*` |
| Paper Target | Printer paper configuration (A4, Letter, etc.). | `WorkSpaceViewModel.PaperChoices` |
| Generated PDF | Temporary booklet produced for preview/printing. | `_pathToCurrentlyDisplayedPdf` in `WorkSpaceViewModel` |

## Data Structures

These shapes should be mirrored in TypeScript for a typed RPC boundary.

### `LayoutMethodSummary`
- `id` (`string`): unique key (e.g., fully qualified type name).
- `displayName` (`string`): text shown in the sidebar (comes from `LayoutMethod.ToString()`).
- `thumbnail` (`string`): base64 PNG rendered via existing `LayoutMethod.GetImage(...)` resized as today.
- `isEnabled` (`boolean`): matches `LayoutMethod.GetIsEnabled` for the current input PDF.
- `isOrientationSensitive` (`boolean`): whether orientation affects the thumbnail (drives refresh logic).

### `PaperTargetInfo`
- `id` (`string`): canonical name (`PaperTarget.Name`).
- `displayName` (`string`): same as `id` for now.
- `paperSize` (`{ width: number, height: number, unit: 'point' }`): pulled from `PaperSize`.

### `WorkspaceState`
- `hasIncomingPdf` (`boolean`)
- `incomingPath` (`string | null`)
- `selectedLayoutId` (`string | null`)
- `selectedPaperId` (`string`)
- `mirror` (`boolean`)
- `rightToLeft` (`boolean`)
- `showCropMarks` (`boolean`)
- `generatedPdfPath` (`string | null`)
- `canReloadPrevious` (`boolean`)
- `previousIncomingFilename` (`string | null`)

### `GenerationStatus`
- `state` (`'idle' | 'working' | 'error' | 'success'`)
- `message` (`string`)
- `error` (`{ message: string, details?: string } | null`)

## Commands (JavaScript → C#)

| Command | Payload | Behavior |
| --- | --- | --- |
| `pickPdf` | none | Opens file dialog, updates `_incomingPath`, triggers layout.
| `dropPdf` | `{ path: string }` | Validates extension, checks lock via `IsAlreadyOpenElsewhere`, updates model.
| `setLayout` | `{ layoutId: string }` | Calls `SetLayoutMethod`.
| `setPaper` | `{ paperId: string }` | Calls `SetPaperTarget` and re-layouts.
| `setMirror` | `{ enabled: boolean }` | Calls `SetMirror`.
| `setRightToLeft` | `{ enabled: boolean }` | Calls `SetRightToLeft`.
| `setCropMarks` | `{ enabled: boolean }` | Calls `ShowCropMarks`.
| `reloadPrevious` | none | Invokes `ReloadPrevious` when available.
| `requestState` | none | Responds with full `WorkspaceState` snapshot.
| `requestLayouts` | none | Returns `LayoutMethodSummary[]`.
| `requestPaperTargets` | none | Returns `PaperTargetInfo[]`.

## Events (C# → JavaScript)

| Event | Payload | Trigger |
| --- | --- | --- |
| `stateChanged` | `WorkspaceState` | Any setter that affects persisted state (`SetPath`, `SetLayoutMethod`, toggles, `ReloadPrevious`).
| `layoutsChanged` | `LayoutMethodSummary[]` | After `PopulateLayoutList`/`UpdateDisplay` recalculations.
| `generationStatus` | `GenerationStatus` | During `ClearThenContinue` → `ContinueConversionAndNavigation` workflow.
| `generatedPdfReady` | `{ path: string }` | After `_browser.Navigate` call.

## Workflow Notes

1. **Initial Load**
   - React requests `WorkspaceState`, layout options, and paper targets once the bridge initializes.
   - View model responds using persisted `Settings.Default` values.

2. **Selecting/Dragging PDFs**
   - `SetPath` opens the document (`XPdfForm.FromFile`) and kicks off `SetLayoutMethod(new NullLayoutMethod())`, which eventually runs `ContinueConversionAndNavigation` and navigates the WebView to the generated temp PDF.
   - Any errors (file locked, conversion failure) surface through `ErrorReport.NotifyUserOfProblem`; the bridge should capture these and emit `generationStatus` updates.

3. **Layout/Paper Changes**
   - Changing layout/paper toggles re-run `ContinueConversionAndNavigation`. React should show a busy indicator until `generatedPdfReady` fires.

4. **Settings Toggles**
   - Mirror/RTL/Crop toggles persist immediately to `Settings.Default` and retrigger the layout pipeline.

5. **Previous Session**
   - `Settings.Default.PreviousIncomingPath` enables "Open Previous". Bridge exposes this via `WorkspaceState` so React can show the action conditionally.

## File Access Considerations

- Generated files are stored in the user's temp directory (`Path.GetTempPath()`). React should treat `generatedPdfPath` as an opaque string and load it via `<iframe src={file:///...}>`.
- Before overwriting a generated file, `DeleteFileThatMayBeInUse` attempts removal. If deletion fails, C# sends an error message that must be surfaced to the user.
- Drag/drop validation currently happens in WinForms; the React UI should replicate the messaging logic (`That file doesn't end in '.pdf'`, `Looks good, drop it.`, etc.).

## Open Work Items

- Decide whether to expose additional telemetry hooks in the bridge (current maintainer guidance says no extra UI required).
- Identify which error dialogs (`ErrorReport.NotifyUserOfProblem`) should become toast notifications or modal dialogs in React.
