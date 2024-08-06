using System;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using Microsoft.Win32;
using SIL.Reporting;
using PdfDroplet.Properties;

namespace PdfDroplet
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
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
            if (Settings.Default.Reporting == null)
                Settings.Default.Reporting = new ReportingSettings();
            SetupErrorHandling();

            if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Adobe\Acrobat Reader") == null)
            {
                MessageBox.Show(
                    "PdfDroplet requires that your computer be set up to show PDFs in Internet Explorer.  If you have problems, make sure Adobe Reader is installed; re-install it if necessary.");
            }

#if DEBUG
            using (new Analytics("mk41r4rtmyh0ejqtuwaf", new UserInfo(), true))
#else
            string feedbackSetting = System.Environment.GetEnvironmentVariable("FEEDBACK");
		        
			//default is to allow tracking
			var allowTracking = string.IsNullOrEmpty(feedbackSetting) || feedbackSetting.ToLower() == "yes" || feedbackSetting.ToLower() == "true";
            using (new Analytics("mk41r4rtmyh0ejqtuwaf",new UserInfo(), allowTracking))
#endif
            {
                Application.Run(new MainWindow(args.Contains<string>("-about")));
            }

            Settings.Default.Save();
        }

        private static void SetupErrorHandling()
        {
            ErrorReport.EmailAddress = "spam@pdfdroplet.palaso.org".Replace("spam", "issues");
            ErrorReport.AddStandardProperties();

			ExceptionHandler.Init(new SIL.Windows.Forms.Reporting.WinFormsExceptionHandler());
            ExceptionHandler.AddDelegate((w, e) => DesktopAnalytics.Analytics.ReportException(e.Exception));
        }
    }
}
