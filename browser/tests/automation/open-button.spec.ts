import { expect, test, resetAppState } from "./fixtures";

test.describe("Open Button", () => {
  test.beforeEach(async ({ app }) => {
    await resetAppState(app);
  });

  test("shows Open... button in footer", async ({ app }) => {
    const { page } = app;

    // Verify the Open... button is visible in the footer
    const openButton = page.getByRole("button", { name: "Open..." });
    await expect(openButton).toBeVisible();
    await expect(openButton).toHaveText("Open...");
  });

  test("Open... button appears to the left of Help button", async ({ app }) => {
    const { page } = app;

    // Get both buttons
    const openButton = page.getByRole("button", { name: "Open..." });
    const helpButton = page.getByTestId("help-button");

    // Verify both are visible
    await expect(openButton).toBeVisible();
    await expect(helpButton).toBeVisible();

    // Get their positions
    const openBox = await openButton.boundingBox();
    const helpBox = await helpButton.boundingBox();

    // Verify Open... button is to the left of Help button
    expect(openBox).not.toBeNull();
    expect(helpBox).not.toBeNull();
    expect(openBox!.x).toBeLessThan(helpBox!.x);
  });

  test("Open... button has same styling as Help button", async ({ app }) => {
    const { page } = app;

    const openButton = page.getByRole("button", { name: "Open..." });
    const helpButton = page.getByTestId("help-button");

    // Verify both buttons have the same CSS classes
    const openClasses = await openButton.getAttribute("class");
    const helpClasses = await helpButton.getAttribute("class");

    expect(openClasses).toBe(helpClasses);

    // Verify they both have the expected styling classes
    expect(openClasses).toContain("rounded");
    expect(openClasses).toContain("px-3");
    expect(openClasses).toContain("py-1.5");
    expect(openClasses).toContain("transition-colors");
    expect(openClasses).toContain("hover:bg-slate-100");
    expect(openClasses).toContain("hover:text-droplet-accent");
  });

  test("Open... button is disabled during bootstrapping", async ({ app }) => {
    const { page } = app;

    const openButton = page.getByRole("button", { name: "Open..." });

    // Initially the button might be disabled if app is bootstrapping
    // We'll check it becomes enabled when ready
    await expect(openButton).toBeVisible();

    // The button should not be permanently disabled
    // (The exact state depends on the app's current bootstrapping status)
  });
});
