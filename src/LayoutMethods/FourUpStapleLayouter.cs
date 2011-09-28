using System;
using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet
{
    /// <summary>
    /// this is for when the input document is portrait, and we want a printout which is 1/4 the paper size (e.g., a6 booklets),
    /// and we will staple the results.  Each paper with have 4 copies of the same page
    /// </summary>
    public class FourUpStapleLayouter : LayoutMethod
    {
        public FourUpStapleLayouter() : base("4CopyStaple.png")
        {
            PagesPerTwoSidedSheet = 8;
            DoRotate = false;
        }

        public override string ToString()
        {
            return "Four-Up Staple";
        }

        public override bool GetIsEnabled(bool isLandscape)
        {
            return !isLandscape;
        }

        private enum Corner
        {
            topLeft,
            topRight,
            bottomLeft,
            bottomRight
        }


        protected override void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats)
        {
            XGraphics gfx;
            for (int sheetOfPaperIndex = 1; sheetOfPaperIndex <= numberOfSheetsOfPaper; sheetOfPaperIndex++)
            {
                // Front page of a sheet:
                using (gfx = GetGraphicsForNewPage(outputDocument))
                {
                    Draw(gfx, 2*sheetOfPaperIndex - 1);
                }

                // Back page of a sheet
                using (gfx = GetGraphicsForNewPage(outputDocument))
                {
                    if (2*sheetOfPaperIndex <= _inputPdf.PageCount)
                    {
                        Draw(gfx, 2*sheetOfPaperIndex);
                    }

//                    var inputPageNumberBack = numberOfPageSlotsAvailable/2 + (2*sheetOfPaperIndex);
//                    if(inputPageNumberBack <= (numberOfPageSlotsAvailable - vacats))
//                    {
//                        Draw(gfx, inputPageNumberBack);
//                    }

                }
            }
        }

        private void Draw(XGraphics gfx, int pageNumber)
        {
            _inputPdf.PageNumber = pageNumber;
            XRect box;

            gfx.DrawImage(_inputPdf, new XRect(0, 0, _outputWidth / 2, _outputHeight / 2));
            gfx.DrawImage(_inputPdf, new XRect(0, _outputWidth / 2, _outputWidth / 2, _outputHeight / 2));
            gfx.DrawImage(_inputPdf, new XRect(_outputHeight/2, 0, _outputWidth/2, _outputHeight/2));
            gfx.DrawImage(_inputPdf, new XRect(_outputHeight/2, _outputWidth/2, _outputWidth/2, _outputHeight/2));
        }
    }
}