using System;
using System.Drawing;
using System.IO;
using SIL.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet.LayoutMethods
{
    public class NullLayoutMethod :LayoutMethod
    {
        public NullLayoutMethod() : base("")
        {
        }

		public override void Layout(XPdfForm inputPdf, string inputPath, string outputPath, PaperTarget paperTarget, bool rightToLeft, bool showCropMarks)
        {
            if (!showCropMarks)
            {
	            File.Copy(inputPath, outputPath,true); // we don't have any value to add, so just deliver a copy of the original
            }
            else
            {
				//_rightToLeft = rightToLeft;
				_inputPdf = inputPdf;
				_showCropMarks = showCropMarks;

	            PdfDocument outputDocument = new PdfDocument();
				outputDocument.PageLayout = PdfPageLayout.SinglePage;

	            _paperWidth = _inputPdf.PixelWidth;
				_paperHeight =_inputPdf.PixelHeight;
//	            if (showCropMarks)
//	            {
//					_paperWidth.Millimeter += (2.0 * LayoutMethod.kMillimetersBetweenTrimAndMediaBox);
//					_paperHeight.Millimeter += (2.0 * LayoutMethod.kMillimetersBetweenTrimAndMediaBox);
//	            }
	            for (int idx = 1; idx <= _inputPdf.PageCount; idx++)
	            {
		            using (XGraphics gfx = GetGraphicsForNewPage(outputDocument))
		            {
			            DrawPage(gfx, idx);
		            }
	            }

	            outputDocument.Save(outputPath);
            }
        }

	    protected override void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats)
	    {
		    throw new NotImplementedException();
	    }

	    private void DrawPage(XGraphics targetGraphicsPort, int pageNumber)
	    {
		    _inputPdf.PageNumber = pageNumber;
			
		    XRect sourceRect = new XRect(0, 0, _inputPdf.PixelWidth,_inputPdf.PixelHeight);	    
			//what's not obvious here is that the targetGraphicsPort has previously been
			//set up to do a transformation to shift the content down and to the right into its trimbox
		    targetGraphicsPort.DrawImage(_inputPdf, sourceRect);
	    }

	    public override bool GetIsEnabled(bool isLandscape)
        {
            return true;
        }
        public override string ToString()
        {
            return "Original";
        }

        public override Image GetImage(bool isLandscape)
        {
            return Image.FromFile(FileLocator.GetFileDistributedWithApplication("images", isLandscape ? "originalLandscape.png" : "originalPortrait.png"));
        }

        public override bool ImageIsSensitiveToOrientation
        {
            get { return true; }
        }

    }
}
