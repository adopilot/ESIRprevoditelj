
using EsirDriver;
using FiskalniPrevoditelj.Servisi;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using Velopack;

namespace FiskalniPrevoditelj
{
    public static class MauiProgram
    {
        private static Mutex _mutex;
        public static bool HasMainWindow;
        public static MauiApp CreateMauiApp()
        {
            const string appName = "FiskalniPrevoditelj";

            _mutex = new Mutex(true, appName, out bool isNewInstance);
            if (!isNewInstance)
            {
                // If an instance is already running, exit the application
                Environment.Exit(0);
            }

          


            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });







            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddFluentUIComponents();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            FiskalPrevoditeljToEsir fiskalPrevoditeljToEsir = new FiskalPrevoditeljToEsir(new EsirDriver.Modeli.EsirConfigModel(), new EsirDriver.Modeli.PrevoditeljSettingModel() { PathInputFiles = "inicijalizacija" });
            StateServis stateServis = new StateServis();

            builder.Services.AddSingleton<FiskalPrevoditeljToEsir>(fiskalPrevoditeljToEsir);
            builder.Services.AddSingleton<Servisi.ConfigSaveServis>();
            builder.Services.AddSingleton<StateServis>(stateServis);
            builder.Services.AddSingleton<StartupServis>();
            
            
            VelopackApp.Build().Run();

#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                // Make sure to add "using Microsoft.Maui.LifecycleEvents;" in the top of the file
                events.AddWindows(windowsLifecycleBuilder =>
                {
                    windowsLifecycleBuilder.OnWindowCreated(window =>
                    {
                        if (!HasMainWindow)
                        {
                            HasMainWindow = true;
                            window.ExtendsContentIntoTitleBar = false;
                            var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                            var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
                            /* ČKEĆEMO KASU DA OVO KORISIMO KAKO VI JE UVEĆALI I SVE OSTALO POPAILI
                            switch (appWindow.Presenter)
                            {
                                case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                                    //disable the max button
                                    overlappedPresenter.IsMaximizable = true;
                                    overlappedPresenter.IsMinimizable = false;
                                    //overlappedPresenter.Maximize();
                                    //overlappedPresenter.SetBorderAndTitleBar(true, true);
                                    break;

                            }
                            */
                            /*Ovo radi samo je alter mi hocemo 
                            appWindow.Closing += async (s, e) =>
                            {

                                App.Current.Dispatcher.Dispatch(async () => {
                                    var ado = await App.Current.MainPage.DisplayAlert("Alert", "Are you sure you want to close the application.", "Da", "Ne");
                                    if (ado)
                                    {
                                        App.Current?.CloseWindow(Application.Current.MainPage.Window);
                                        App.Current?.Quit();

                                    }
                                });



                                e.Cancel = true;
                            };
                            */
                            appWindow.Closing += async (s, e) =>
                            {
                                
                                App.Current.Dispatcher.Dispatch(async () =>
                                {


                                    
                                    var pitanje = await App.Current.MainPage.DisplayActionSheet("Zatvarate li apliakciju ?", "Odustani od zatvaranja", null, "Da zatvaram", "Ne ostajem", "Samo je smanji minimiziraj");
                                    switch (pitanje)
                                    {
                                        case "Ne ostajem":
                                            return;
                                        case "Samo je smanji minimiziraj":

                                            var Window = App.Current.Windows.First();
                                            var nativeWindow = Window.Handler.PlatformView;
                                            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                                            WindowId WindowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
                                            AppWindow appWindow = AppWindow.GetFromWindowId(WindowId);
                                            var p = appWindow.Presenter as OverlappedPresenter;
                                            p.Minimize();

                                            break;

                                        case "Da zatvaram":
                                            for (int intCounter = App.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
                                            {
                                                App.Current?.CloseWindow(App.Current.Windows[intCounter]);
                                            }

                                            App.Current?.CloseWindow(Application.Current.MainPage.Window);
                                            App.Current?.Quit();
                                            break;
                                        default:
                                            break;

                                    }
                                   
                                }
                                 
                                );
                                e.Cancel = true;
                                    
                            };
                                    

                            
                        };
                    });
                });
            });
#endif


            return builder.Build();
            }
        }
}