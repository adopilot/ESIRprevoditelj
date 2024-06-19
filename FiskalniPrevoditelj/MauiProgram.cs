
using EsirDriver;
using FiskalniPrevoditelj.Servisi;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Velopack;

namespace FiskalniPrevoditelj
{
    public static class MauiProgram
    {
        private static Mutex _mutex;

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
                        //When user execute the closing method, we can make the window do not close by   e.Cancel = true;.
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

                    });
                });
            });
#endif




            /*
            builder.ConfigureLifecycleEvents(lifecycle =>
             {
#if WINDOWS
                 lifecycle.AddWindows(windows =>
                 {
                     windows.OnWindowCreated(window =>
                     {
                         var nativeWindow = window;
                         
                         nativeWindow.AppWindow.Closing += (s, e) =>
                         {
                             var app = App.Current;
                             s.Title = "Kakav si ";
                             s.Show(true);
                             using (var serviceScope = App.Current.Handler.MauiContext.Services.GetService<IServiceScopeFactory>().CreateScope())
                             {
                                 var ado = serviceScope.ServiceProvider.GetService<IDialogService>();

                                 Console.WriteLine("šta si sada gdi esei");

                                 ado.ShowError("Jesi li ti normalan", "Proglem prilikom konfiguracije");
                             }

                             e.Cancel = true;
                         };

                     });
                 });
#endif
             });
            */


            return builder.Build();
        }

       
    }
}
