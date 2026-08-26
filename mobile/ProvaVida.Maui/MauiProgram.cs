using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ProvaVida.Maui.Infrastructure;
using ProvaVida.Maui.Pages;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;
using ProvaVida.Maui.ViewModels;
namespace ProvaVida.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
#if ANDROID || IOS || MACCATALYST
            .UseLocalNotification()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configuração por ambiente via appsettings.json / appsettings.Debug.json
        using var appsettingsStream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
        builder.Configuration.AddJsonStream(appsettingsStream);

#if DEBUG
        try
        {
            // appsettings.Debug.json é opcional — sobrescreve ApiBaseUrl para desenvolvimento local
            using var debugStream = FileSystem.OpenAppPackageFileAsync("appsettings.Debug.json").GetAwaiter().GetResult();
            builder.Configuration.AddJsonStream(debugStream);
        }
        catch { /* arquivo não incluído no build — usa configuração de appsettings.json */ }
#endif

        var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://provida-api.enzojb.com.br/";

        // HttpClient
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl)
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

        // Logging — arquivo em produção para diagnóstico, Debug em desenvolvimento
        // FileLoggerProvider é implementação própria sem reflection — compatível com AOT Android
        var logPath = Path.Combine(FileSystem.AppDataDirectory,
            $"provavida-{DateTime.Now:yyyyMMdd}.log");
        builder.Logging
            .SetMinimumLevel(LogLevel.Information)
            .AddProvider(new FileLoggerProvider(logPath, LogLevel.Warning));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
