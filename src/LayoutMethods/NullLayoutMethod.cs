using System;
using System.IO;
using PdfSharp.Pdf;

namespace PdfDroplet
{
    class NullLayoutMethod :LayoutMethod
    {
        public NullLayoutMethod() : base("originalPortrait.png")
        {
        }

        public override void Layout(string inputPath, string outputPath, PaperTarget paperTarget, bool rightToLeft, PdfSharp.Drawing.XPdfForm inputPdf)
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
    }
}