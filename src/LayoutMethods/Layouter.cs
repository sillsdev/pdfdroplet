using System.Drawing;
using System.Drawing.Drawing2D;
using Palaso.IO;
using PdfDroplet.Properties;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet.LayoutMethods
{
    public abstract class LayoutMethod
    {
        private readonly string _imageName;
        protected XUnit _paperWidth;
        protected XUnit _paperHeight;
        protected XPdfForm _inputPdf;
        protected bool _rightToLeft;
        protected bool _calendarMode;
	    private bool _showCropMarks;


	    protected LayoutMethod(string imageName)
        {
            _imageName = imageName;
        }

        public virtual bool ImageIsSensitiveToOrientation
        {
            get { return false; }
        }

	    /// <summary>
	    /// Produce a new pdf with rearranged pages
	    /// </summary>
	    /// <param name="inputPdf">the source pdf</param>
	    /// <param name="inputPath">the path to the source pdf (used by null layouter)</param>
	    /// <param name="outputPath"></param>
	    /// <param name="paperTarget">The size of the pages of the output pdf</param>
	    /// <param name="rightToLeft">Is this a right-to-left language?  Might be better-named "backToFront"</param>
	    /// <param name="showCropMarks">For commercial printing, make a Trimbox, BleedBox, and crop marks</param>
	    public virtual void Layout(XPdfForm inputPdf, string inputPath, string outputPath, PaperTarget paperTarget, bool rightToLeft, bool showCropMarks)
        {
            _rightToLeft = rightToLeft;
            _inputPdf = inputPdf;
		    _showCropMarks = showCropMarks;

            PdfDocument outputDocument = new PdfDocument();

            // Show single pages
            // (Note: one page contains two pages from the source document.
            //  If the number of pages of the source document can not be
            //  divided by 4, the first pages of the output document will
            //  each contain only one page from the source document.)
            outputDocument.PageLayout = PdfPageLayout.SinglePage;

            // Determine width and height
            _paperWidth = paperTarget.GetPaperDimensions(_inputPdf.PixelWidth, _inputPdf.PixelHeight).X;
            _paperHeight = paperTarget.GetPaperDimensions(_inputPdf.PixelWidth, _inputPdf.PixelHeight).Y;


            int inputPages = _inputPdf.PageCount;
            int numberOfSheetsOfPaper = inputPages / 4;
            if (numberOfSheetsOfPaper * 4 < inputPages)
                numberOfSheetsOfPaper += 1;
            int numberOfPageSlotsAvailable = 4 * numberOfSheetsOfPaper;
            int vacats = numberOfPageSlotsAvailable - inputPages;

			LayoutInner(outputDocument, numberOfSheetsOfPaper, numberOfPageSlotsAvailable, vacats);
           
//            if(true)
//                foreach (PdfPage page in outputDocument.Pages)
//                {
//                    
//                   var  gfx = XGraphics.FromPdfPage(page);
//                    gfx.DrawImage(page, 0.0,0.0);
//                    page.MediaBox = new PdfRectangle(new XPoint(m.X2, m.Y1), new XPoint(m.X1, m.Y2));
//                }
            outputDocument.Save(outputPath);
        }

		protected abstract void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats);


	    protected XGraphics GetGraphicsForNewPage(PdfDocument outputDocument)
	    {
		    XGraphics gfx;
		    PdfPage page = outputDocument.AddPage();
		    //page.Orientation = PageOrientation.Landscape;//review: why does this say it's always landscape (and why does that work?) Or maybe it has no effect?

		    const double millimetersBetweenTrimAndMediaBox = 6; //I read that "3.175" is standard, but then the crop marks are barely visible. I'm concerned that if they aren't obvious, people might not understand what they are seeing, and be confused.
			var xunitsBetweenTrimAndMediaBox = XUnit.FromMillimeter(millimetersBetweenTrimAndMediaBox);

			if (_showCropMarks)
		    {
				XPoint upperLeftTrimBoxCorner = new XPoint(xunitsBetweenTrimAndMediaBox, xunitsBetweenTrimAndMediaBox);
			    page.Width = XUnit.FromMillimeter(_paperWidth.Millimeter+(2.0*millimetersBetweenTrimAndMediaBox));
				page.Height = XUnit.FromMillimeter(_paperHeight.Millimeter + (2.0 * millimetersBetweenTrimAndMediaBox)); ;
			    page.TrimBox = new PdfRectangle (upperLeftTrimBoxCorner, new XSize(_paperWidth, _paperHeight));
			    //page.CropBox = page.TrimBox;
		    }
		    else
		    {
				page.Width = _paperWidth;
				page.Height = _paperHeight;
		    }

			gfx = XGraphics.FromPdfPage(page);

			if (_showCropMarks)
		    {
			    DrawCropMarks(page, gfx, xunitsBetweenTrimAndMediaBox);
				//push the page down and to the left
				gfx.TranslateTransform(xunitsBetweenTrimAndMediaBox,xunitsBetweenTrimAndMediaBox);
		    }

		    if (Settings.Default.Mirror)
            {
                Matrix mirrorMatrix = new Matrix(-1, 0, 0, 1, 0, 0);
                gfx.MultiplyTransform(mirrorMatrix);
                gfx.TranslateTransform(-_paperWidth, 1);
            }

            return gfx;
        }

	    private static void DrawCropMarks(PdfPage page, XGraphics gfx, XUnit xunitsBetweenTrimAndMediaBox)
	    {
		    XPoint upperLeftTrimBoxCorner = page.TrimBox.ToXRect().TopLeft;
			XPoint upperRightTrimBoxCorner = page.TrimBox.ToXRect().TopRight;
		    XPoint lowerLeftTrimBoxCorner = page.TrimBox.ToXRect().BottomLeft;
		    XPoint lowerRightTrimBoxCorner = page.TrimBox.ToXRect().BottomRight;

			//while blue would look nicer, then if they make color separations, the marks wouldn't show all all of them. 
			//Note that in InDesign, there is a "registration color" which looks black but is actually 100% of all each 
			//sep color, so it always prints. But I don't see a way to do that in PDF.
			//.25 is a standard width
			var pen = new XPen(XColor.FromKnownColor(XKnownColor.Black), .25); 
			
		    var gapLength = XUnit.FromMillimeter(3.175); // this 3.175 is the industry standard

		    gfx.DrawLine(pen, upperLeftTrimBoxCorner.X - gapLength, upperLeftTrimBoxCorner.Y,
		                 upperLeftTrimBoxCorner.X - xunitsBetweenTrimAndMediaBox, upperLeftTrimBoxCorner.Y);
		    gfx.DrawLine(pen, upperLeftTrimBoxCorner.X, upperLeftTrimBoxCorner.Y - gapLength, upperLeftTrimBoxCorner.X,
		                 upperLeftTrimBoxCorner.Y - xunitsBetweenTrimAndMediaBox);

		    gfx.DrawLine(pen, upperRightTrimBoxCorner.X + gapLength, upperRightTrimBoxCorner.Y,
		                 upperRightTrimBoxCorner.X + xunitsBetweenTrimAndMediaBox, upperLeftTrimBoxCorner.Y);
		    gfx.DrawLine(pen, upperRightTrimBoxCorner.X, upperRightTrimBoxCorner.Y - gapLength, upperRightTrimBoxCorner.X,
		                 upperLeftTrimBoxCorner.Y - xunitsBetweenTrimAndMediaBox);

		    gfx.DrawLine(pen, lowerLeftTrimBoxCorner.X - gapLength, lowerLeftTrimBoxCorner.Y,
		                 lowerLeftTrimBoxCorner.X - xunitsBetweenTrimAndMediaBox, lowerLeftTrimBoxCorner.Y);
		    gfx.DrawLine(pen, lowerLeftTrimBoxCorner.X, lowerLeftTrimBoxCorner.Y + gapLength, lowerLeftTrimBoxCorner.X,
		                 lowerLeftTrimBoxCorner.Y + xunitsBetweenTrimAndMediaBox);

		    gfx.DrawLine(pen, lowerRightTrimBoxCorner.X + gapLength, lowerRightTrimBoxCorner.Y,
		                 lowerRightTrimBoxCorner.X + xunitsBetweenTrimAndMediaBox, lowerRightTrimBoxCorner.Y);
		    gfx.DrawLine(pen, lowerRightTrimBoxCorner.X, lowerRightTrimBoxCorner.Y + gapLength, lowerRightTrimBoxCorner.X,
		                 lowerRightTrimBoxCorner.Y + xunitsBetweenTrimAndMediaBox);
	    }


	    public abstract bool GetIsEnabled(bool isLandscape);

        public virtual Image GetImage(bool isLandscape)
        {
            return Image.FromFile(FileLocator.GetFileDistributedWithApplication("images", _imageName));
        }
    }
}