using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.UI.Xaml.Controls;

namespace KasaKlijent
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

#if WINDOWS
            var webView2 = (this.blazorWebView.Handler.PlatformView as WebView2);
            await webView2.EnsureCoreWebView2Async();
            var settings = webView2.CoreWebView2.Settings;
            settings.AreDevToolsEnabled = true;
            settings.IsZoomControlEnabled = false;
            settings.IsGeneralAutofillEnabled = false;
            settings.AreDefaultContextMenusEnabled = false;
            settings.IsPasswordAutosaveEnabled = false;
            settings.IsStatusBarEnabled = true;
            var userAgent = settings.UserAgent;
            Console.WriteLine(userAgent);
#if DEBUG
            settings.AreBrowserAcceleratorKeysEnabled = true;

#else
                settings.AreBrowserAcceleratorKeysEnabled = false;

#endif

#endif
        }
    }

    }
