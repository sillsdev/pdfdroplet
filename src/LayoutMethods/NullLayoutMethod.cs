using System;
using System.Drawing;
using System.IO;
using SIL.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfDroplet.LayoutMethods
{
	/// <summary>
	/// Pass the input PDF file along to the output, optionally setting the TrimBox, ArtBox, and BleedBox
	/// to a smaller size offset inside the MediaBox (and CropBox).
	/// </summary>
    public class NullLayoutMethod :LayoutMethod
    {
		protected double _bleedMM;
		private const double kBleedMicroDeltaMM = 0.01; // use for floating point comparisons instead of 0.0

		/// <summary>
		/// bleedMM is the amount in mm to offset the TrimBox et al. inside the MediaBox.  The default is 0mm
		/// (TrimBox the same as the MediaBox).
		/// </summary>
		/// <param name="bleedMM"></param>
		public NullLayoutMethod(double bleedMM=0.0) : base("")
		{
			_bleedMM = bleedMM;
		}

		public override void Layout(XPdfForm inputPdf, string inputPath, string outputPath, PaperTarget paperTarget, bool rightToLeft, bool showCropMarks)
        {
	        if (!showCropMarks && Math.Abs(_bleedMM) < kBleedMicroDeltaMM)
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

				// NB: Setting outputDocument.Settings.TrimMargins.All does not do what we want: it either
				// shrinks or expands the MediaBox/CropBox/BleedBox sizes.  It does not change either
				// TrimBox or ArtBox.

				for (int idx = 1; idx <= _inputPdf.PageCount; idx++)
	            {
		            using (XGraphics gfx = GetGraphicsForNewPage(outputDocument))
		            {
			            DrawPage(gfx, idx);
		            }
	            }

				if (Math.Abs(_bleedMM) > kBleedMicroDeltaMM)
				{
					var tempPath = Path.ChangeExtension(Path.Combine(Path.GetDirectoryName(outputPath),
						Path.GetRandomFileName()), "pdf");
					outputDocument.Save(tempPath);
					outputDocument.Close();
					outputDocument = PdfReader.Open(tempPath, PdfDocumentOpenMode.Import);
					var bleedMargin = new XUnit(_bleedMM, XGraphicsUnit.Millimeter);
					PdfDocument realOutput = new PdfDocument();
					realOutput.PageLayout = PdfPageLayout.SinglePage;
					var trimLocation = new XPoint(bleedMargin.Point, bleedMargin.Point);
					foreach (var page in outputDocument.Pages)
					{
						var trimBox = new PdfRectangle(trimLocation, new XSize(page.MediaBox.Width - 2 * bleedMargin.Point, page.MediaBox.Height - 2 * bleedMargin.Point));
						// All of the boxes start out the same size: MediaBox, CropBox, BleedBox, ArtBox, and TrimBox.
						// MediaBox is presumably the physical paper size.
						// Leave CropBox the same as MediaBox.  Cropbox limits what you see in Adobe Acrobat Reader DC and maybe elsewhere.
						// Also leave BleedBox the same as MediaBox.  See https://i0.wp.com/makingcomics.spiltink.org/wp-content/uploads/2015/05/averageamericancomicsized.jpg.
						page.ArtBox = trimBox;
						page.TrimBox = trimBox;
						realOutput.AddPage(page);
					}
					outputDocument.Close();
					File.Delete(tempPath);
					realOutput.Save(outputPath);
				}
				else
				{
					outputDocument.Save(outputPath);
				}
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
