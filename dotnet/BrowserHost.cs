using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using PdfDroplet.Api;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace PdfDroplet
{
    public partial class BrowserHost : UserControl
    {
        private DocumentViewModel _model;
        private readonly API _bridge;
        private const string AutomationDebugPortEnvVar = "PDFDROPLET_AUTOMATION_PORT";
        private const string ReactDevServerEnvVar = "PDFDROPLET_UI_DEV_SERVER";
        private const string ReactVirtualHostName = "app.pdfdroplet";
        internal const string PreviewVirtualHostName = "preview.pdfdroplet";
        private readonly JsonSerializerOptions _bridgeSerializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        private bool _webViewMessagingInitialized;
        private bool _externalDragActive;

        public BrowserHost()
        {
            InitializeComponent();
            _model = new DocumentViewModel();
            _bridge = new API(this, _model, this);

            Padding = new Padding(0);

            // Enable drag and drop on this control
            this.AllowDrop = true;

            InitializeExternalDragDropHandling();

            InitializeWebView2Async();
        }

        internal IApi UiBridge
        {
            get { return _bridge; }
        }

        private async void InitializeWebView2Async()
        {
            try
            {
                // Set user data folder to a writable location in AppData
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PdfDroplet", "WebView2");

#if DEBUG
                // In Debug mode, completely clear the cache to ensure fresh content
                ClearWebView2Cache(userDataFolder);
#endif

                var automationPort = TryGetAutomationDebugPort();

                // Build browser arguments
                var browserArgs = new System.Collections.Generic.List<string>();

                if (automationPort.HasValue)
                {
                    browserArgs.Add($"--remote-debugging-port={automationPort.Value}");
                }


                // disable HTTP cache to ensure fresh content on every load
                browserArgs.Add("--disable-http-cache");
                browserArgs.Add("--disk-cache-size=0");


                CoreWebView2EnvironmentOptions environmentOptions = browserArgs.Count > 0
                    ? new CoreWebView2EnvironmentOptions
                    {
                        AdditionalBrowserArguments = string.Join(" ", browserArgs)
                    }
                    : null;

                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, environmentOptions);
                await _browser.EnsureCoreWebView2Async(environment);

                if (automationPort.HasValue)
                {
                    _browser.CoreWebView2.Settings.AreDevToolsEnabled = true;
                    Console.WriteLine($"PDFDroplet automation: WebView2 remote debugging listening on port {automationPort.Value}");
                }

#if DEBUG
                // Enable context menus in debug builds for development
                _browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
#else
                // Disable context menus in release builds for end users
                _browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
#endif

                ConfigurePreviewHostMapping();

                _browser.CoreWebView2.NewWindowRequested += OnCoreNewWindowRequested;

                // Configure PDF toolbar settings
                _browser.CoreWebView2.Settings.HiddenPdfToolbarItems =
                     CoreWebView2PdfToolbarItems.Rotate
                    | CoreWebView2PdfToolbarItems.FullScreen
                    | CoreWebView2PdfToolbarItems.Save;

                InitializeBridgeMessaging();
                await LoadReactUiAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView2: {ex.Message}\n\nPlease ensure WebView2 Runtime is installed.",
                    "WebView2 Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static int? TryGetAutomationDebugPort()
        {
            var rawValue = Environment.GetEnvironmentVariable(AutomationDebugPortEnvVar);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            if (int.TryParse(rawValue, out var port) && port > 0 && port <= 65535)
            {
                return port;
            }

            Console.WriteLine($"Invalid {AutomationDebugPortEnvVar} value '{rawValue}'. Expected an integer between 1 and 65535.");
            return null;
        }

        private void InitializeBridgeMessaging()
        {
            if (_webViewMessagingInitialized || _browser?.CoreWebView2 == null)
            {
                return;
            }

            _webViewMessagingInitialized = true;

            _browser.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            // Disable WebView2's built-in external drop handling
            // The BrowserHost control will handle all drag-drop events instead
            try
            {
                _browser.AllowExternalDrop = true;
                Console.WriteLine("[drop] WebView2.AllowExternalDrop set to false - BrowserHost will handle drops");
            }
            catch (Exception error)
            {
                Console.WriteLine($"[drop] Unable to set AllowExternalDrop on WebView2: {error.Message}");
            }

            _bridge.WorkspaceStateChanged += OnBridgeWorkspaceStateChanged;
            _bridge.LayoutChoicesChanged += OnBridgeLayoutChoicesChanged;
            _bridge.GenerationStatusChanged += OnBridgeGenerationStatusChanged;
            _bridge.GeneratedPdfReady += OnBridgeGeneratedPdfReady;

            PostEvent("bridgeReady", null);
        }

        private void InitializeExternalDragDropHandling()
        {
            try
            {
                // Attach drag handlers only to this UserControl (not to WebView2)
                AttachDragDropHandlers(this);

                Console.WriteLine("[drop] Drag and drop handlers initialized on BrowserHost control");
            }
            catch (Exception error)
            {
                Console.WriteLine($"[drop] Unable to initialize drag and drop: {error.Message}");
            }
        }

        private void AttachDragDropHandlers(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.AllowDrop = true;

            control.DragEnter -= OnHostDragEnter;
            control.DragOver -= OnHostDragOver;
            control.DragLeave -= OnHostDragLeave;
            control.DragDrop -= OnHostDragDrop;

            control.DragEnter += OnHostDragEnter;
            control.DragOver += OnHostDragOver;
            control.DragLeave += OnHostDragLeave;
            control.DragDrop += OnHostDragDrop;
        }

        private async Task LoadReactUiAsync()
        {
            var coreWebView2 = _browser?.CoreWebView2;
            if (coreWebView2 == null)
            {
                return;
            }

            if (await TryNavigateToDevServerAsync(coreWebView2).ConfigureAwait(true))
            {
                _bridge.SetRuntimeMode(RuntimeMode.DevServer);
                return;
            }

            var distDirectory = TryResolveReactDistDirectory();
            if (!string.IsNullOrEmpty(distDirectory) && Directory.Exists(distDirectory))
            {
                // Disable caching in Debug mode to ensure latest built files are always loaded
#if DEBUG
                coreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                // Note: WebView2 doesn't have a direct API to disable HTTP cache,
                // but we can force cache bypass by adding cache-control headers via navigation
                Console.WriteLine("🔄 Debug mode: WebView2 cache will be bypassed for static files");
#endif
                coreWebView2.SetVirtualHostNameToFolderMapping(
                    ReactVirtualHostName,
                    distDirectory,
                    CoreWebView2HostResourceAccessKind.Allow);

#if DEBUG
                // Force cache refresh by navigating with cache-busting parameter
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                coreWebView2.Navigate($"https://{ReactVirtualHostName}/index.html?_cb={timestamp}");
#else
                coreWebView2.Navigate($"https://{ReactVirtualHostName}/index.html");
#endif
                _bridge.SetRuntimeMode(RuntimeMode.Bundle);
                return;
            }

            var fallbackHtml = "<html><head><style>body{font-family:'Segoe UI',sans-serif;background:#f8fafc;color:#0f172a;margin:0;padding:2rem;}" +
                               ".card{max-width:640px;margin:0 auto;background:#ffffff;border-radius:18px;box-shadow:0 15px 35px rgba(15,23,42,0.08);padding:2.5rem;}" +
                               "h1{font-size:1.8rem;margin-bottom:1rem;}p{margin:0 0 0.75rem;}code{background:#e2e8f0;padding:0.15rem 0.35rem;border-radius:6px;font-size:0.95rem;}</style></head>" +
                               "<body><div class='card'><h1>React UI not available</h1><p>PdfDroplet couldn't find the React workspace bundle.</p>" +
                               "<p>Start the dev server and set the <code>PDFDROPLET_UI_DEV_SERVER</code> environment variable, or build the frontend (npm install & npm run build) so that <code>browser/dist</code> exists in the browser folder.</p>" +
                               "</div></body></html>";

            coreWebView2.NavigateToString(fallbackHtml);
        }

        private void ConfigurePreviewHostMapping()
        {
            var coreWebView2 = _browser?.CoreWebView2;
            if (coreWebView2 == null)
            {
                return;
            }

            try
            {
                var previewDirectory = DocumentViewModel.GetPreviewDirectory();
                coreWebView2.SetVirtualHostNameToFolderMapping(
                    PreviewVirtualHostName,
                    previewDirectory,
                    CoreWebView2HostResourceAccessKind.Allow);
            }
            catch (Exception error)
            {
                Console.WriteLine($"Failed to configure preview host mapping: {error.Message}");
            }
        }

        private async Task<bool> TryNavigateToDevServerAsync(CoreWebView2 coreWebView2)
        {
            var devServerUrl = Environment.GetEnvironmentVariable(ReactDevServerEnvVar);

#if DEBUG
            devServerUrl ??= "http://localhost:5173";
#endif

            if (string.IsNullOrWhiteSpace(devServerUrl))
            {
                return false;
            }

            if (!Uri.TryCreate(devServerUrl, UriKind.Absolute, out var devServerUri))
            {
                return false;
            }

            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(2)
                };

                using var response = await httpClient.GetAsync(devServerUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                Console.WriteLine($"✓ Connected to Vite dev server at {devServerUri} - Hot reload enabled!");
            }
            catch
            {
                return false;
            }

            coreWebView2.Navigate(devServerUri.ToString());
            return true;
        }

        private static string TryResolveReactDistDirectory()
        {
            static bool HasIndexHtml(string directory)
            {
                if (string.IsNullOrWhiteSpace(directory))
                {
                    return false;
                }

                var candidate = Path.Combine(directory, "index.html");
                return File.Exists(candidate);
            }

            var baseDirectory = AppContext.BaseDirectory;

            var commonCandidates = new[]
            {
                Path.Combine(baseDirectory, "ui-dist"),
                Path.Combine(baseDirectory, "browser", "dist"),
                Path.Combine(baseDirectory, "dist"),
            };

            foreach (var candidate in commonCandidates)
            {
                if (HasIndexHtml(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }

            var current = new DirectoryInfo(baseDirectory);
            for (var i = 0; i < 8 && current != null; i++, current = current.Parent)
            {
                var distInRepo = Path.Combine(current.FullName, "browser", "dist");
                if (HasIndexHtml(distInRepo))
                {
                    return Path.GetFullPath(distInRepo);
                }
            }

            return null;
        }

        private async void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string requestId = null;

            try
            {
                Console.WriteLine("[bridge] WebMessageReceived payload: " + e.WebMessageAsJson);
                using (var document = JsonDocument.Parse(e.WebMessageAsJson))
                {
                    var root = document.RootElement;
                    if (!root.TryGetProperty("type", out var typeElement))
                        return;

                    if (!string.Equals(typeElement.GetString(), "request", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (!root.TryGetProperty("id", out var idElement))
                        return;

                    requestId = idElement.GetString();
                    if (string.IsNullOrWhiteSpace(requestId))
                        return;

                    if (!root.TryGetProperty("method", out var methodElement))
                        throw new ArgumentException("Request missing method.");

                    var method = methodElement.GetString();
                    Console.WriteLine($"[bridge] Received request '{method}' (id={requestId})");

                    JsonElement? paramsElement = null;
                    if (root.TryGetProperty("params", out var element))
                    {
                        paramsElement = element;
                    }

                    var result = await ExecuteRequestAsync(method, paramsElement).ConfigureAwait(true);
                    PostResponse(requestId, result, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[bridge] Error processing request {requestId}: {ex.Message}\n{ex}");
                if (!string.IsNullOrWhiteSpace(requestId))
                {
                    PostResponse(requestId, null, MapToRpcError(ex));
                }
            }
        }

        private async Task<object> ExecuteRequestAsync(string method, JsonElement? parameters)
        {
            Console.WriteLine($"[bridge] ExecuteRequestAsync handling '{method}'");
            switch (method)
            {
                case "requestState":
                    return await _bridge.GetWorkspaceStateAsync().ConfigureAwait(true);
                case "requestLayouts":
                    return await _bridge.GetLayoutChoicesAsync().ConfigureAwait(true);
                case "requestPaperTargets":
                    return await _bridge.GetPaperTargetsAsync().ConfigureAwait(true);
                case "getRuntimeInfo":
                    return await _bridge.GetRuntimeInfoAsync().ConfigureAwait(true);
                case "pickPdf":
                    return await _bridge.PickPdfAsync().ConfigureAwait(true);
                case "dropPdf":
                    var dropPath = RequireString(parameters, "path");
                    Console.WriteLine($"[bridge] dropPdf requested for path '{dropPath}' (exists={File.Exists(dropPath)})");
                    var dropResult = await _bridge.DropPdfAsync(dropPath).ConfigureAwait(true);
                    Console.WriteLine(
                        "[bridge] dropPdf completed => HasIncomingPdf={0}, GeneratedPdfPath='{1}'",
                        dropResult.HasIncomingPdf,
                        dropResult.GeneratedPdfPath ?? string.Empty);
                    return dropResult;
                case "reloadPrevious":
                    return await _bridge.ReloadPreviousAsync().ConfigureAwait(true);
                case "setLayout":
                    return await _bridge.SetLayoutAsync(RequireString(parameters, "layoutId")).ConfigureAwait(true);
                case "setPaper":
                    return await _bridge.SetPaperTargetAsync(RequireString(parameters, "paperId")).ConfigureAwait(true);
                case "setMirror":
                    return await _bridge.SetMirrorAsync(RequireBoolean(parameters, "enabled")).ConfigureAwait(true);
                case "setRightToLeft":
                    return await _bridge.SetRightToLeftAsync(RequireBoolean(parameters, "enabled")).ConfigureAwait(true);
                case "setCropMarks":
                    return await _bridge.SetCropMarksAsync(RequireBoolean(parameters, "enabled")).ConfigureAwait(true);
                case "saveBooklet":
                    return await _bridge.SaveBookletAsync().ConfigureAwait(true);
                default:
                    throw new NotSupportedException($"Unknown request method '{method}'.");
            }
        }

        private static string RequireString(JsonElement? parameters, string propertyName)
        {
            if (parameters.HasValue && parameters.Value.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }

            throw new ArgumentException($"Request parameter '{propertyName}' must be provided as a string.");
        }

        private static bool RequireBoolean(JsonElement? parameters, string propertyName)
        {
            if (parameters.HasValue && parameters.Value.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                return value.GetBoolean();
            }

            throw new ArgumentException($"Request parameter '{propertyName}' must be provided as a boolean.");
        }

        private void PostResponse(string requestId, object result, RpcError error)
        {
            if (_browser?.CoreWebView2 == null || string.IsNullOrWhiteSpace(requestId))
                return;

            var envelope = new
            {
                type = "response",
                id = requestId,
                result = error == null ? result : null,
                error
            };

            var json = JsonSerializer.Serialize(envelope, _bridgeSerializationOptions);
            _browser.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void PostEvent(string eventName, object payload)
        {
            if (_browser?.CoreWebView2 == null)
                return;

            var envelope = new
            {
                type = "event",
                @event = eventName,
                payload
            };

            var json = JsonSerializer.Serialize(envelope, _bridgeSerializationOptions);
            _browser.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void OnHostDragEnter(object sender, DragEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            var effect = GetPreferredDragEffect(e);
            e.Effect = effect;
            UpdateExternalDragState(effect != DragDropEffects.None);

            try
            {
                var formats = e.Data?.GetFormats() ?? Array.Empty<string>();
                Console.WriteLine($"[drop][host] DragEnter allowed={e.AllowedEffect} -> effect={effect}; formats={string.Join(", ", formats)}");
            }
            catch (Exception error)
            {
                Console.WriteLine($"[drop][host] DragEnter diagnostics failed: {error.Message}");
            }
        }

        private void OnHostDragOver(object sender, DragEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            var effect = GetPreferredDragEffect(e);
            e.Effect = effect;
            UpdateExternalDragState(effect != DragDropEffects.None);
        }

        private void OnHostDragLeave(object sender, EventArgs e)
        {
            UpdateExternalDragState(false);
        }

        private void OnHostDragDrop(object sender, DragEventArgs e)
        {
            UpdateExternalDragState(false);

            var dataObject = e?.Data;
            var formats = dataObject?.GetFormats() ?? Array.Empty<string>();

            Console.WriteLine($"[drop][host] DragDrop received; effect={e?.Effect}; formats={string.Join(", ", formats)}");

            if (!TryExtractDroppedFilePath(dataObject, out var path))
            {
                PostExternalDrop(null, formats);
                return;
            }

            PostExternalDrop(path, formats);
        }

        private void UpdateExternalDragState(bool isActive)
        {
            if (_externalDragActive == isActive)
            {
                return;
            }

            _externalDragActive = isActive;
            PostEvent("externalDragState", new { isActive });
        }

        private void PostExternalDrop(string path, string[] formats)
        {
            PostEvent("externalDrop", new { path, formats = formats ?? Array.Empty<string>() });
        }

        private void OnCoreNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            var handled = false;

            try
            {
                var uri = e.Uri;
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    if (TryNormalizeFromUri(uri, out var path) && !string.IsNullOrWhiteSpace(path))
                    {
                        Console.WriteLine($"[drop][newWindow] Intercepted CoreWebView2 new-window request for '{uri}' => '{path}'");
                        UpdateExternalDragState(false);
                        PostExternalDrop(path, new[] { "core-new-window", "uri" });
                        handled = true;
                    }
                    else if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed) && parsed.IsFile)
                    {
                        var normalized = NormalizeDroppedPath(parsed.LocalPath);
                        if (!string.IsNullOrWhiteSpace(normalized))
                        {
                            Console.WriteLine($"[drop][newWindow] Intercepted CoreWebView2 new-window request for '{uri}' => '{normalized}'");
                            UpdateExternalDragState(false);
                            PostExternalDrop(normalized, new[] { "core-new-window", parsed.Scheme });
                            handled = true;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[drop][newWindow] Error while processing new window request: {error.Message}");
            }

            if (handled)
            {
                e.Handled = true;
            }
        }

        private static DragDropEffects GetPreferredDragEffect(DragEventArgs e)
        {
            if (e?.Data == null)
            {
                return DragDropEffects.None;
            }

            var dataObject = e.Data;
            try
            {
                if (dataObject.GetDataPresent(DataFormats.FileDrop))
                {
                    return SelectEffect(e.AllowedEffect, DragDropEffects.Copy);
                }

                if (dataObject.GetDataPresent(DataFormats.Text) || dataObject.GetDataPresent(DataFormats.UnicodeText))
                {
                    return SelectEffect(e.AllowedEffect, DragDropEffects.Copy);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[drop] Error while evaluating drag formats: {error.Message}");
            }

            return DragDropEffects.None;
        }

        private static DragDropEffects SelectEffect(DragDropEffects allowed, DragDropEffects preferred)
        {
            if ((allowed & preferred) != 0)
            {
                return preferred;
            }

            if ((allowed & DragDropEffects.Copy) != 0)
            {
                return DragDropEffects.Copy;
            }

            if ((allowed & DragDropEffects.Move) != 0)
            {
                return DragDropEffects.Move;
            }

            if ((allowed & DragDropEffects.Link) != 0)
            {
                return DragDropEffects.Link;
            }

            return DragDropEffects.None;
        }

        private static bool TryExtractDroppedFilePath(IDataObject dataObject, out string normalizedPath)
        {
            normalizedPath = null;
            if (dataObject == null)
            {
                return false;
            }

            try
            {
                if (dataObject.GetDataPresent(DataFormats.FileDrop))
                {
                    if (dataObject.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                    {
                        var candidate = NormalizeDroppedPath(files[0]);
                        if (!string.IsNullOrWhiteSpace(candidate))
                        {
                            normalizedPath = candidate;
                            return true;
                        }
                    }
                }

                if (TryNormalizeFromText(dataObject, DataFormats.Text, out normalizedPath))
                {
                    return true;
                }

                if (TryNormalizeFromText(dataObject, DataFormats.UnicodeText, out normalizedPath))
                {
                    return true;
                }

                if (dataObject.GetDataPresent("text/uri-list"))
                {
                    var uriListRaw = dataObject.GetData("text/uri-list") as string;
                    if (TryNormalizeFromUriList(uriListRaw, out normalizedPath))
                    {
                        return true;
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[drop] Error reading drop payload: {error.Message}");
            }

            normalizedPath = null;
            return false;
        }

        private static bool TryNormalizeFromText(IDataObject dataObject, string format, out string normalized)
        {
            normalized = null;
            if (!dataObject.GetDataPresent(format))
            {
                return false;
            }

            var raw = dataObject.GetData(format) as string;
            return TryNormalizeFromText(raw, out normalized);
        }

        private static bool TryNormalizeFromText(string value, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmed = value.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return false;
            }

            if (TryNormalizeFromUri(trimmed, out normalized))
            {
                return true;
            }

            var candidate = NormalizeDroppedPath(trimmed);
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                normalized = candidate;
                return true;
            }

            return false;
        }

        private static bool TryNormalizeFromUri(string value, out string normalized)
        {
            normalized = null;
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (!uri.IsFile)
            {
                return false;
            }

            normalized = NormalizeDroppedPath(uri.LocalPath);
            return !string.IsNullOrWhiteSpace(normalized);
        }

        private static bool TryNormalizeFromUriList(string value, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var lines = value
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"));

            foreach (var line in lines)
            {
                if (TryNormalizeFromUri(line, out normalized))
                {
                    return true;
                }

                if (TryNormalizeFromText(line, out normalized))
                {
                    return true;
                }
            }

            normalized = null;
            return false;
        }

        private static string NormalizeDroppedPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim().Trim('\"');
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return null;
            }

            try
            {
                return Path.GetFullPath(trimmed);
            }
            catch
            {
                return trimmed;
            }
        }

        private static bool IsPdfPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private RpcError MapToRpcError(Exception exception)
        {
            switch (exception)
            {
                case ArgumentException argumentException:
                    return new RpcError("invalid-argument", argumentException.Message);
                case OperationCanceledException canceledException:
                    return new RpcError("cancelled", canceledException.Message);
                case NotSupportedException notSupportedException:
                    return new RpcError("not-implemented", notSupportedException.Message);
                default:
                    return new RpcError("operation-failed", exception.Message, exception.ToString());
            }
        }

        private void OnBridgeWorkspaceStateChanged(object sender, WorkspaceStateChangedEventArgs e)
        {
            PostEvent("stateChanged", e.State);
        }

        private void OnBridgeLayoutChoicesChanged(object sender, LayoutChoicesChangedEventArgs e)
        {
            PostEvent("layoutsChanged", e.Layouts);
        }

        private void OnBridgeGenerationStatusChanged(object sender, GenerationStatusChangedEventArgs e)
        {
            PostEvent("generationStatus", e.Status);
        }

        private void OnBridgeGeneratedPdfReady(object sender, GeneratedPdfReadyEventArgs e)
        {
            PostEvent("generatedPdfReady", new { path = e.Path });
        }

        private sealed class RpcError
        {
            public RpcError(string code, string message, string details = null)
            {
                Code = code;
                Message = message;
                Details = details;
            }

            public string Code { get; }
            public string Message { get; }
            public string Details { get; }
        }

        private void DisposeWorkspaceResources()
        {
            _model?.DisposeGeneratedPreviews();
        }

        private static void ClearWebView2Cache(string userDataFolder)
        {
            try
            {
                if (Directory.Exists(userDataFolder))
                {
                    Console.WriteLine($"🗑️  Clearing WebView2 cache at {userDataFolder}");

                    // Try to delete the entire cache directory
                    // This will fail if another instance is running, which is fine
                    Directory.Delete(userDataFolder, recursive: true);
                    Console.WriteLine("✅ WebView2 cache cleared successfully");
                }
            }
            catch (IOException ex)
            {
                // Expected if another instance is running
                Console.WriteLine($"⚠️  Could not clear WebView2 cache (may be in use by another instance): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error clearing WebView2 cache: {ex.Message}");
            }
        }



    }
}
