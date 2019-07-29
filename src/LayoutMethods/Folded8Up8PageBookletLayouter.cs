using System;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet.LayoutMethods
{
	/// <summary>
	/// Layout an 8-page booklet that is printed 8up on one side of one sheet of page.  The output
	/// page is folded (and partially cut) to allow a complete booklet without staples or other binding.
	/// The input pdf must be in portrait orientation.
	/// </summary>
	/// <remarks>
	/// The original request was for A6 portrait pages to be transformed for 8up A3 paper printing.
	/// This code makes no assumptions about input size or output size.  It will mindlessly lay out
	/// A4 portrait pages 8up shrunk onto letter sized output.  Whether the result is satisfactory
	/// is up to the user...
	/// </remarks>
	public class Folded8Up8PageBookletLayouter : LayoutMethod
	{
		public Folded8Up8PageBookletLayouter() : base("folded8Up8PageBooklet.png")
		{
		}

		public override string ToString()
		{
			return "Fold/Cut 8Up 8 Page Booklet";
		}

		public override bool GetIsEnabled(bool isLandscape)
		{
			return !isLandscape;
		}

		protected override void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats)
		{
			using (XGraphics gfx = GetGraphicsForNewPage(outputDocument))
			{
				var inputPages = Math.Min(_inputPdf.PageCount, 8);
				for (var idx = 1; idx <= inputPages; ++idx)
				{
					switch (idx)
					{
						case 1:
							Draw8UpPageFor8PageBooklet(gfx, idx, _paperWidth / 4, _paperHeight / 2);		// Draw Bottom Inner Left
							break;
						case 2:
							Draw8UpPageFor8PageBooklet(gfx, idx, _paperWidth / 2, _paperHeight / 2);		// Draw Bottom Inner Right
							break;
						case 3:
							Draw8UpPageFor8PageBooklet(gfx, idx, (3 * _paperWidth) / 4, _paperHeight / 2);	// Draw Bottom Right Corner
							break;
						case 4:
							Draw8UpPageFor8PageBooklet(gfx, idx, (3 * _paperWidth) / 4, 0);		// Draw Top Right Corner
							break;
						case 5:
							Draw8UpPageFor8PageBooklet(gfx, idx, _paperWidth / 2, 0);			// Draw Top Inner Right
							break;
						case 6:
							Draw8UpPageFor8PageBooklet(gfx, idx, _paperWidth / 4, 0);			// Draw Top Inner Left
							break;
						case 7:
							Draw8UpPageFor8PageBooklet(gfx, idx, 0, 0);							// Draw Top Left Corner
							break;
						case 8:
							Draw8UpPageFor8PageBooklet(gfx, idx, 0, _paperHeight / 2);			// Draw Bottom Left Corner
							break;
					}
				}
			}
		}

		private void Draw8UpPageFor8PageBooklet(XGraphics gfx, int pageNumber, double xorigin, double yorigin)
		{
			var state = gfx.Save();
			_inputPdf.PageNumber = pageNumber;
			var box = new XRect(xorigin, yorigin, _paperWidth / 4, _paperHeight / 2);
			if (yorigin == 0)
			{
				var pagePoint = new XPoint(xorigin + _paperWidth / 8, yorigin + _paperHeight / 4);
				gfx.RotateAtTransform(180, pagePoint);
			}
			gfx.DrawImage(_inputPdf, box);
			gfx.Restore(state);
		}
	}
}