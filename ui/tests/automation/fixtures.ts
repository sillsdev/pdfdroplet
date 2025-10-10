import { test as base, expect, type Page } from "@playwright/test";
import { launchPdfDroplet, type BridgeInvoker } from "./support/app-controller";

type AppFixture = {
  app: {
    page: Page;
    invoke: BridgeInvoker;
  };
};

const test = base.extend<AppFixture>({
  app: async ({ page: _page }, applyFixture) => {
    void _page;
    const controller = await launchPdfDroplet();

    try {
      await applyFixture({ page: controller.page, invoke: controller.invoke });
    } finally {
      await controller.stop();
    }
  },
});

export { test, expect };
