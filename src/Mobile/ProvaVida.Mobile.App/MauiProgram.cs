using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Infrastructure.Data;

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

        // Registrar DatabaseMigrator no DI
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "provavida.db");
        builder.Services.AddSingleton<IDatabaseMigrator>(sp =>
            new DatabaseMigrator(dbPath, sp.GetRequiredService<ILogger<DatabaseMigrator>>()));

        var app = builder.Build();

        // Executar migrations antes de subir o app
        var migrator = app.Services.GetRequiredService<IDatabaseMigrator>();
        migrator.Migrate();

        return app;
    }
}
