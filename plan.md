# PDF Droplet WebView2 + React Migration Plan

## 🧭 Context

PdfDroplet is currently a WinForms application whose main UX is implemented in `WorkspaceControl`. It already embeds a WinForms `WebView2` control for PDF preview, while other interactions (file picking, layout options, output generation) are built with classic WinForms widgets bound to `WorkSpaceViewModel`. We want to replace the WinForms chrome with a modern React interface that is hosted inside WebView2, built with Vite, TypeScript, and Tailwind CSS. The existing PDF preview will remain, but will be rendered as an `<iframe>` inside the React UI that points at the generated booklet PDF.

## 🎯 Goals & Success Criteria

- Deliver an in-process WebView2-hosted React SPA that fully replaces the WinForms UI controls.
- Preserve all current capabilities: drag/drop PDFs, show layout choices, update settings (paper size, mirror, RTL, crop marks), launch help/about, reload previous files, and trigger booklet generation.
- Keep business logic, file processing, and PDF generation in C# with minimal disruption.
- Maintain offline support and installer footprint comparable to today.
- Provide a developer workflow where the React app can run in dev mode (hot reload) and be bundled into the desktop build via Vite for production.

## 🔧 Assumptions & Constraints

- We will continue targeting .NET Framework/WinForms (per `PdfDroplet.csproj`) while factoring out WinForms UI pieces.
- Build pipelines can install Node.js ≥ 18 to run Vite builds.
- WebView2 runtime is already a dependency; no extra browser engine needed.
- Existing PDF generation APIs (`WorkSpaceViewModel`, `LayoutMethod`, etc.) remain authoritative and will be surfaced to the web UI through a new interop layer.
- Localization (resources under `publish/<lang>`) must keep working; future React i18n strategy should reuse existing resource strings where practical.

## 📦 Deliverables

- `ui/` directory inside this repository containing the Vite + React + TypeScript + Tailwind project.
- New C# host layer that initializes WebView2, loads the bundled React assets, and bridges calls between JavaScript and existing view models.
- Updated WinForms shell (likely `MainWindow` + `WorkspaceControl`) trimmed to host only the WebView2 control and any required compatibility chrome.
- Build scripts (MSBuild targets or post-build events) to:
  - Run `npm install`/`npm run build` for production packaging.
  - Copy the generated static assets into the application resources/dist folder consumed by WebView2.
- Developer documentation covering dev server usage, test strategy, and release build steps (single-repo workflow).
- Automated end-to-end Playwright test (no mocks) that exercises the packaged UI, with room to add more scenarios as needed.

## 🏗️ Target Architecture Overview

- **Shell**: WinForms window containing a single `WebView2` control.
- **Frontend**: React SPA rendered via WebView2. Tailwind handles styling; component routing is minimal (likely single view with modals/panels).
- **Interop Layer**: C# class responsible for exposing operations (open file dialog, generate booklet, fetch settings, load PDF preview URL) to JavaScript and receiving commands/events from the SPA.
- **PDF Preview**: React renders an `<iframe>` whose `src` points to the latest generated PDF file; the host ensures files are accessible (e.g., `file:///` URLs or temporary HTTP endpoint).
- **State Flow**: React manages UI state, while authoritative data (paper sizes, layout definitions, persisted settings) live in C# and are synced on demand through the interop bridge.

## ✅ Task Checklist

- [x] Audit existing WinForms/UI flows in `WorkspaceControl`, `WorkSpaceViewModel`, and associated layout classes; define the data/contracts the React app needs. (See `docs/ui-contracts.md`.)
- [x] Carve out a dedicated C# interop service (`IWorkspaceUiBridge`) that encapsulates file picking, layout selection, settings persistence, and booklet generation hooks. (See `src/Interop/`.)
- [ ] Scaffold the Vite + React + TypeScript + Tailwind project inside `ui/` and recreate the existing layout (sidebar choices, preview pane, footer controls).
- [ ] Implement drag/drop and file selection through the interop service without mocks, wiring UI state to the C# view model.
- [ ] Build a WebView2 messaging bridge using `window.chrome.webview.postMessage` and `CoreWebView2.WebMessageReceived`, wrapped in a minimal RPC layer with request IDs and typed payloads.
- [ ] Expose layout metadata, thumbnails, and persisted settings via the RPC bridge; ensure React components refresh when C# state changes.
- [ ] Stream PDF generation progress through the bridge and update the React UI with status, errors, and success notifications.
- [ ] Render the generated booklet PDF in an `<iframe>` and manage file lifecycle (refresh detection, cleanup of temporary files).
- [ ] Integrate the Vite production build into MSBuild, copy the bundle into app resources, and update `PdfDroplet.iss` packaging.
- [ ] Add a Playwright end-to-end test that launches the bundled UI and verifies the primary screen (sidebar, controls, iframe) renders correctly using the real bridge.
- [ ] Document developer workflows for dev server/hot reload, build, and debugging the interop layer.
- [ ] Remove obsolete WinForms controls/resources once the React UI is feature-complete and update screenshots/documentation accordingly.

