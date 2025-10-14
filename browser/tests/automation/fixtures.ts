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
    { scope: "worker", auto: true },
  ],
});

// Export a setup function that test files can call in their beforeEach hooks
export async function resetAppState(app: AppFixture["app"]) {
  // Reset the WebView between tests to avoid state leakage across cases.
  await app.page.reload({ waitUntil: "domcontentloaded" });
  
  // Wait for React to initialize by waiting for a known element
  await app.page.getByTestId("about-button").waitFor({ state: "visible", timeout: 10000 });
  
  // Aggressively close any open modals - try multiple approaches
  const modalBackdrop = app.page.getByTestId("modal-backdrop");
  
  // Wait a bit for any animations/mounting to complete
  await app.page.waitForTimeout(100);
  
  // Check and close modals multiple times to handle race conditions
  for (let attempt = 0; attempt < 3; attempt++) {
    const isModalVisible = await modalBackdrop.isVisible().catch(() => false);
    if (!isModalVisible) break;
    
    // Try Escape key first (more reliable)
    await app.page.keyboard.press("Escape");
    await app.page.waitForTimeout(200);
    
    // If still visible, try clicking close button
    const isStillVisible = await modalBackdrop.isVisible().catch(() => false);
    if (isStillVisible) {
      // Try to find and click any close buttons
      const closeButtons = app.page.locator('[data-testid*="close-button"], button:has-text("Close")');
      const closeButtonCount = await closeButtons.count();
      if (closeButtonCount > 0) {
        await closeButtons.first().click({ force: true });
      }
      await app.page.waitForTimeout(200);
    }
  }
  
  // Final verification that no modal is visible
  await modalBackdrop.waitFor({ state: "hidden", timeout: 1000 }).catch(() => {
    // If modal is still visible, log it but continue
    console.warn("Warning: Modal backdrop still visible after cleanup attempts");
  });
}

export { test, expect };
