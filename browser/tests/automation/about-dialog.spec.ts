import { expect, test, resetAppState } from "./fixtures";

test.describe("About Dialog", () => {
  test.beforeEach(async ({ app }) => {
    await resetAppState(app);
  });
  test("shows About button in footer", async ({ app }) => {
    const { page } = app;

    // Verify the About button is visible in the footer
    const aboutButton = page.getByTestId("about-button");
    await expect(aboutButton).toBeVisible();
    await expect(aboutButton).toHaveText("About");
  });

  test("opens the about dialog when About button is clicked", async ({
    app,
  }) => {
    const { page } = app;

    // Verify modal is not initially visible
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).not.toBeVisible();

    // Click the About button
    const aboutButton = page.getByTestId("about-button");
    await expect(aboutButton).toBeVisible();
    await aboutButton.click();

    // Verify the modal opens
    await expect(modalBackdrop).toBeVisible();
  });

  test("displays complete about information", async ({ app }) => {
    const { page } = app;

    // Find and click the About button
    const aboutButton = page.getByTestId("about-button");
    await expect(aboutButton).toBeVisible();
    await aboutButton.click();

    // Verify the modal is open
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).toBeVisible();

    // Verify the SIL logo is displayed
    const logo = page.getByTestId("sil-logo");
    await expect(logo).toBeVisible();
    await expect(logo).toHaveAttribute("alt", "SIL International Logo");

    // Verify the title is present
    await expect(page.getByText("About PDF Droplet")).toBeVisible();

    // Verify copyright text is present
    await expect(page.getByText("Copyright © 2012-")).toBeVisible();

    // Verify the link to the website
    const websiteLink = page.getByTestId("pdfdroplet-link");
    await expect(websiteLink).toBeVisible();
    await expect(websiteLink).toHaveAttribute(
      "href",
      "https://software.sil.org/pdfdroplet/",
    );
  });

  test("closes when the close button is clicked", async ({ app }) => {
    const { page } = app;

    // Open the dialog
    const aboutButton = page.getByTestId("about-button");
    await aboutButton.click();

    // Verify modal is open
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).toBeVisible();

    // Click the close button in the modal
    const closeButton = page.getByTestId("about-close-button");
    await expect(closeButton).toBeVisible();
    await closeButton.click();

    // Verify modal is closed
    await expect(modalBackdrop).not.toBeVisible();
  });

  test("closes when clicking the X button", async ({ app }) => {
    const { page } = app;

    // Open the dialog
    const aboutButton = page.getByTestId("about-button");
    await aboutButton.click();

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
    const aboutButton = page.getByTestId("about-button");
    await aboutButton.click();

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
    const aboutButton = page.getByTestId("about-button");
    await aboutButton.click();

    // Verify modal is open
    const modalBackdrop = page.getByTestId("modal-backdrop");
    await expect(modalBackdrop).toBeVisible();

    // Press Escape key
    await page.keyboard.press("Escape");

    // Verify modal is closed
    await expect(modalBackdrop).not.toBeVisible();
  });

  test("logo image loads successfully", async ({ app }) => {
    const { page } = app;

    // Open the dialog
    const aboutButton = page.getByTestId("about-button");
    await aboutButton.click();

    // Wait for the logo to be visible
    const logo = page.getByTestId("sil-logo");
    await expect(logo).toBeVisible();

    // Check that the image has loaded (natural dimensions are set)
    const hasLoaded = await logo.evaluate((img: HTMLImageElement) => {
      return img.complete && img.naturalHeight > 0;
    });

    expect(hasLoaded).toBe(true);
  });
});
