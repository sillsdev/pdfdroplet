using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
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
            Text = string.Format("PDF Droplet: {0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
        }

    }
}