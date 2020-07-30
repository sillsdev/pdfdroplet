using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet.LayoutMethods
{
    /// <summary>
    /// this is for when the input document is landscape, and we want a printout that can be cut down the middle (e.g. making a4 paper into a5 slices)
    /// This means that along the top, we have the first half of the book, and along the bottom, the second half.
    /// </summary>
    public class CutLandscapeLayout : LayoutMethod
    {
        public CutLandscapeLayout():base("cutBooklet.png")
        {
            
        }
        public override string ToString()
        {
            return "Cut && Stack";
        }

        public override bool GetIsEnabled(XPdfForm inputPdf)
        {
            return IsLandscape(inputPdf);
        }

		protected override void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats)
        {
            XGraphics gfx;
            for (int sheetOfPaperIndex = 1; sheetOfPaperIndex <= numberOfSheetsOfPaper; sheetOfPaperIndex++)
            {
                // Front page of a sheet:
                using (gfx = GetGraphicsForNewPage(outputDocument))
                {
                    DrawTop(gfx, 2*sheetOfPaperIndex - 1);

                    var inputPageNumber = numberOfPageSlotsAvailable/2 + (2*sheetOfPaperIndex) - 1;
                    //if ((sheetOfPaperIndex - 1) + 3 <= (numberOfPageSlotsAvailable - vacats)) // Skip if 
                    if (inputPageNumber <= (numberOfPageSlotsAvailable - vacats))
                    {
                        DrawBottom(gfx, inputPageNumber);
                    }
                }

                // Back page of a sheet
                using (gfx = GetGraphicsForNewPage(outputDocument))
                {
                    if (2*sheetOfPaperIndex <= _inputPdf.PageCount)
                        //prevent asking for page 2 with a single page document (JH Oct 2010)
                    {
                        DrawTop(gfx, 2*sheetOfPaperIndex);
                    }

                    var inputPageNumberBack = numberOfPageSlotsAvailable/2 + (2*sheetOfPaperIndex);
                    if(inputPageNumberBack <= (numberOfPageSlotsAvailable - vacats))
                    {
                        DrawBottom(gfx, inputPageNumberBack);
                    }
                }
            }
        }

        private void DrawBottom(XGraphics gfx, int pageNumber /* NB: page number is one-based*/)
        {
            _inputPdf.PageNumber = pageNumber;
            XRect box = new XRect(0, _paperHeight / 2, _paperWidth, _paperHeight / 2);
            gfx.DrawImage(_inputPdf, box);
         }

        private void DrawTop(XGraphics gfx, int pageNumber)
        {
            _inputPdf.PageNumber = pageNumber;
            XRect box = new XRect(0, 0, _paperWidth, _paperHeight / 2);
            gfx.DrawImage(_inputPdf, box);
        }

    }
}