## 🔌 Web ↔ C# Communication Strategy

We will rely on WebView2 web messages end-to-end:

- JavaScript sends commands via `window.chrome.webview.postMessage`, and C# handles them inside `CoreWebView2.WebMessageReceived`.
- Responses and events flow back through `CoreWebView2.PostWebMessageAsJson`.
- A thin RPC wrapper will package requests with IDs, enforce typed payloads, and surface async results as promises. Errors move through a standardized `{ errorCode, message }` structure.

Alternative approaches (host object injection or an embedded HTTP server) remain fallback options if we later hit message size limits or need external automation, but they are out of scope for this implementation.

## 🧱 React Application Structure

- `src/App.tsx`: top-level layout with sidebar, main preview, footer controls.
- `src/features/layouts`: components for layout selection, thumbnails, state syncing.
- `src/features/settings`: toggles for mirror, RTL, crop marks; persists via bridge calls.
- `src/features/files`: drag/drop zone, choose file button, status messaging.
- `src/features/preview`: iframe + refresh indicator.
- `src/lib/bridge.ts`: strongly typed client for WebMessage RPC (request/response, event subscriptions).
- `src/state/store.ts`: Zustand or Redux Toolkit store to coordinate global state and caching of values from C#.
- Tailwind config with design tokens matching current branding (colors sourced from existing WinForms UI or `Resources`).

## 🧪 Testing Strategy

- **End-to-End**: One Playwright test that boots the packaged WebView2 UI (no mocks) and asserts the primary workspace renders correctly (sidebar, layout controls, preview iframe, footer toggles). Extend with additional scenarios as coverage grows.
- **Frontend Unit Tests (if needed)**: Vitest with React Testing Library for isolated component logic or regression fixes. Skip by default unless a behavior is hard to verify via Playwright.
- **Interop Validation**: Prefer exercising the real RPC bridge through Playwright. Reserve C# unit tests for pure business logic unrelated to the messaging layer.

## 🚢 Deployment & Packaging

- Extend MSBuild to run the Vite build in `Release` configuration. Fail builds if the frontend bundle is missing or outdated.
- Embed static assets as `Resource` or copy to `output/Release/Ui` and configure WebView2 to load via `CoreWebView2.NavigateToString`/`SetVirtualHostNameToFolderMapping`.
- Update Inno Setup (`PdfDroplet.iss`) to include the new asset directory.
- Document runtime requirements (WebView2 Evergreen Runtime, NA).

## ⚠️ Risks & Mitigations

- **Interop Complexity**: Mitigate with a small typed RPC layer and shared TypeScript definitions generated from C# models (e.g., via `TypeGen`).
- **Drag/Drop Fidelity**: WebView2 handles drag/drop differently than WinForms; consider overlay drop zone in WinForms layer or use WebView2's `AddWebResourceRequestedFilter` for file access.
- **PDF Accessibility**: iframe needs proper fallback if PDF viewer fails; show error messaging and allow opening externally.
- **Localization Drift**: React components must load string tables from existing `.resx` files (export to JSON at build time) to avoid regressions.
- **Bundle Size**: Keep Vite bundle small; tree-shake icons, prefer CSS utilities, and lazy-load heavy components.

## ✅ Definition of Done

- React UI feature parity confirmed via acceptance checklist.
- Automated test suite (C# + JS) green in CI.
- Installer builds include new assets and pass smoke tests on clean Windows machine.
- Telemetry, error reporting, and update mechanism continue functioning.

## 📝 Maintainer Decisions

- The React UI lives in this repository alongside the C# code; no submodule split.
- No upcoming features require adjustments to the planned layout.
- Accessibility expectations are satisfied by WebView2 defaults; no extra assistive integrations.
- No dedicated analytics/telemetry UI changes are needed beyond existing behavior.
- Once the React UI ships, we can remove the legacy WinForms chrome rather than maintaining a fallback.
