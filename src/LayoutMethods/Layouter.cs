using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Palaso.IO;
using PdfDroplet.Properties;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfDroplet
{
    public abstract class LayoutMethod
    {
        private readonly string _imageName;
        protected XUnit _outputWidth;
        protected XUnit _outputHeight;
        protected XPdfForm _inputPdf;
        protected bool _rightToLeft;
        //protected bool _landscapeOriginal=false;
        protected bool _calendarMode;

        protected LayoutMethod(string imageName)
        {
            _imageName = imageName;
        }

        public virtual bool ImageIsSensitiveToOrientation
        {
            get { return false; }
        }


        public virtual void Layout(string inputPath, string outputPath, PaperTarget paperTarget, bool rightToLeft, XPdfForm inputPdf)
        {
            _rightToLeft = rightToLeft;
            _inputPdf = inputPdf;

            PdfDocument outputDocument = new PdfDocument();

            // Show single pages
            // (Note: one page contains two pages from the source document.
            //  If the number of pages of the source document can not be
            //  divided by 4, the first pages of the output document will
            //  each contain only one page from the source document.)
            outputDocument.PageLayout = PdfPageLayout.SinglePage;

            // Determine width and height
            _outputWidth = paperTarget.GetPaperDimensions(_inputPdf.PixelWidth, _inputPdf.PixelHeight).X;
            _outputHeight = paperTarget.GetPaperDimensions(_inputPdf.PixelWidth, _inputPdf.PixelHeight).Y;


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
            page.Width = _outputWidth;
            page.Height = _outputHeight;

            gfx = XGraphics.FromPdfPage(page);

            if (Settings.Default.Mirror)
            {
                Matrix mirrorMatrix = new Matrix(-1, 0, 0, 1, 0, 0);
                gfx.MultiplyTransform(mirrorMatrix);
                gfx.TranslateTransform(-_outputWidth, 1);
            }

            return gfx;
        }


        public abstract bool GetIsEnabled(bool isLandscape);

        public virtual Image GetImage(bool isLandscape)
        {
            return Image.FromFile(FileLocator.GetFileDistributedWithApplication("images", _imageName));
        }
    }
}