using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Mobile.Application.Http;
using ProvaVida.Mobile.Application.Navigation;
using ProvaVida.Mobile.Application.Services;
using ProvaVida.Mobile.Infrastructure.Data;
using ProvaVida.Mobile.Infrastructure.Http;
using ProvaVida.Mobile.Infrastructure.Repositories;
using ProvaVida.Mobile.App.Navigation;
using ProvaVida.Mobile.App.Pages;
using ProvaVida.Mobile.App.Services;
using ProvaVida.Mobile.App.ViewModels;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.App;

/// <summary>
/// Ponto de entrada do aplicativo MAUI. Configura o contêiner DI.
/// As migrations são executadas em <see cref="App.InicializarAppAsync"/> (OnStart),
/// onde falhas podem ser exibidas ao usuário antes de encerrar.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Cria e configura o <see cref="MauiApp"/>, registrando todos os serviços necessários.
    /// </summary>
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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ── Banco de dados SQLite ─────────────────────────────────────────────

        // Caminho do banco SQLite no diretório de dados do app
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "provavida.db");

        // Migrator: singleton pois controla schema único
        // Migrations executadas em App.OnStart para tratar falhas com mensagem ao usuário (RF-206.2)
        builder.Services.AddSingleton<IDatabaseMigrator>(sp =>
            new DatabaseMigrator(dbPath, sp.GetRequiredService<ILogger<DatabaseMigrator>>()));

        // Fábrica de conexões: singleton — compartilha o caminho do banco
        builder.Services.AddSingleton<IDbConnectionFactory>(
            new SqliteConnectionFactory(dbPath));

        // ── Repositórios SQLite ───────────────────────────────────────────────

        // Transient — cada operação abre/fecha sua própria conexão via factory
        builder.Services.AddTransient<IUsuarioRepository, SqliteUsuarioRepository>();
        builder.Services.AddTransient<ICheckinRepository, SqliteCheckinRepository>();
        builder.Services.AddTransient<ITokenRepository, SqliteTokenRepository>();

        // ── HTTP / API ────────────────────────────────────────────────────────

        // Base URL configurável por ambiente (RNF-206)
        // Development Windows: porta 5268 (launchSettings.json perfil http)
        // Development WSL2: porta 5001 (serviço no Linux acessível via localhost)
        // Produção: URL da VM Oracle via appsettings.json
#if DEBUG
        var apiBaseUrl = "http://localhost:5268/";
#else
        var apiBaseUrl = "http://localhost:5001/";
#endif
        builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ── Navegação ─────────────────────────────────────────────────────────

        // IAppNavigation: permite que TokenManager navegue para Login sem depender do MAUI diretamente
        builder.Services.AddSingleton<IAppNavigation, MauiAppNavigation>();

        // ── Serviços de autenticação ──────────────────────────────────────────

        builder.Services.AddTransient<ISessionManager, SessionManager>();
        builder.Services.AddTransient<ITokenManager, TokenManager>();
        builder.Services.AddTransient<IAuthMobileService, AuthMobileService>();

        // ── Serviços de device / conectividade / localização ─────────────────

        // Singletons: wrappers sobre APIs estáticas do MAUI Essentials (RF-303, RF-304)
        builder.Services.AddSingleton<ILocationService, MauiLocationService>();
        builder.Services.AddSingleton<IConnectivityService, MauiConnectivityService>();
        builder.Services.AddSingleton<IDeviceInfoService, MauiDeviceInfoService>();

        // ── Serviços de check-in ──────────────────────────────────────────────

        // Transient: sem estado entre requests (RF-302, RF-303, RF-304)
        builder.Services.AddTransient<ICheckinMobileService, CheckinMobileService>();

        // ── ViewModels e Pages ────────────────────────────────────────────────

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CadastroViewModel>();
        builder.Services.AddTransient<CadastroPage>();
        builder.Services.AddTransient<CheckinViewModel>();
        builder.Services.AddTransient<CheckinPage>();

        return builder.Build();
    }
}
