import { expect, test, resetAppState } from "./fixtures";
import type { LayoutMethodSummary } from "../../src/lib/contracts";
import { createSamplePdf } from "./support/pdf-utils";

test.describe("Layout Images", () => {
  test.beforeEach(async ({ app }) => {
    await resetAppState(app);
  });
  test("should load all layout thumbnail images without broken links", async ({
    app,
  }) => {
    const { page, invoke } = app;
    const pdf = await createSamplePdf("layout-images");

    try {
      // Load a PDF so all layouts are enabled/visible
      await invoke("dropPdf", { path: pdf.path });

      // Wait for the preview to be ready
      const iframe = page.locator('iframe[title="Booklet preview"]');
      await expect(iframe).toBeVisible({ timeout: 45_000 });

      // Get all layout choices
      const layouts = await invoke<LayoutMethodSummary[]>("requestLayouts");
      expect(layouts.length).toBeGreaterThan(0);

      // Check each layout that has a thumbnail image
      const layoutsWithThumbnails = layouts.filter(
        (layout) => layout.thumbnailImage && layout.thumbnailImage.length > 0
      );

      console.log(
        `Checking ${layoutsWithThumbnails.length} layouts with thumbnails...`
      );

      for (const layout of layoutsWithThumbnails) {
        console.log(
          `Checking layout "${layout.displayName}" with thumbnail: ${layout.thumbnailImage}`
        );

        // Find the layout button
        const layoutButton = page.getByRole("button", {
          name: new RegExp(layout.displayName, "i"),
        });

        await expect(layoutButton).toBeVisible();

        // Find the image within this layout button
        const layoutImage = layoutButton.locator(
          `img[src="/images/${layout.thumbnailImage}"]`
        );

        // Check that the image element exists
        await expect(layoutImage).toBeVisible({
          timeout: 5_000,
        });

        // Verify the image loaded successfully by checking its natural dimensions
        const imageLoaded = await layoutImage.evaluate((img: HTMLImageElement) => {
          return img.complete && img.naturalWidth > 0 && img.naturalHeight > 0;
        });

        expect(imageLoaded).toBe(true);

        console.log(
          `✓ Layout "${layout.displayName}" image loaded successfully (${layout.thumbnailImage})`
        );
      }

      console.log(
        `All ${layoutsWithThumbnails.length} layout images loaded successfully!`
      );
    } finally {
      await pdf.dispose();
    }
  });

  test("should display placeholder for layouts without thumbnail images", async ({
    app,
  }) => {
    const { page, invoke } = app;
    const pdf = await createSamplePdf("layout-placeholder");

    try {
      // Load a PDF
      await invoke("dropPdf", { path: pdf.path });

      // Wait for the preview to be ready
      const iframe = page.locator('iframe[title="Booklet preview"]');
      await expect(iframe).toBeVisible({ timeout: 45_000 });

      // Get all layout choices
      const layouts = await invoke<LayoutMethodSummary[]>("requestLayouts");

      // Check layouts without thumbnails (if any)
      const layoutsWithoutThumbnails = layouts.filter(
        (layout) => !layout.thumbnailImage || layout.thumbnailImage.length === 0
      );

      if (layoutsWithoutThumbnails.length > 0) {
        console.log(
          `Found ${layoutsWithoutThumbnails.length} layouts without thumbnails`
        );

        for (const layout of layoutsWithoutThumbnails) {
          const layoutButton = page.getByRole("button", {
            name: new RegExp(layout.displayName, "i"),
          });

          await expect(layoutButton).toBeVisible();

          // Should have a placeholder div instead of an image
          const placeholder = layoutButton.locator("div").filter({
            hasText: "Preview",
          });

          await expect(placeholder).toBeVisible();
          console.log(
            `✓ Layout "${layout.displayName}" has placeholder correctly displayed`
          );
        }
      } else {
        console.log("All layouts have thumbnail images defined");
      }
    } finally {
      await pdf.dispose();
    }
  });
});
