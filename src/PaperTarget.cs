using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PdfDroplet
{
    public class PaperTarget
    {
        public readonly int Width;
        public readonly int Height;
        public string Name;

        public PaperTarget(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
        }

        public virtual Point GetOutputDimensions(int inputWidth, int intputHeight)
        {
            return new Point(Width, Height);
        }


        public override string ToString()
        {
            return Name;
        }
    }

    class DoublePaperTarget : PaperTarget
    {
        public DoublePaperTarget()
            : base(StaticName, 0,0)
        {
            
        }
        public override Point GetOutputDimensions(int inputWidth, int inputHeight)
        {
            return new Point(inputWidth * 2, inputHeight);
        }
        public const string StaticName = @"PerservePage";//this is tied to use settings, so don't change it.
    }

    class SameSizePaperTarget : PaperTarget
    {
        public SameSizePaperTarget()
            : base(StaticName, 0, 0)
        {

        }

        public const string StaticName = @"ShrinkPage";//this is tied to use settings, so don't change it.

        public override Point GetOutputDimensions(int inputWidth, int inputHeight)
        {
            return new Point(inputHeight, inputWidth);
        }
    }
}
