using System.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet.LayoutMethods
{
    /// <summary>
    /// Layout a 4up booklet that will be folded horizontally and cut vertically.  This may be
    /// either portrait or landscape in orientation depending on the original page layout.
    /// </summary>
    public class SideFold4UpSingleBookletLayouter : LayoutMethod
    {
        public SideFold4UpSingleBookletLayouter():base("sideFoldCut4UpSingleBooklet.svg")
        {

        }

        public override string ToString()
        {
            return "Fold/Cut Single 4Up Booklet";
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
			// Recalculate for showing 4up instead of 2up on each side of a sheet.
			// This layout minimizes the use of paper for a single booklet.
			int inputPages = _inputPdf.PageCount;
			numberOfSheetsOfPaper = inputPages / 8;
			if (numberOfSheetsOfPaper * 8 < inputPages)
				numberOfSheetsOfPaper += 1;
			numberOfPageSlotsAvailable = 8 * numberOfSheetsOfPaper;
			vacats = numberOfPageSlotsAvailable - inputPages;
	        Debug.Assert(vacats >= 0 && vacats < 8);
	        int vacatsSkipped = 0;
	        bool skipLastRow = vacats >= 4;

            XGraphics gfx;
            for (int idx = 1; idx <= numberOfSheetsOfPaper; idx++)
            {
	            bool onFirstSheet = idx == 1;
	            bool onLastSheet = idx == numberOfSheetsOfPaper;

                // Front side of a sheet:
				int topLeftFrontPage = numberOfPageSlotsAvailable - (4 * (idx - 1));
	            if (skipLastRow)
		            topLeftFrontPage -= 4;
				int bottomLeftFrontPage = topLeftFrontPage - 2;
				int topRightFrontPage = 4 * idx - 3;
				int bottomRightFrontPage = topRightFrontPage + 2;
				using (gfx = GetGraphicsForNewPage(outputDocument))
				{
	                if (onFirstSheet && topLeftFrontPage > inputPages)
		                ++vacatsSkipped;
	                else
						DrawTopLeftCorner(gfx, topLeftFrontPage);

					if ((onFirstSheet && bottomLeftFrontPage > inputPages) || (onLastSheet && skipLastRow))
		                ++vacatsSkipped;
	                else
		                DrawBottomLeftCorner(gfx, bottomLeftFrontPage);

	                DrawTopRightCorner(gfx, topRightFrontPage);

					if ((onFirstSheet && bottomRightFrontPage > inputPages) || (onLastSheet && skipLastRow))
		                ++vacatsSkipped;
					else
		                DrawBottomRightCorner(gfx, bottomRightFrontPage);
                }

                // Back side of a sheet:
				int topLeftBackPage = topRightFrontPage + 1;
				int bottomLeftBackPage = bottomRightFrontPage + 1;
				int topRightBackPage = topLeftFrontPage - 1;
				int bottomRightBackPage = bottomLeftFrontPage - 1;
				using (gfx = GetGraphicsForNewPage(outputDocument))
                {
					if (topLeftBackPage > inputPages)
						++vacatsSkipped;
					else
						DrawTopLeftCorner(gfx, topLeftBackPage);

					if ((onFirstSheet && bottomLeftBackPage > inputPages) || (onLastSheet && skipLastRow))
						++vacatsSkipped;
					else
						DrawBottomLeftCorner(gfx, bottomLeftBackPage);

					if (onFirstSheet && topRightBackPage > inputPages)
		                ++vacatsSkipped;
	                else
		                DrawTopRightCorner(gfx, topRightBackPage);

					if ((onFirstSheet && bottomRightBackPage > inputPages) || (onLastSheet && skipLastRow))
						++vacatsSkipped;
					else
						DrawBottomRightCorner(gfx, bottomRightBackPage);
				}
            }
			Debug.Assert(vacats == vacatsSkipped);
		}

		private void DrawTopLeftCorner(XGraphics gfx, int pageNumber /* NB: page number is one-based*/)
		{
			_inputPdf.PageNumber = pageNumber;
			var box = new XRect(LeftEdgeForSuperiorPage, 0, _paperWidth / 2, _paperHeight / 2);
			gfx.DrawImage(_inputPdf, box);
		}

		private void DrawBottomLeftCorner(XGraphics gfx, int pageNumber /* NB: page number is one-based*/)
		{
			_inputPdf.PageNumber = pageNumber;
			var box = new XRect(LeftEdgeForSuperiorPage, _paperHeight / 2, _paperWidth / 2, _paperHeight / 2);
			gfx.DrawImage(_inputPdf, box);
		}

		private void DrawTopRightCorner(XGraphics gfx, int pageNumber /* NB: page number is one-based*/)
		{
			_inputPdf.PageNumber = pageNumber;
			var box = new XRect(LeftEdgeForInferiorPage, 0, _paperWidth / 2, _paperHeight / 2);
			gfx.DrawImage(_inputPdf, box);
		}

		private void DrawBottomRightCorner(XGraphics gfx, int pageNumber /* NB: page number is one-based*/)
		{
			_inputPdf.PageNumber = pageNumber;
			var box = new XRect(LeftEdgeForInferiorPage, _paperHeight / 2, _paperWidth / 2, _paperHeight / 2);
			gfx.DrawImage(_inputPdf, box);
		}

        public override bool GetIsEnabled(XPdfForm inputPdf)
        {
            return IsPortrait(inputPdf) || IsLandscape(inputPdf);
        }
    }
}
