using System;
using System.Drawing;
using Palaso.IO;
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
        
        
        protected int PagesPerTwoSidedSheet {get;set;}

        protected LayoutMethod(string imageName)
        {
            PagesPerTwoSidedSheet = 4;
            _imageName = imageName;
            DoRotate = true;
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
            _outputWidth = paperTarget.GetPaperDimensions(_inputPdf.PixelWidth, _inputPdf.PixelHeight, DoRotate).X;
            _outputHeight = paperTarget.GetPaperDimensions(_inputPdf.PixelWidth, _inputPdf.PixelHeight, DoRotate).Y;


            int inputPages = _inputPdf.PageCount;
            int numberOfSheetsOfPaper = inputPages / PagesPerTwoSidedSheet;
            if (numberOfSheetsOfPaper * PagesPerTwoSidedSheet < inputPages)
                numberOfSheetsOfPaper += 1;
            int numberOfPageSlotsAvailable = PagesPerTwoSidedSheet * numberOfSheetsOfPaper;
            int vacats = numberOfPageSlotsAvailable - inputPages;

            LayoutInner(outputDocument, numberOfSheetsOfPaper, numberOfPageSlotsAvailable, vacats);

            outputDocument.Save(outputPath);
        }

        protected bool DoRotate{ get; set;}


        protected abstract void LayoutInner(PdfDocument outputDocument, int numberOfSheetsOfPaper, int numberOfPageSlotsAvailable, int vacats);



        protected XGraphics GetGraphicsForNewPage(PdfDocument outputDocument)
        {

            XGraphics gfx;
            PdfPage page = outputDocument.AddPage();
            //page.Orientation = PageOrientation.Landscape;//review: why does this say it's always landscape (and why does that work?) Or maybe it has no effect?
            page.Width = _outputWidth;
            page.Height = _outputHeight;

            gfx = XGraphics.FromPdfPage(page);
            return gfx;
        }


        public abstract bool GetIsEnabled(bool isLandscape);

        public virtual Image GetImage(bool isLandscape)
        {
            return Image.FromFile(FileLocator.GetFileDistributedWithApplication("images", _imageName));
        }
    }
}