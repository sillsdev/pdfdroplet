using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using PdfDroplet.Interop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfDroplet
{
    public partial class WorkspaceControl : UserControl
    {
        private WorkSpaceViewModel _model;
        private readonly IWorkspaceUiBridge _bridge;
        private const string AutomationDebugPortEnvVar = "PDFDROPLET_AUTOMATION_PORT";
        private const string ReactDevServerEnvVar = "PDFDROPLET_UI_DEV_SERVER";
    private const string ReactVirtualHostName = "app.pdfdroplet";
    internal const string PreviewVirtualHostName = "preview.pdfdroplet";
        private readonly JsonSerializerOptions _bridgeSerializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private bool _webViewMessagingInitialized;

        public WorkspaceControl()
        {
            InitializeComponent();
            _model = new WorkSpaceViewModel(this);
            _bridge = new WorkspaceUiBridge(this, _model, this);

            Padding = new Padding(0);

            InitializeWebView2Async();
        }

        internal IWorkspaceUiBridge UiBridge
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

                var automationPort = TryGetAutomationDebugPort();
                CoreWebView2EnvironmentOptions environmentOptions = null;
                if (automationPort.HasValue)
                {
                    environmentOptions = new CoreWebView2EnvironmentOptions
                    {
                        AdditionalBrowserArguments = $"--remote-debugging-port={automationPort.Value}"
                    };
                }

                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, environmentOptions);
                await _browser.EnsureCoreWebView2Async(environment);

                if (automationPort.HasValue)
                {
                    _browser.CoreWebView2.Settings.AreDevToolsEnabled = true;
                    Console.WriteLine($"PDFDroplet automation: WebView2 remote debugging listening on port {automationPort.Value}");
                }

                ConfigurePreviewHostMapping();

                // Configure PDF toolbar settings
                _browser.CoreWebView2.Settings.HiddenPdfToolbarItems =
                    CoreWebView2PdfToolbarItems.Print
                    | CoreWebView2PdfToolbarItems.Rotate
                    | CoreWebView2PdfToolbarItems.Save
                    | CoreWebView2PdfToolbarItems.SaveAs
                    | CoreWebView2PdfToolbarItems.FullScreen
                    | CoreWebView2PdfToolbarItems.MoreSettings;

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

            _bridge.WorkspaceStateChanged += OnBridgeWorkspaceStateChanged;
            _bridge.LayoutChoicesChanged += OnBridgeLayoutChoicesChanged;
            _bridge.GenerationStatusChanged += OnBridgeGenerationStatusChanged;
            _bridge.GeneratedPdfReady += OnBridgeGeneratedPdfReady;

            PostEvent("bridgeReady", null);
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
                return;
            }

            var distDirectory = TryResolveReactDistDirectory();
            if (!string.IsNullOrEmpty(distDirectory) && Directory.Exists(distDirectory))
            {
                coreWebView2.SetVirtualHostNameToFolderMapping(
                    ReactVirtualHostName,
                    distDirectory,
                    CoreWebView2HostResourceAccessKind.Allow);
                coreWebView2.Navigate($"https://{ReactVirtualHostName}/index.html");
                return;
            }

            var fallbackHtml = "<html><head><style>body{font-family:'Segoe UI',sans-serif;background:#f8fafc;color:#0f172a;margin:0;padding:2rem;}" +
                               ".card{max-width:640px;margin:0 auto;background:#ffffff;border-radius:18px;box-shadow:0 15px 35px rgba(15,23,42,0.08);padding:2.5rem;}" +
                               "h1{font-size:1.8rem;margin-bottom:1rem;}p{margin:0 0 0.75rem;}code{background:#e2e8f0;padding:0.15rem 0.35rem;border-radius:6px;font-size:0.95rem;}</style></head>" +
                               "<body><div class='card'><h1>React UI not available</h1><p>PdfDroplet couldn’t find the React workspace bundle.</p>" +
                               "<p>Start the dev server and set the <code>PDFDROPLET_UI_DEV_SERVER</code> environment variable, or build the frontend (npm install & npm run build) so that <code>ui/dist</code> exists next to the solution.</p>" +
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
                var previewDirectory = WorkSpaceViewModel.GetPreviewDirectory();
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
                Path.Combine(baseDirectory, "ui", "dist"),
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
                var distInRepo = Path.Combine(current.FullName, "ui", "dist");
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
                if (!string.IsNullOrWhiteSpace(requestId))
                {
                    PostResponse(requestId, null, MapToRpcError(ex));
                }
            }
        }

        private async Task<object> ExecuteRequestAsync(string method, JsonElement? parameters)
        {
            switch (method)
            {
                case "requestState":
                    return await _bridge.GetWorkspaceStateAsync().ConfigureAwait(true);
                case "requestLayouts":
                    return await _bridge.GetLayoutChoicesAsync().ConfigureAwait(true);
                case "requestPaperTargets":
                    return await _bridge.GetPaperTargetsAsync().ConfigureAwait(true);
                case "pickPdf":
                    return await _bridge.PickPdfAsync().ConfigureAwait(true);
                case "dropPdf":
                    return await _bridge.DropPdfAsync(RequireString(parameters, "path")).ConfigureAwait(true);
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



    }
}
