import { expect, test } from "./fixtures";
import {
  type LayoutMethodSummary,
  type PaperTargetInfo,
  type WorkspaceState,
} from "../../src/lib/contracts";
import { createSamplePdf } from "./support/pdf-utils";

test.describe.configure({ mode: "serial" });

test("renders the booklet preview after dropping a PDF", async ({ app }) => {
  const { page, invoke } = app;
  const pdf = await createSamplePdf("preview");

  try {
    await invoke<WorkspaceState>("dropPdf", { path: pdf.path });

    const iframe = page.locator('iframe[title="Booklet preview"]');
    await expect(iframe).toBeVisible({ timeout: 45_000 });
    await expect(iframe).toHaveAttribute(
      "src",
      /https:\/\/preview\.pdfdroplet\//
    );

    const state = await invoke<WorkspaceState>("requestState");
    expect(state.hasIncomingPdf).toBe(true);
    expect(state.generatedPdfPath).toMatch(/^https:\/\/preview\.pdfdroplet\//);
  } finally {
    await pdf.dispose();
  }
});

test("allows selecting layouts and toggling settings", async ({ app }) => {
  const { page, invoke } = app;
  const pdf = await createSamplePdf("layout-toggle");

  try {
    const initialState = await invoke<WorkspaceState>("dropPdf", {
      path: pdf.path,
    });

    const iframe = page.locator('iframe[title="Booklet preview"]');
    await expect(iframe).toBeVisible({ timeout: 45_000 });
    await expect(iframe).toHaveAttribute("src", /https:\/\/preview\.pdfdroplet\//);

    const layouts = await invoke<LayoutMethodSummary[]>("requestLayouts");
    const alternateLayout = layouts.find(
      (layout: LayoutMethodSummary) =>
        layout.isEnabled && layout.id !== initialState.selectedLayoutId
    );

    expect(alternateLayout).toBeDefined();

    await page
      .getByRole("button", {
        name: new RegExp(alternateLayout!.displayName, "i"),
      })
      .click();

    await expect.poll(async () => {
      const state = await invoke<WorkspaceState>("requestState");
      return state.selectedLayoutId;
    }).toBe(alternateLayout!.id);
  const rtlToggle = page.getByLabel("Right-to-Left Language");
  await expect(rtlToggle).toBeEnabled();
  await rtlToggle.setChecked(true);
    await expect.poll(async () => {
      const state = await invoke<WorkspaceState>("requestState");
      return state.rightToLeft;
    }).toBe(true);

  const cropToggle = page.getByLabel("Crop Marks");
  await expect(cropToggle).toBeEnabled();
  await cropToggle.setChecked(true);
    await expect.poll(async () => {
      const state = await invoke<WorkspaceState>("requestState");
      return state.showCropMarks;
    }).toBe(true);
  } finally {
    await pdf.dispose();
  }
});

test("updates paper selection and syncs workspace state", async ({ app }) => {
  const { page, invoke } = app;
  const pdf = await createSamplePdf("paper");

  try {
    await invoke<WorkspaceState>("dropPdf", { path: pdf.path });

    const initialState = await invoke<WorkspaceState>("requestState");
    const iframe = page.locator('iframe[title="Booklet preview"]');
    await expect(iframe).toBeVisible({ timeout: 45_000 });
    await expect(iframe).toHaveAttribute("src", /https:\/\/preview\.pdfdroplet\//);

    const paperTargets = await invoke<PaperTargetInfo[]>("requestPaperTargets");
    const alternatePaper = paperTargets.find(
      (paper: PaperTargetInfo) => paper.id !== initialState.selectedPaperId
    );

    expect(alternatePaper).toBeDefined();

    const paperSelect = page.locator("aside select").first();
    await expect(paperSelect).toBeEnabled();
    await paperSelect.selectOption(alternatePaper!.id);

    await expect.poll(async () => {
      const state = await invoke<WorkspaceState>("requestState");
      return state.selectedPaperId;
    }).toBe(alternatePaper!.id);
  } finally {
    await pdf.dispose();
  }
});

test("reload previous reprocesses the booklet", async ({ app }) => {
  const { page, invoke } = app;
  const pdf = await createSamplePdf("reload");

  try {
    const stateAfterDrop = await invoke<WorkspaceState>("dropPdf", {
      path: pdf.path,
    });

    expect(stateAfterDrop.canReloadPrevious).toBe(true);

    const reloadButton = page.getByRole("button", { name: /Open Previous/ });
    await expect(reloadButton).toBeVisible();

    const initialGeneratedPath = stateAfterDrop.generatedPdfPath;

    await reloadButton.click();

    const loadingOverlay = page.getByText("Loading previous PDF…");
    await loadingOverlay
      .first()
      .waitFor({ state: "visible", timeout: 10_000 })
      .catch(() => undefined);

    await expect.poll(async () => {
      const state = await invoke<WorkspaceState>("requestState");
      return state.generatedPdfPath;
    }).not.toBe(initialGeneratedPath);

    const refreshedState = await invoke<WorkspaceState>("requestState");
    expect(refreshedState.hasIncomingPdf).toBe(true);
    expect(refreshedState.generatedPdfPath).toMatch(
      /^https:\/\/preview\.pdfdroplet\//
    );
  } finally {
    await pdf.dispose();
  }
});
