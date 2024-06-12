using Microsoft.UI.Xaml.Controls;

namespace FiskalniPrevoditelj
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

      
            private async void MainPage_Loaded(object sender, EventArgs e)
            {
#if DEBUG
            Console.WriteLine("debug");
#else
            var webView2 = (this.blazorWebView.Handler.PlatformView as WebView2);
            await webView2.EnsureCoreWebView2Async();
            var settings = webView2.CoreWebView2.Settings;
            settings.AreDevToolsEnabled = true;
            settings.IsZoomControlEnabled = false;
            settings.IsGeneralAutofillEnabled = false;
            settings.AreDefaultContextMenusEnabled = false;
            settings.IsPasswordAutosaveEnabled = false;
            settings.IsStatusBarEnabled = false;
            var userAgent = settings.UserAgent;
            settings.AreBrowserAcceleratorKeysEnabled = false;
#endif



        }
    }
}
