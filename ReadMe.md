PdfDroplet is a free, simple little GUI tool which only does one thing: it takes your PDF and gives you a new one, with the pages combined and reordered, ready for saving and printing as booklets.

`PdfDroplet.exe` can also be used as a library for making booklets in other programs. [Bloom](https://github.com/BloomBooks/BloomDesktop) uses it this way.

The GUI has only been released on Windows, but on Linux PdfDroplet has been used as a non-GUI library.

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) (Visual Studio 2022 17.8+ works out of the box).
- [Node.js 18+](https://nodejs.org/) for the React/Vite frontend build.
- Microsoft Edge WebView2 Runtime (normally installed automatically with Edge; installers should ship it as well).

## Building

Run `dotnet build PdfDroplet.sln` (or build inside Visual Studio). MSBuild now restores the frontend dependencies and runs the Vite production build automatically:

- `npm install` executes once per checkout (tracked by `ui/node_modules/.msbuild-stamp`).
- `npm run build` produces `ui/dist` which is copied to `output/<Configuration>/ui-dist`.
- `dotnet publish -r win-x86` includes the same files under `output/<Configuration>/win-x86/publish/ui-dist` so the installer can package them.

You do **not** need to run any separate scripts unless you are iterating on the frontend. Clean operations (`dotnet clean`) remove both the compiled binaries and the copied `ui-dist` folders.

## Frontend Development

1. From the `ui/` directory run `npm install` (first time) and then `npm run dev`.
2. Launch PdfDroplet from Visual Studio or `dotnet run`.
3. In Debug builds PdfDroplet automatically tries `http://localhost:5173`; you can override the URL by setting the `PDFDROPLET_UI_DEV_SERVER` environment variable before starting the app.
4. Hot reload happens entirely in the browser—no rebuild of the C# project is required unless you change the interop contracts.

When the dev server is unavailable PdfDroplet falls back to the packaged assets, so you can keep working offline by running a full build.

### Frontend Testing

All frontend npm scripts live in the `ui/` workspace. Run them from that directory (or pass `--prefix ui`) so `npm` can find the correct `package.json`. For example:

- `cd ui && npm run test:e2e -- --list` lists the Playwright scenarios without launching the app.
- `cd ui && npm run test:e2e` executes the smoke test, which boots the WinForms host and verifies the WebView bridge end to end.

### Debugging the WebView and Bridge

- Set the environment variable `PDFDROPLET_AUTOMATION_PORT=9222` (or another free port) before launching PdfDroplet to enable WebView2 remote debugging. You can then attach Playwright or open `edge://inspect` to poke around the live UI.
- The bridge uses JSON-RPC envelopes. Inspect `ui/src/lib/bridge.ts` for the client and `WorkspaceControl.cs` for the server.
- Preview PDFs are generated into a rotating temp directory. If you work on file lifecycle logic, remember the C# side provides cleanup via `WorkSpaceViewModel.DisposeWorkspaceResources`.

## Legacy Preview Requirement

To get a preview of the output inside an older PdfDroplet build you needed an Internet Explorer compatible PDF viewer (Acrobat Reader, PDF-XChange, Foxit). The new WebView2 host uses the built-in Chromium PDF viewer, so no extra software is required—though those readers remain useful for double-checking generated files. You can also use https://github.com/sillsdev/SmallPdfDropletTest to exercise the engine via command line.

## Disable Analytics

We don't want developer and tester runs (and crashes) polluting our statistics. Add the environment variable `feedback=off`.

## Continuous Build System

Each time code is checked in, an automatic build begins on our [TeamCity build server](http://build.palaso.org/project.html?projectId=PdfDroplet). Similarly, when there is a new version of a PdfDroplet dependency (e.g., SIL.Core), that server automatically rebuilds PdfDroplet.
