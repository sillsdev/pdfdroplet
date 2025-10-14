# PdfDroplet Versioning System

## Overview

PdfDroplet uses a comprehensive versioning system that ensures version numbers are consistently applied across:
- The .NET executable (visible in file properties)
- The About dialog in the UI
- The installer filename

## Version Format

- **Base Version**: Defined in `Directory.Build.props` (e.g., `2.6`)
- **Full Version**: `{Base}.{BuildNumber}` (e.g., `2.6.123`)
  - Build number comes from `github.run_number` in CI builds
  - For local dev builds, defaults to `0` (e.g., `2.6.0`)

## How It Works

### 1. Build Time (GitHub Actions)

The `.github/workflows/manual-release.yml` workflow:

1. Reads base version from `Directory.Build.props`
2. Appends `github.run_number` to create full version (e.g., `2.6.123`)
3. Creates `browser/assets/version.json` with version metadata:
   ```json
   {
     "version": "2.6.123",
     "buildNumber": "123",
     "buildDate": "2025-01-15T10:30:00Z"
   }
   ```
4. Passes full version to MSBuild via `/p:Version=2.6.123`
5. Builds installer with version in filename: `PdfDropletInstaller-2.6.123.exe`

### 2. Assembly Versioning (.NET)

The `dotnet/DotNet.csproj` project:

- Sets `AssemblyVersion`, `FileVersion`, and `InformationalVersion` to `$(Version)`
- This makes the version visible in Windows file properties (right-click → Properties → Details)

### 3. Runtime Version Display

The version flows to the UI through:

1. **Backend**: `WorkspaceUiBridge.GetApplicationVersion()` reads version from assembly
2. **Bridge**: Exposes version via `RuntimeInfo` record
3. **Frontend**: `AboutDialog` fetches version via `bridge.getRuntimeInfo()` and displays it

## Local Development

For local development, a template `version.json` exists with:
```json
{
  "version": "2.6.0",
  "buildNumber": "0",
  "buildDate": "2025-01-01T00:00:00Z"
}
```

The version will display as the value from `Directory.Build.props` with `.0` appended.

## Updating the Version

To bump the version:

1. Edit `Directory.Build.props`
2. Update the `<Version>` property (e.g., from `2.6` to `2.7`)
3. Commit and push
4. The next CI build will automatically append the build number

## Version Visibility

Users can see the version in:

1. **About Dialog**: Click Help → About to see "Version 2.6.123"
2. **EXE Properties**: Right-click `PdfDroplet.exe` → Properties → Details tab
3. **Installer Filename**: `PdfDropletInstaller-2.6.123.exe`
4. **GitHub Release**: Tagged as `v2.6.123`
