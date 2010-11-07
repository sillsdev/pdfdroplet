using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet
{
    public class Converter
    {
         public static void Convert(string inputPath, string outputPath, PaperTarget paperTarget) 
        {
            PdfDocument outputDocument = new PdfDocument();

            // Show single pages
            // (Note: one page contains two pages from the source document.
            //  If the number of pages of the source document can not be
            //  divided by 4, the first pages of the output document will
            //  each contain only one page from the source document.)
            outputDocument.PageLayout = PdfPageLayout.SinglePage;

            XGraphics gfx;

            // Open the external document as XPdfForm object
            XPdfForm inputPdf = OpenDocumentForPdfSharp(inputPath);
            // Determine width and height
            double outputWidth = paperTarget.GetOutputDimensions(inputPdf.PixelWidth,inputPdf.PixelHeight).X;
            double outputHeight = paperTarget.GetOutputDimensions(inputPdf.PixelWidth, inputPdf.PixelHeight).Y;

            int inputPages = inputPdf.PageCount;
            int sheets = inputPages / 4;
            if (sheets * 4 < inputPages)
                sheets += 1;
            int allpages = 4 * sheets;
            int vacats = allpages - inputPages;

            for (int idx = 1; idx <= sheets; idx += 1)
            {
                // Front page of a sheet:
                PdfPage page = outputDocument.AddPage();
                page.Orientation = PageOrientation.Landscape;
                page.Width =  outputWidth;
                page.Height = outputHeight;

                gfx = XGraphics.FromPdfPage(page);

                XRect box;

                //Left side of front
                if (vacats > 0)  // Skip if left side has to remain blank
                    vacats -= 1;
                else
                {
                    inputPdf.PageNumber = allpages + 2 * (1 - idx); // NB: page numberis one-based
                    box = new XRect(0, 0, outputWidth / 2, outputHeight);
                    gfx.DrawImage(inputPdf, box);
                }

                //Right side of the front
                inputPdf.PageNumber = 2 * idx - 1;
                box = new XRect(outputWidth / 2, 0, outputWidth / 2, outputHeight);
                gfx.DrawImage(inputPdf, box);

                // Back page of a sheet
                page = outputDocument.AddPage();
                page.Orientation = PageOrientation.Landscape;
                page.Width =  outputWidth;
                page.Height = outputHeight;

                gfx = XGraphics.FromPdfPage(page);

                if (2 * idx <= inputPdf.PageCount) //prevent asking for page 2 with a single page document (JH Oct 2010)
                {
                    //Left side of back
                    inputPdf.PageNumber = 2 * idx;
                    box = new XRect(0, 0, outputWidth / 2, outputHeight);
                    gfx.DrawImage(inputPdf, box);
                }

                //Right side of the Back
                if (vacats > 0) // Skip if right side has to remain blank
                    vacats -= 1;
                else
                {
                    inputPdf.PageNumber = allpages + 1 - 2 * idx;
                    box = new XRect(outputWidth / 2, 0, outputWidth / 2, outputHeight);
                    gfx.DrawImage(inputPdf, box);
                }
            }

            outputDocument.Save(outputPath);
        }

        /// <summary>
        /// from http://forum.pdfsharp.net/viewtopic.php?p=2069
        /// Get a version of the document which pdfsharp can open, downgrading if necessary
        /// </summary>
        static public XPdfForm OpenDocumentForPdfSharp(string path)
        {
            try
            {
                var form = XPdfForm.FromFile(path);
                //this causes it to notice if can't actually read it
                int dummy = form.PixelWidth;
                return form;
            }
            catch (PdfSharp.Pdf.IO.PdfReaderException)
            {
                //workaround if pdfsharp doesnt dupport this pdf
                return XPdfForm.FromFile(WritePdf1pt4Version(path));
            }
        }


        /// <summary>
        /// from http://forum.pdfsharp.net/viewtopic.php?p=2069
        /// uses itextsharp to convert any pdf to 1.4 compatible pdf
        /// </summary>
        static private string WritePdf1pt4Version(string inputPath)
        {
            var tempFileName = Path.GetTempFileName();
            File.Delete(tempFileName);
            string outputPath = tempFileName + ".pdf";

            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(inputPath);

            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // step 1: creation of a document-object
            iTextSharp.text.Document document = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            // step 2: we create a writer that listens to the document
            iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));
            //write pdf that pdfsharp can understand
            writer.SetPdfVersion(iTextSharp.text.pdf.PdfWriter.PDF_VERSION_1_4);
            // step 3: we open the document
            document.Open();
            iTextSharp.text.pdf.PdfContentByte cb = writer.DirectContent;
            iTextSharp.text.pdf.PdfImportedPage page;

            int rotation;

            int i = 0;
            while (i < n)
            {
                i++;
                document.SetPageSize(reader.GetPageSizeWithRotation(i));
                document.NewPage();
                page = writer.GetImportedPage(reader, i);
                rotation = reader.GetPageRotation(i);
                if (rotation == 90 || rotation == 270)
                {
                    cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                }
                else
                {
                    cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                }
            }
            // step 5: we close the document
            document.Close();
            return outputPath;
        }

    }
}
