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



            //bring in settings from any previous version
            if (Settings.Default.NeedUpgrade)
            {
                //see http://stackoverflow.com/questions/3498561/net-applicationsettingsbase-should-i-call-upgrade-every-time-i-load
                Settings.Default.Upgrade();
                Settings.Default.NeedUpgrade = false;
                Settings.Default.Save();
            }
            if(Settings.Default.Reporting == null)
                Settings.Default.Reporting = new ReportingSettings();
            SetupErrorHandling(); 
            SetupUsageTracking();
            Application.Run(new MainWindow(args.Contains<string>("-about")));
            Settings.Default.Save();
        }

        private static void SetupErrorHandling()
        {
            ErrorReport.EmailAddress = "hide@gmail.org".Replace("hide","hattonjohn");
            ErrorReport.AddStandardProperties();
            ExceptionHandler.Init();
        }

        private static void SetupUsageTracking()
        {
            UsageReporter.Init(Settings.Default.Reporting,"pdfdroplet.palaso.org", "UA-22170471-5");
        }

        public static UsageReporter Usage { get; set; }
    }

  
}
