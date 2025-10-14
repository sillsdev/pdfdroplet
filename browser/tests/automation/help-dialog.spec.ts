import { expect, test, resetAppState } from "./fixtures";

test.describe("Help Dialog", () => {
  test.beforeEach(async ({ app }) => {
    await resetAppState(app);
  });
  test("shows Help button in footer", async ({ app }) => {
    const { page } = app;

    // Verify the Help button is visible in the footer
    const helpButton = page.getByTestId("help-button");
    await expect(helpButton).toBeVisible();
    await expect(helpButton).toHaveText("Help");
  });

  test("opens the help dialog when Help button is clicked", async ({ app }) => {
    const { page } = app;

    // Verify modal is not initially visible
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).not.toBeVisible();

    // Click the Help button
    const helpButton = page.getByTestId("help-button");
    await expect(helpButton).toBeVisible();
    await helpButton.click();

    // Verify the modal opens
    await expect(modalBackdrop).toBeVisible();
  });

  test("displays help content with title", async ({ app }) => {
    const { page } = app;

    // Find and click the Help button
    const helpButton = page.getByTestId("help-button");
    await expect(helpButton).toBeVisible();
    await helpButton.click();

    // Verify the modal is open
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).toBeVisible();

    // Verify the title is present (use getByRole to be more specific)
    await expect(page.getByRole("heading", { name: "Help" })).toBeVisible();

    // Verify help content is displayed
    const helpContent = page.getByTestId("help-content");
    await expect(helpContent).toBeVisible();

    // Wait for content to load and check for specific help text
    await expect(
      page.getByText(/While PdfDroplet can work with pages/)
    ).toBeVisible({ timeout: 5000 });
  });

  test("displays help steps in the content", async ({ app }) => {
    const { page } = app;

    // Open the help dialog
    const helpButton = page.getByTestId("help-button");
    await helpButton.click();

    // Wait for content to load
    const helpContent = page.getByTestId("help-content");
    await expect(helpContent).toBeVisible();

    // Verify the steps are present
    await expect(page.getByText(/Step 1/)).toBeVisible({ timeout: 5000 });
    await expect(page.getByText(/Step 2/)).toBeVisible();
    await expect(page.getByText(/Step 3/)).toBeVisible();
    await expect(page.getByText(/Step 4/)).toBeVisible();
  });

  test("displays help images", async ({ app }) => {
    const { page } = app;

    // Open the help dialog
    const helpButton = page.getByTestId("help-button");
    await helpButton.click();

    // Wait for content to load
    const helpContent = page.getByTestId("help-content");
    await expect(helpContent).toBeVisible();

    // Wait for images to be present in the DOM
    await page.waitForSelector('img[src*="help"]', {
      timeout: 5000,
    });

    // Find all help step images
    const images = page.locator('img[src*="help"]');
    const imageCount = await images.count();

    // Should have 4 step images
    expect(imageCount).toBeGreaterThanOrEqual(4);

    // Verify at least the first image is visible
    await expect(images.first()).toBeVisible();
  });

  test("closes when the X button is clicked", async ({ app }) => {
    const { page } = app;

    // Open the dialog
    const helpButton = page.getByTestId("help-button");
    await helpButton.click();

    // Verify modal is open
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).toBeVisible();

    // Click the X button in the header
    const xButton = page.getByRole("button", { name: "Close dialog" });
    await expect(xButton).toBeVisible();
    await xButton.click();

    // Verify modal is closed
    await expect(modalBackdrop).not.toBeVisible();
  });

  test("closes when clicking the backdrop", async ({ app }) => {
    const { page } = app;

    // Open the dialog
    const helpButton = page.getByTestId("help-button");
    await helpButton.click();

    // Verify modal is open
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).toBeVisible();

    // Click the backdrop (outside the modal content)
    await modalBackdrop.click({ position: { x: 10, y: 10 } });

    // Verify modal is closed
    await expect(modalBackdrop).not.toBeVisible();
  });

  test("closes when pressing Escape key", async ({ app }) => {
    const { page } = app;

    // Open the dialog
    const helpButton = page.getByTestId("help-button");
    await helpButton.click();

    // Verify modal is open
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).toBeVisible();

    // Press Escape key
    await page.keyboard.press("Escape");

    // Verify modal is closed
    await expect(modalBackdrop).not.toBeVisible();
  });

  test("loads help content without errors", async ({ app }) => {
    const { page } = app;

    // Open the help dialog
    const helpButton = page.getByTestId("help-button");
    await helpButton.click();

    // Wait for content to load
    const helpContent = page.getByTestId("help-content");
    await expect(helpContent).toBeVisible();

    // Verify no error message is displayed
    const errorMessage = page.getByText(/Failed to load help/);
    await expect(errorMessage).not.toBeVisible();

    // Verify loading indicator is not shown
    const loadingMessage = page.getByText(/Loading help content/);
    await expect(loadingMessage).not.toBeVisible();
  });
});
