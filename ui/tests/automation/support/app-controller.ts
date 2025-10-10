import {
  chromium,
  type Browser,
  type BrowserContext,
  type Page,
} from "@playwright/test";
import { spawn, type ChildProcess } from "node:child_process";
import { createServer } from "node:net";
import { request } from "node:http";
import { setTimeout as delay } from "node:timers/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

export type BridgeInvoker = <TResult>(
  method: string,
  parameters?: unknown,
  timeoutMs?: number
) => Promise<TResult>;

export type AppController = {
  page: Page;
  invoke: BridgeInvoker;
  stop: () => Promise<void>;
};

const currentDir = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(currentDir, "../../../..");
const projectPath = path.resolve(repoRoot, "src/PdfDroplet.csproj");

export async function launchPdfDroplet(): Promise<AppController> {
  const automationPort = await getAvailablePort();

  const appProcess = spawn(
    "dotnet",
    ["run", "--project", projectPath, "--configuration", "Debug"],
    {
      env: {
        ...process.env,
        PDFDROPLET_AUTOMATION_PORT: String(automationPort),
      },
      cwd: repoRoot,
      stdio: "inherit",
    }
  );

  let browser: Browser | null = null;
  let context: BrowserContext | null = null;
  let page: Page | null = null;

  try {
    await waitForWebView2(automationPort);

    browser = await chromium.connectOverCDP(
      `http://127.0.0.1:${automationPort}`
    );
    context = await acquireAppContext(browser);
    page = await acquireAppPage(context);

    await page.waitForLoadState("domcontentloaded");
    await ensureBridgeReady(page);

    const invoke = createBridgeInvoker(page);

    return {
      page,
      invoke,
      stop: async () => {
        await browser?.close();
        await stopProcess(appProcess);
      },
    };
  } catch (error) {
    await browser?.close().catch(() => undefined);
    await stopProcess(appProcess).catch(() => undefined);
    throw error;
  }
}

async function getAvailablePort(): Promise<number> {
  return await new Promise<number>((resolve, reject) => {
    const server = createServer();
    server.on("error", reject);
    server.listen(0, () => {
      const address = server.address();
      server.close((closeError) => {
        if (closeError) {
          reject(closeError);
          return;
        }

        if (!address || typeof address === "string") {
          reject(new Error("Failed to obtain automation port"));
          return;
        }

        resolve(address.port);
      });
    });
  });
}

async function waitForWebView2(port: number, timeoutMs = 30_000): Promise<void> {
  const deadline = Date.now() + timeoutMs;

  while (Date.now() < deadline) {
    try {
      await new Promise<void>((resolve, reject) => {
        const req = request(
          {
            hostname: "127.0.0.1",
            port,
            path: "/json/version",
            method: "GET",
            timeout: 1_000,
          },
          (res) => {
            res.resume();
            if (
              (res.statusCode ?? 500) >= 200 &&
              (res.statusCode ?? 500) < 300
            ) {
              resolve();
            } else {
              reject(new Error(`Unexpected status code ${res.statusCode}`));
            }
          }
        );

        req.on("error", reject);
        req.end();
      });

      return;
    } catch {
      await delay(500);
    }
  }

  throw new Error(`Timed out waiting for WebView2 CDP endpoint on port ${port}`);
}

async function stopProcess(child: ChildProcess): Promise<void> {
  if (child.exitCode !== null) {
    return;
  }

  await new Promise<void>((resolve) => {
    child.once("exit", () => resolve());

    const killTimer = setTimeout(() => {
      if (child.exitCode !== null) {
        resolve();
        return;
      }

      if (process.platform === "win32" && child.pid) {
        spawn("taskkill", ["/f", "/t", "/pid", child.pid.toString()]).once(
          "exit",
          () => resolve()
        );
      } else {
        child.kill("SIGKILL");
      }
    }, 2_000);

    if (!child.kill()) {
      clearTimeout(killTimer);
      resolve();
    }
  });
}

async function acquireAppContext(
  browser: Browser,
  timeoutMs = 30_000
): Promise<BrowserContext> {
  const deadline = Date.now() + timeoutMs;

  while (Date.now() < deadline) {
    const contexts = browser.contexts();
    for (const candidate of contexts) {
      if (candidate.pages().length > 0) {
        return candidate;
      }
    }

    await delay(250);
  }

  throw new Error("Timed out waiting for WebView context to become available.");
}

async function acquireAppPage(
  context: BrowserContext,
  timeoutMs = 30_000
): Promise<Page> {
  const deadline = Date.now() + timeoutMs;

  while (Date.now() < deadline) {
    const [candidate] = context.pages();

    if (candidate) {
      await candidate.waitForLoadState("domcontentloaded");
      return candidate;
    }

    await delay(250);
  }

  throw new Error("Timed out waiting for WebView page to become available.");
}

async function ensureBridgeReady(page: Page): Promise<void> {
  await page.waitForFunction(
    () =>
      Boolean(
        (window as unknown as { chrome?: { webview?: { postMessage?: unknown } } }).chrome
          ?.webview?.postMessage
      ),
    undefined,
    {
      timeout: 15_000,
    }
  );
}

function createBridgeInvoker(page: Page): BridgeInvoker {
  return async <TResult>(method: string, parameters?: unknown, timeoutMs = 15_000) => {
    const result = await page.evaluate(
      async (opts) => {
        const candidate = window as typeof window & {
          chrome?: {
            webview?: {
              addEventListener: (type: string, listener: EventListener) => void;
              removeEventListener: (
                type: string,
                listener: EventListener
              ) => void;
              postMessage: (message: unknown) => void;
            };
          };
        };

        const bridge = candidate.chrome?.webview;
        if (!bridge || typeof bridge.postMessage !== "function") {
          throw new Error("Bridge is not available");
        }

        const invoke = (
          envelope: { method: string; params?: unknown },
          timeout: number
        ) =>
          new Promise<unknown>((resolve, reject) => {
            const id = `playwright-${envelope.method}-${Date.now()}-${Math.random()
              .toString(16)
              .slice(2)}`;

            const timer = setTimeout(() => {
              cleanup();
              reject(
                new Error(`Timed out waiting for response to ${envelope.method}`)
              );
            }, timeout);

            const cleanup = () => {
              clearTimeout(timer);
              bridge.removeEventListener("message", listener as EventListener);
            };

            const listener = (event: MessageEvent<string>) => {
              try {
                const payload =
                  typeof event.data === "string"
                    ? JSON.parse(event.data)
                    : event.data;

                if (!payload || typeof payload !== "object") {
                  return;
                }

                if (payload.type !== "response" || payload.id !== id) {
                  return;
                }

                cleanup();
                if (payload.error) {
                  reject(new Error(payload.error?.message ?? "Bridge call failed"));
                } else {
                  resolve(payload.result);
                }
              } catch {
                // Ignore JSON parse errors and keep waiting.
              }
            };

            bridge.addEventListener("message", listener as EventListener);
            bridge.postMessage({
              type: "request",
              id,
              method: envelope.method,
              params: envelope.params,
            });
          });

        return await invoke(
          { method: opts.method, params: opts.parameters },
          opts.timeoutMs
        );
      },
      { method, parameters, timeoutMs }
    );
    return result as TResult;
  };
}
