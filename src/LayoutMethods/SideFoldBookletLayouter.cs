using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet.LayoutMethods
{
    /// <summary>
    /// TODO: separate out the calendar case from this (which is triggered when input width>height)
    /// and when that is done, we should also flip the pages (currently you have to flip the physical paper to make it work)
    /// </summary>
    public class SideFoldBookletLayouter : LayoutMethod
    {
        public SideFoldBookletLayouter():base("sideFoldBooklet.png")
        {
            
        }

        public override string ToString()
        {
            return "Fold Booklet";
        }

        protected override void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats)
        {
            XGraphics gfx;
            for (int idx = 1; idx <= numberOfSheetsOfPaper; idx++)
            {
                // Front page of a sheet:
                using (gfx = GetGraphicsForNewPage(outputDocument))
                {
                    //Left side of front
                    if (vacats > 0) // Skip if left side has to remain blank
                        vacats -= 1;
                    else
                    {
                        DrawSuperiorSide(gfx, numberOfPageSlotsAvailable + 2 * (1 - idx));
                    }

                    //Right side of the front
                    DrawInferiorSide(gfx, 2 * idx - 1);
                }

                // Back page of a sheet
                using (gfx = GetGraphicsForNewPage(outputDocument))
                {
                    if (2 * idx <= _inputPdf.PageCount) //prevent asking for page 2 with a single page document (JH Oct 2010)
                    {
                        //Left side of back
                        DrawSuperiorSide(gfx, 2 * idx);
                    }

                    //Right side of the Back
                    if (vacats > 0) // Skip if right side has to remain blank
                        vacats -= 1;
                    else
                    {
                        DrawInferiorSide(gfx, numberOfPageSlotsAvailable + 1 - 2 * idx);
                    }
                }
            }
        }

        public override bool GetIsEnabled(bool isLandscape)
        {
            return !isLandscape;
        }


        /// With the portrait, left-to-right-language mode, this is the Right side.
        /// With the landscape, this is the bottom half.
        private void DrawInferiorSide(XGraphics gfx, int pageNumber /* NB: page number is one-based*/)
        {
            _inputPdf.PageNumber = pageNumber;
            XRect box;
            if (_inputPdf.PixelWidth > _inputPdf.PixelHeight)//landscape calendar
            {
                box = new XRect(0, _outputHeight / 2, _outputWidth, _outputHeight / 2);
            }
            else
            {
                var leftEdge = _rightToLeft ? 0 : _outputWidth / 2;
                box = new XRect(leftEdge, 0, _outputWidth / 2, _outputHeight);
            }
            gfx.DrawImage(_inputPdf, box);
        }

        /// <summary>
        /// With the portrait, left-to-right-language mode, this is the Left side.
        /// With the landscape, this is the top half.
        /// </summary>
        private void DrawSuperiorSide(XGraphics gfx, int pageNumber)
        {
            _inputPdf.PageNumber = pageNumber;
            XRect box;
            if (_inputPdf.PixelWidth > _inputPdf.PixelHeight)//landscape calendar
            {
                box = new XRect(0, 0, _outputWidth, _outputHeight / 2);
            }

            else
            {
                var leftEdge = _rightToLeft ? _outputWidth / 2 : 0;
                box = new XRect(leftEdge, 0, _outputWidth / 2, _outputHeight);
            }
            gfx.DrawImage(_inputPdf, box);

        }

    }
}
