
using EsirDriver;
using FiskalniPrevoditelj.Servisi;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
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

            builder.Services.AddSingleton<FiskalPrevoditeljToEsir>(fiskalPrevoditeljToEsir);
            builder.Services.AddSingleton<Servisi.ConfigSaveServis>();
            builder.Services.AddSingleton<StateServis>();
            builder.Services.AddSingleton<StartupServis>();
            VelopackApp.Build().Run();

            return builder.Build();
        }
    }
}
