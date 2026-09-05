using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Infrastructure.Data;
using ProvaVida.Mobile.Infrastructure.Repositories;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.App;

/// <summary>
/// Ponto de entrada do aplicativo MAUI. Configura o DI e executa as migrations do SQLite.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Cria e configura o <see cref="MauiApp"/>, registrando serviços e executando migrations.
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

        // Caminho do banco SQLite no diretório de dados do app
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "provavida.db");

        // Migrator: singleton pois controla schema único
        builder.Services.AddSingleton<IDatabaseMigrator>(sp =>
            new DatabaseMigrator(dbPath, sp.GetRequiredService<ILogger<DatabaseMigrator>>()));

        // Fábrica de conexões: singleton — compartilha o caminho do banco
        builder.Services.AddSingleton<IDbConnectionFactory>(
            new SqliteConnectionFactory(dbPath));

        // Repositórios: transient — cada operação abre/fecha sua própria conexão via factory
        builder.Services.AddTransient<IUsuarioRepository, SqliteUsuarioRepository>();
        builder.Services.AddTransient<ICheckinRepository, SqliteCheckinRepository>();

        // Tela inicial
        builder.Services.AddTransient<MainPage>();

        var app = builder.Build();

        // Executar migrations antes de subir o app
        var migrator = app.Services.GetRequiredService<IDatabaseMigrator>();
        migrator.Migrate();

        return app;
    }
}
