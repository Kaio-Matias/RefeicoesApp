using Camera.MAUI;
using Microsoft.Extensions.Logging;
using RefeicoesApp.Services;
using RefeicoesApp.ViewModels;
using RefeicoesApp.Views;
using Refit;

namespace RefeicoesApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCameraView() 
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddRefitClient<IApiRefeicoes>()
            .ConfigureHttpClient(c =>
            {
       
                c.BaseAddress = new Uri("http://10.1.0.51:8090");
            });

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<CameraViewModel>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<CameraPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}