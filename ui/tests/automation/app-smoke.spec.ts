import {
  chromium,
  expect,
  test,
  type Browser,
  type BrowserContext,
  type Page,
} from "@playwright/test";
import { spawn } from "node:child_process";
import { createServer } from "node:net";
import { setTimeout as delay } from "node:timers/promises";
import { request } from "node:http";
import path from "node:path";
import { fileURLToPath } from "node:url";

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

async function waitForWebView2(
  port: number,
  timeoutMs = 30_000
): Promise<void> {
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

  throw new Error(
    `Timed out waiting for WebView2 CDP endpoint on port ${port}`
  );
}

async function stopProcess(child: ReturnType<typeof spawn>): Promise<void> {
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
    for (const context of contexts) {
      if (context.pages().length > 0) {
        return context;
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

test("PdfDroplet boots and exposes the WebView bridge", async () => {
  test.setTimeout(90_000);

  const automationPort = await getAvailablePort();
  const currentDir = path.dirname(fileURLToPath(import.meta.url));
  const projectPath = path.resolve(
    currentDir,
    "../../../src/PdfDroplet.csproj"
  );

  const appProcess = spawn(
    "dotnet",
    ["run", "--project", projectPath, "--configuration", "Debug"],
    {
      env: {
        ...process.env,
        PDFDROPLET_AUTOMATION_PORT: String(automationPort),
      },
      stdio: "inherit",
    }
  );

  await waitForWebView2(automationPort);

  const browser = await chromium.connectOverCDP(
    `http://127.0.0.1:${automationPort}`
  );

  try {
    const context = await acquireAppContext(browser);
    const page = await acquireAppPage(context);

    await expect(page.getByText("Printer Paper Size")).toBeVisible({
      timeout: 45_000,
    });
    await expect(page.getByText("Drag a PDF document here")).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Choose a PDF to open" }).first()
    ).toBeVisible();

    const bridgeProbe = await page.evaluate(async () => {
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
        return { hasBridge: false };
      }

      const invoke = <TResult>(
        method: string,
        parameters?: unknown,
        timeoutMs = 15_000
      ) =>
        new Promise<TResult>((resolve, reject) => {
          const id = `playwright-${method}-${Date.now()}-${Math.random()
            .toString(16)
            .slice(2)}`;

          const timer = setTimeout(() => {
            cleanup();
            reject(new Error(`Timed out waiting for response to ${method}`));
          }, timeoutMs);

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
                reject(
                  new Error(payload.error?.message ?? "Bridge call failed")
                );
              } else {
                resolve(payload.result as TResult);
              }
            } catch {
              // Ignore JSON parse errors and keep waiting.
            }
          };

          bridge.addEventListener("message", listener as EventListener);
          bridge.postMessage({
            type: "request",
            id,
            method,
            params: parameters,
          });
        });

      const [state, layouts, paperTargets] = await Promise.all([
        invoke<Record<string, unknown>>("requestState"),
        invoke<Array<Record<string, unknown>>>("requestLayouts"),
        invoke<Array<Record<string, unknown>>>("requestPaperTargets"),
      ]);

      return {
        hasBridge: true,
        state,
        layoutCount: Array.isArray(layouts) ? layouts.length : 0,
        paperTargetCount: Array.isArray(paperTargets) ? paperTargets.length : 0,
      };
    });

    expect(bridgeProbe.hasBridge).toBe(true);
    expect(typeof bridgeProbe.state).toBe("object");
    expect(bridgeProbe.layoutCount).toBeGreaterThan(0);
    expect(bridgeProbe.paperTargetCount).toBeGreaterThan(0);
  } finally {
    await browser.close();
    await stopProcess(appProcess);
  }
});
