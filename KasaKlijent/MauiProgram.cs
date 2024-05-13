using EsirDriver;
using Microsoft.Extensions.Logging;

namespace KasaKlijent
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            FiskalPrevoditeljToEsir fiskalPrevoditeljToEsir = new FiskalPrevoditeljToEsir(new EsirDriver.Modeli.EsirConfigModel(), new EsirDriver.Modeli.PrevoditeljSettingModel() { PathInputFiles="inicijalizacija" });

            builder.Services.AddSingleton<FiskalPrevoditeljToEsir>(fiskalPrevoditeljToEsir);
            
                

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
            
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
