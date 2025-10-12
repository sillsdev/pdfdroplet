# Agent Instructions for PdfDroplet Development

## Project Overview

PdfDroplet is a hybrid .NET WinForms + React application. The .NET app hosts a WebView2 control that loads the React UI. This creates specific workflows for development and testing.

## Architecture

- **Frontend**: React + TypeScript + Vite (in `browser/`)
- **Backend**: .NET 8 WinForms with WebView2 (in `dotnet/`)
- **Tests**: Playwright E2E tests (in `browser/tests/automation/`)

The .NET app can load the React UI in two ways:
1. **Dev Server Mode**: Connects to Vite dev server at `http://localhost:5173` (hot reload)
2. **Built Files Mode**: Serves static files from `dotnet/bin/Debug/net8.0/ui-dist/`

## Critical: How Playwright Tests See Code Changes

**⚠️ IMPORTANT**: Playwright tests launch the .NET app via `dotnet run`, which then loads the React UI. For tests to pick up your latest React code changes, you MUST do Build Before Testing

```bash
# From the browser/ directory
npm run build

# Then run tests
npm run test:e2e
```

## How the Build System Works

### Detection Logic (browser.proj)

The MSBuild project `browser/browser.proj` has smart detection:

1. **Checks for `PDFDROPLET_UI_DEV_SERVER` env var**: If set, uses that URL
2. **In Debug mode**: Checks if `http://localhost:5173` is responding
3. **If dev server found**: Skips build, prints "✓ Dev server detected - Hot reload enabled!"
4. **If no dev server**: Runs `npm run build` to create static files

### File Copying (DotNet.csproj)

After the frontend builds (or if using dev server), `dotnet/DotNet.csproj`:

1. **CopyFrontendToOutput target**: Copies `browser/dist/` → `dotnet/bin/Debug/net8.0/ui-dist/`
2. Runs after every .NET build
3. Only copies if `browser/dist/` exists

### Runtime Loading (WorkspaceControl.cs)

When the app starts:

1. **First tries dev server**: Checks `PDFDROPLET_UI_DEV_SERVER` or `http://localhost:5173` (Debug only)
2. **Falls back to static files**: Looks for `ui-dist/index.html` in bin directory
3. If neither exists, shows error

## Testing Commands

```bash
# Run all Playwright tests
npm run test:e2e

# Run specific test file
npx playwright test tests/automation/app-smoke.spec.ts

# Run with UI mode (helpful for debugging)
npx playwright test --ui

# Run with headed browser
npx playwright test --headed
```

## Debugging Tips

### Tests aren't picking up my changes
- **Verify build ran**: Check that `browser/dist/index.html` exists and has recent timestamp
- **Clear old builds**: `npm run build` from browser/ directory
- **Check file timestamps**: Ensure `dist/` files are newer than your source changes

### Dev server not being detected
- **Verify it's running**: Visit `http://localhost:5173` in a browser
- **Check the port**: Default is 5173, but can be configured
- **Set env var explicitly**: `$env:PDFDROPLET_UI_DEV_SERVER="http://localhost:5173"`

### App won't start
- **Missing ui-dist**: Run `npm run build` in browser/
- **Check console output**: The app prints which mode it's using
- **Verify .NET build**: Ensure `dotnet run` works from dotnet/ directory

## Environment Variables

- `PDFDROPLET_UI_DEV_SERVER`: Override dev server URL (optional)
- `PDFDROPLET_AUTOMATION_PORT`: Used by Playwright for CDP connection (set automatically)

