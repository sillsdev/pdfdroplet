using System;
using System.Drawing;
using DotImpose.LayoutMethods;
using SIL.IO;

namespace PdfDroplet
{
    /// <summary>
    /// Extension methods for DotImpose LayoutMethod classes
    /// </summary>
    internal static class LayoutMethodExtensions
    {
        /// <summary>
        /// Get the thumbnail filename for a layout method based on its ID and orientation
        /// </summary>
        public static string GetThumbnailFilename(this LayoutMethod method, bool isLandscape)
        {
            var layoutId = method.Id;
            
            // Map layout method IDs to their image filenames
            // These should match the image files in the browser/assets/images directory
            switch (layoutId)
            {
                case "original":
                    return isLandscape ? "originalLandscape.png" : "originalPortrait.png";
                case "sideFoldBooklet":
                    return "sideFoldBooklet.png";
                case "calendar":
                    return "calendar.png";
                case "cutBooklet":
                    return "cutBooklet.png";
                case "sideFoldCut4UpBooklet":
                    return "sideFoldCut4UpBooklet.png";
                case "sideFoldCut4UpSingleBooklet":
                    return "sideFoldCut4UpSingleBooklet.png";
                case "folded8Up8PageBooklet":
                    return "folded8Up8PageBooklet.png";
                case "square6UpBooklet":
                    return "square6UpBooklet.png";
                default:
                    return "originalPortrait.png"; // fallback
            }
        }
    }
}
