PdfDroplet is a simple little windows app which only does one thing: it takes your PDF and gives you a new one, with the pages combined and reordered, ready for saving and printing as booklets.

# Application Requirements

- Windows
- Microsoft Edge WebView2 Runtime (normally installed automatically with Edge; installers should ship it as well).

# Developing

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) (Visual Studio 2022 17.8+ works out of the box).
- [Node.js 18+](https://nodejs.org/) for the React/Vite frontend build.

## Building

Building requires restoring nuget and npm dependencies running the Vite.

#### Visual Studio

Right-click on the solution and "Rebuild Solution".

#### Command line

Run `dotnet build PdfDroplet.sln` (or build inside Visual Studio).

## Frontend Development

1. From the `ui/` directory run `npm install` (first time) and then `npm run dev`, which will hot-update as needed.
2. Launch PdfDroplet from Visual Studio or `dotnet run`.

### Frontend Testing

All frontend npm scripts live in the `ui/` workspace. Run them from that directory (or pass `--prefix ui`) so `npm` can find the correct `package.json`. For example:

- `npm run test:e2e` executes the smoke test, which runs the WinForms host.

### Debugging the WebView and Bridge

- Set the environment variable `PDFDROPLET_AUTOMATION_PORT=9222` (or another free port) before launching PdfDroplet to enable WebView2 remote debugging. You can then attach Playwright or open `edge://inspect` to poke around the live UI.

## Disable Analytics

We don't want developer and tester runs (and crashes) polluting our statistics. Add the environment variable `feedback=off`.
