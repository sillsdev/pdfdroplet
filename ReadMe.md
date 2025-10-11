PdfDroplet is a simple little windows app which only does one thing: it takes your PDF and gives you a new one, with the pages combined and reordered, ready for saving and printing as booklets.

# Application Requirements

- Windows
- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- Microsoft Edge WebView2 Runtime (normally installed automatically with Edge; installers should ship it as well).

# Development

## Prerequisites


- [Node.js 18+](https://nodejs.org/) for the React/Vite frontend build

## Quick Start

### Basic Workflow (Backend Only)

If you're only working on the .NET backend code:

1. Open `PdfDroplet.sln` in Visual Studio
2. Press F5 to build and run

The solution will automatically:
- Install npm dependencies (first time)
- Build the frontend with Vite
- Launch the application

### Frontend Development Workflow

If you're working on the frontend (React/TypeScript):

1. **Start the Vite dev server** (one-time per session):
   ```bash
   cd browser
   npm install    # First time only
   npm run dev
   ```
   This starts a development server at `http://localhost:5173` with hot module replacement (HMR).

2. **Run the application** from Visual Studio (F5)

3. **Make changes** to your React/TypeScript code - they will appear instantly without rebuilding!

The application automatically detects when the Vite dev server is running and uses it instead of the built files. You'll see this message in the console:
```
✓ Connected to Vite dev server at http://localhost:5173 - Hot reload enabled!
```

When the dev server is running, the `browser` project skips the `npm run build` step, making your builds much faster.

## Project Structure

The solution contains two projects:

- **`dotnet`** (`src/DotNet.csproj`) - The main .NET WinForms application
- **`browser`** (`browser/browser.proj`) - The React/Vite frontend build system

The `dotnet` project depends on `browser`, which handles:
- Installing npm dependencies
- Detecting if Vite dev server is running
- Building the frontend with Vite (when dev server is not running)
- Copying built assets to the output directory

## Building from Command Line

```bash
# Build everything
dotnet build PdfDroplet.sln

# Build and run
dotnet run --project src/DotNet.csproj

# Clean all build artifacts (including frontend)
dotnet clean PdfDroplet.sln
```

## Frontend Testing

All frontend npm scripts live in the `browser/` workspace. Run them from that directory:

```bash
cd browser
npm run test:e2e    # End-to-end tests with Playwright
npm run build       # Production build
npm run preview     # Preview production build
```

## Advanced Configuration

### Custom Dev Server URL

To use a different dev server URL (e.g., if port 5173 is in use):

1. Start Vite on a different port:
   ```bash
   npm run dev -- --port 5174
   ```

2. Set the environment variable:
   ```
   PDFDROPLET_UI_DEV_SERVER=http://localhost:5174
   ```

### Debugging the WebView and Bridge

Set the environment variable `PDFDROPLET_AUTOMATION_PORT=9222` (or another free port) before launching PdfDroplet to enable WebView2 remote debugging. You can then:
- Attach Playwright for automated testing
- Navigate to `edge://inspect` in Edge to debug the live UI

### Disable Analytics

We don't want developer and tester runs (and crashes) polluting our statistics. Set the environment variable:
```
feedback=off
```

## Tips

- **Working on backend only?** Just press F5. The frontend will build once and you're good to go.
- **Working on frontend?** Start `npm run dev` once, then press F5. Make frontend changes with instant hot reload.
- **Switching between projects?** No need to restart the Vite dev server - it keeps running in the background.
- **Production build?** Stop the Vite dev server and rebuild, or run `dotnet build` to get production-optimized assets.
