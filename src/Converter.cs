using System.IO;
using PdfSharp.Drawing;

namespace PdfDroplet
{
    public class Converter
    {

        public void Convert(string inputPath, string outputPath, PaperTarget paperTarget, bool rightToLeft, bool landscapeAsCalendar)
        {
            var inputPdf = OpenDocumentForPdfSharp(inputPath);

            Layouter layouter;
            if(inputPdf.PixelWidth > inputPdf.PixelHeight)
            {
                if(landscapeAsCalendar)
                {
                    layouter = new PortraitLayouter(); // not a typo... yet.
                }
                else
                {
                    layouter = new CutLandscapeLayouter(); 
                }
            }
            else
            {
                layouter = new PortraitLayouter();     
            }
            layouter.Layout(inputPath, outputPath, paperTarget, rightToLeft, landscapeAsCalendar, inputPdf);
        }


        /// <summary>
        /// from http://forum.pdfsharp.net/viewtopic.php?p=2069
        /// Get a version of the document which pdfsharp can open, downgrading if necessary
        /// </summary>
        static private XPdfForm OpenDocumentForPdfSharp(string path)
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
