using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet.LayoutMethods
{
    /// <summary>
    /// Layout a 4up booklet that will be folded horizontally and cut vertically.  This may be
    /// either portrait or landscape in orientation depending on the original page layout.
    /// </summary>
    public class SideFold4UpBookletLayouter : LayoutMethod
    {
        public SideFold4UpBookletLayouter():base("sideFoldCut4UpBooklet.png")
        {

        }

        public override string ToString()
        {
            return "Fold/Cut 4Up Booklet";
        }

		/// <summary>
		/// 4up layout requires matching paper orientation instead of the opposite paper orientation.
		/// This method achieves that happy state.
		/// </summary>
		protected override void SetPaperSize(PaperTarget paperTarget)
		{
			var size = paperTarget.GetPaperDimensions(_inputPdf.PixelHeight, _inputPdf.PixelWidth);
			_paperWidth = size.X;
			_paperHeight = size.Y;
		}

		protected override void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats)
        {
			for (var idx = 1; idx <= numberOfSheetsOfPaper; idx++)
            {
	            XGraphics gfx;
				// Front page of a sheet:
				using (gfx = GetGraphicsForNewPage(outputDocument))
				{
					//Left side of front
					if (vacats > 0) // Skip if left side has to remain blank
						vacats -= 1;
					else
						DrawSuperiorSide(gfx, numberOfPageSlotsAvailable + 2 * (1 - idx));

					//Right side of the front
					DrawInferiorSide(gfx, 2 * idx - 1);
				}

				// Back page of a sheet
				using (gfx = GetGraphicsForNewPage(outputDocument))
				{
					if (2 * idx <= _inputPdf.PageCount) //prevent asking for page 2 with a single page document (JH Oct 2010)
						//Left side of back
						DrawSuperiorSide(gfx, 2 * idx);

					//Right side of the Back
					if (vacats > 0) // Skip if right side has to remain blank
						vacats -= 1;
					else
						DrawInferiorSide(gfx, numberOfPageSlotsAvailable + 1 - 2 * idx);
				}
			}
		}

		private void DrawInferiorSide(XGraphics gfx, int pageNumber)
		{
			_inputPdf.PageNumber = pageNumber;
			var box = new XRect(LeftEdgeForInferiorPage, 0, _paperWidth / 2, _paperHeight / 2);
			gfx.DrawImage(_inputPdf, box);
			_inputPdf.PageNumber = pageNumber;
			box = new XRect(LeftEdgeForInferiorPage, _paperHeight / 2, _paperWidth / 2, _paperHeight / 2);
			gfx.DrawImage(_inputPdf, box);
		}

		private void DrawSuperiorSide(XGraphics gfx, int pageNumber)
		{
			_inputPdf.PageNumber = pageNumber;
			var box = new XRect(LeftEdgeForSuperiorPage, 0, _paperWidth / 2, _paperHeight / 2);
			gfx.DrawImage(_inputPdf, box);
			_inputPdf.PageNumber = pageNumber;
			box = new XRect(LeftEdgeForSuperiorPage, _paperHeight / 2, _paperWidth / 2, _paperHeight / 2);
			gfx.DrawImage(_inputPdf, box);
		}

        public override bool GetIsEnabled(XPdfForm inputPdf)
        {
            return IsPortrait(inputPdf) || IsLandscape(inputPdf);
        }
    }
}
