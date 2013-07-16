using System;
using System.Drawing;
using System.IO;
using Palaso.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet.LayoutMethods
{
    public class NullLayoutMethod :LayoutMethod
    {
        public NullLayoutMethod() : base("")
        {
        }

		public override void Layout(XPdfForm inputPdf, string inputPath, string outputPath, PaperTarget paperTarget, bool rightToLeft, bool commercialPrinting)
        {
            File.Copy(inputPath, outputPath,true);
        }
		protected override void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats)
        {
            throw new NotImplementedException();
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