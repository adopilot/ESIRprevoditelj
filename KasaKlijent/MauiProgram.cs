﻿using EsirDriver;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;

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
            builder.Services.AddSingleton<FiskalPrevoditeljToEsir>();


#if DEBUG
            builder.Services.AddFluentUIComponents();
            builder.Services.AddBlazorWebViewDeveloperTools();            
            builder.Logging.AddDebug();

#endif
            builder.Services.AddFluentUIComponents();
            return builder.Build();
        }
    }
}
