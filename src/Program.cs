using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Palaso.CommandLineProcessing;
using Palaso.Reporting;
using PdfDroplet.Properties;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SetupErrorHandling(); 
            Application.Run(new MainWindow(args.Contains<string>("-about")));
            Settings.Default.Save();
        }

        private static void SetupErrorHandling()
        {
            ErrorReport.EmailAddress = "hide@gmail.org".Replace("hide","hattonjohn");
            ErrorReport.AddStandardProperties();
            ExceptionHandler.Init();

            UsageReporter.ReportLaunchesAsync();
        }
    }

  
}
