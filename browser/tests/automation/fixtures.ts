import { test as base, expect, type Page } from "@playwright/test";
import { launchPdfDroplet, type BridgeInvoker } from "./support/app-controller";

type AppFixture = {
  app: {
    page: Page;
    invoke: BridgeInvoker;
  };
};

const test = base.extend<AppFixture>({
  app: [
    async ({}, use) => {
      // Launch the host once per worker so the .NET build only happens once per suite.
      const controller = await launchPdfDroplet();

      try {
        await use({ page: controller.page, invoke: controller.invoke });
      } finally {
        await controller.stop();
      }
    },
    { scope: "worker" },
  ],
});

test.beforeEach(async ({ app }) => {
  // Reset the WebView between tests to avoid state leakage across cases.
  await app.page.reload({ waitUntil: "domcontentloaded" });
});

export { test, expect };
