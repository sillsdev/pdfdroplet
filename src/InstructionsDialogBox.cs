using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using SIL.IO;

namespace PdfDroplet
{
    partial class InstructionsDialogBox : Form
    {
        public InstructionsDialogBox()
        {
            InitializeComponent();

            InitializeWebView2Async();
        }

        private async void InitializeWebView2Async()
        {
            try
            {
                // Set user data folder to a writable location in AppData
                var userDataFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PdfDroplet", "WebView2");

                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await _browser.EnsureCoreWebView2Async(environment);
                _browser.CoreWebView2.Navigate(FileLocationUtilities.GetFileDistributedWithApplication("instructions.htm"));

                // After first navigation, intercept further navigation to open in external browser
                _browser.CoreWebView2.NavigationStarting += OnNavigationStarting;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView2: {ex.Message}",
                    "WebView2 Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool _firstNavigation = true;

        /// <summary>
        /// forward any link-clicks to their main browser, not this window (except first navigation)
        /// </summary>
        private void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (_firstNavigation)
            {
                _firstNavigation = false;
                return;
            }

            e.Cancel = true;
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
