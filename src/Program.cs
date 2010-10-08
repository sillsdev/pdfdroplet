using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Palaso.Reporting;

namespace PdfDroplet
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            SetupErrorHandling();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow(args.Contains<string>("-about")));
        }

        private static void SetupErrorHandling()
        {
            ErrorReport.EmailAddress = "issues@wesay.org";
            ErrorReport.AddStandardProperties();
            ExceptionHandler.Init();
        }
    }
}
