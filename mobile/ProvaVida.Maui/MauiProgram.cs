using Microsoft.Extensions.Logging;
using ProvaVida.Maui.Pages;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;
using ProvaVida.Maui.ViewModels;
namespace ProvaVida.Maui;

public static class MauiProgram
{
    // URL base da API — usar 10.0.2.2 para emulador Android apontar para localhost do Windows
#if DEBUG
    private const string ApiBaseUrl = "http://localhost:5182/";
#else
    private const string ApiBaseUrl = "https://provida-api.enzojb.com.br/";
#endif

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // HttpClient
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(ApiBaseUrl)
        });

        // Storage
        builder.Services.AddSingleton<ITokenStorage, TokenStorage>();
        builder.Services.AddSingleton<IUsuarioStorage, UsuarioStorage>();
        builder.Services.AddSingleton<LocalDatabase>();

        // Services
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IContaService, ContaService>();
        builder.Services.AddSingleton<ICheckInService, CheckInService>();
        builder.Services.AddSingleton<IHeartbeatService, HeartbeatService>();
        builder.Services.AddSingleton<LocationService>();
        builder.Services.AddSingleton<SyncService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CadastroViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();
        builder.Services.AddTransient<CheckInViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CadastroPage>();
        builder.Services.AddTransient<PerfilPage>();
        builder.Services.AddTransient<CheckInPage>();
        builder.Services.AddTransient<LoadingPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
