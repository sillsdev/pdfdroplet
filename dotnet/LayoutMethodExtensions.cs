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
                    return isLandscape ? "originalLandscape.svg" : "originalPortrait.svg";
                case "sideFoldBooklet":
                    return "sideFoldBooklet.svg";
                case "calendar":
                    return "calendar.svg";
                case "cutBooklet":
                    return "cutBooklet.svg";
                case "sideFoldCut4UpBooklet":
                    return "sideFoldCut4UpBooklet.svg";
                case "sideFoldCut4UpSingleBooklet":
                    return "sideFoldCut4UpSingleBooklet.svg";
                case "folded8Up8PageBooklet":
                    return "folded8Up8PageBooklet.svg";
                case "square6UpBooklet":
                    return "square6UpBooklet.svg";
                default:
                    return "error"; // fallback
            }
        }
    }
}
