import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import { PDFDocument, StandardFonts, rgb } from "pdf-lib";

export type TempPdf = {
  path: string;
  dispose: () => Promise<void>;
};

export async function createSamplePdf(prefix: string): Promise<TempPdf> {
  const tempDir = await fs.mkdtemp(path.join(os.tmpdir(), "pdfdroplet-ui-"));
  const filePath = path.join(tempDir, `${prefix}-${Date.now()}.pdf`);

  // Create a proper PDF using pdf-lib that PdfSharp can read
  const pdfDoc = await PDFDocument.create();
  const page = pdfDoc.addPage([612, 792]); // Letter size
  const font = await pdfDoc.embedFont(StandardFonts.Helvetica);

  page.drawText("Hello PdfDroplet", {
    x: 100,
    y: 700,
    size: 24,
    font: font,
    color: rgb(0, 0, 0),
  });

  const pdfBytes = await pdfDoc.save();
  await fs.writeFile(filePath, pdfBytes);

  return {
    path: filePath,
    dispose: async () => {
      await fs.unlink(filePath).catch(() => undefined);
      await fs.rmdir(tempDir).catch(() => undefined);
    },
  };
}
