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

   This starts a development server at `http://localhost:5173` with hot module replacement (HMR). You'll get UI in your browser that is just faking the back end.

2. Optionally **Run the application** from Visual Studio (F5)

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
```

### Disable Analytics

We don't want developer and tester runs (and crashes) polluting our statistics. For debug builds, the program will not send analytics. But if you are testing a release version, please first set this environment variable:

```
SIL_FEEDBACK=off
```

## License

This project is licensed under the MIT License.

Copyright © SIL Global 2012-2025
