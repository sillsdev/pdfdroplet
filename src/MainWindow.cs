using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Palaso.IO;
using Palaso.Reporting;
using PdfDroplet.Properties;


namespace PdfDroplet
{
    public partial class MainWindow : Form
    {
        public MainWindow(bool showAbout)
        {
            InitializeComponent();
            Font = SystemFonts.DialogFont;

             SetWindowText();
            timer1.Enabled = showAbout;
            //_instructionsBrowser.Navigate(FileLocator.GetFileDistributedWithApplication("instructions.htm"));
            //_browserForPdf.Navigated +=new WebBrowserNavigatedEventHandler((x,y)=>_stillNavigating = false);


        }

      

        private void SetWindowText()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Text = string.Format("Pdf Droplet: {0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
        }

    }
}