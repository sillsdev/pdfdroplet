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
		protected double _trimBoxDelta;
		public NullLayoutMethod(double trimBoxDelta=0.0) : base("")
		{
			_trimBoxDelta = trimBoxDelta;
		}

		public override void Layout(XPdfForm inputPdf, string inputPath, string outputPath, PaperTarget paperTarget, bool rightToLeft, bool showCropMarks)
        {
	        if (!showCropMarks && _trimBoxDelta < 0.1) // could say _trimBoxDelta==0.0 if we fully trusted double comparisons...
	        {
		        File.Copy(inputPath, outputPath, true); // we don't have any value to add, so just deliver a copy of the original
	        }
			else
			{
				//_rightToLeft = rightToLeft;
				_inputPdf = inputPdf;
				_showCropMarks = showCropMarks;

	            PdfDocument outputDocument = new PdfDocument();
				outputDocument.PageLayout = PdfPageLayout.SinglePage;

				// Despite the name, PixelWidth is the same as PointWidth, just as an integer instead of
				// double precision.  We may as well use all the precision we can get.
				_paperWidth = _inputPdf.PointWidth;
				_paperHeight =_inputPdf.PointHeight;

				if (_trimBoxDelta > 0.1)
				{
					// This sets the ArtBox and TrimBox to be offset inside the page boundaries,
					// and increases the page size (CropBox, BleedBox, and MediaBox) accordingly.
					// If we don't set the TrimMargins, only the MediaBox is set inside the PDF,
					// and that to the original values for the input PDF's page size.
					outputDocument.Settings.TrimMargins = new TrimMargins
					{
						All = new XUnit(_trimBoxDelta, XGraphicsUnit.Millimeter)
					};
				}

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

	    public override bool GetIsEnabled(XPdfForm inputPdf)
        {
            return true;
        }
        public override string ToString()
        {
            return "Original";
        }

        public override Image GetImage(bool isLandscape)
        {
            return Image.FromFile(FileLocationUtilities.GetFileDistributedWithApplication("images", isLandscape ? "originalLandscape.png" : "originalPortrait.png"));
        }

        public override bool ImageIsSensitiveToOrientation
        {
            get { return true; }
        }

    }
}
