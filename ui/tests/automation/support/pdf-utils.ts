import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";

const SAMPLE_PDF_TEMPLATE = `%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>
endobj
4 0 obj
<< /Length 55 >>
stream
BT
/F1 24 Tf
100 700 Td
(Hello PdfDroplet) Tj
ET
endstream
endobj
5 0 obj
<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>
endobj
xref
0 6
0000000000 65535 f 
0000000010 00000 n 
0000000061 00000 n 
0000000116 00000 n 
0000000273 00000 n 
0000000369 00000 n 
trailer
<< /Root 1 0 R /Size 6 >>
startxref
433
%%EOF
`;

export type TempPdf = {
  path: string;
  dispose: () => Promise<void>;
};

export async function createSamplePdf(prefix: string): Promise<TempPdf> {
  const tempDir = await fs.mkdtemp(path.join(os.tmpdir(), "pdfdroplet-ui-"));
  const filePath = path.join(tempDir, `${prefix}-${Date.now()}.pdf`);
  await fs.writeFile(filePath, SAMPLE_PDF_TEMPLATE, "utf8");

  return {
    path: filePath,
    dispose: async () => {
      await fs.unlink(filePath).catch(() => undefined);
      await fs.rmdir(tempDir).catch(() => undefined);
    },
  };
}
