using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Mobile.Infrastructure.Data;

namespace ProvaVida.Mobile.App;

/// <summary>
/// Classe principal do aplicativo. Executa o fluxo de inicialização conforme
/// <c>docs/Fluxo-Inicialização.md</c>: DbUp → logado? → Check-in; internet? → Login; senão → erro + encerrar.
/// </summary>
public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IDatabaseMigrator _migrator;
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<App> _logger;

    /// <summary>
    /// Inicializa o aplicativo com as dependências necessárias para o fluxo de inicialização.
    /// </summary>
    public App(IDatabaseMigrator migrator, ISessionManager sessionManager, ILogger<App> logger)
    {
        InitializeComponent();
        _migrator = migrator;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    /// <inheritdoc/>
    protected override async void OnStart()
    {
        base.OnStart();
        await InicializarAppAsync();
    }

    /// <summary>
    /// Fluxo de inicialização: RF-206.
    /// DbUp → logado? → Check-in; não logado + internet → Login; não logado + sem internet → erro + encerrar.
    /// </summary>
    private async Task InicializarAppAsync()
    {
        // RF-206.1 / RF-206.2 — executar migrations; falha → exibir erro e encerrar
        bool migrationOk;
        try
        {
            migrationOk = await Task.Run(() => _migrator.Migrate());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao executar migrations do SQLite.");
            migrationOk = false;
        }

        if (!migrationOk)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                "Não foi possível criar/atualizar o banco de dados local.",
                "OK");
            Quit();
            return;
        }

        // RF-206.3 — usuário logado → navegar para Check-in
        if (await _sessionManager.IsLoggedInAsync())
        {
            await Shell.Current.GoToAsync("//CheckinPage");
            return;
        }

        // RF-206.4 — não logado + internet → navegar para Login
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        // RF-206.5 — não logado + sem internet → exibir erro e encerrar
        await Shell.Current.DisplayAlertAsync(
            "Sem conexão",
            "Não é possível efetuar login/cadastro sem acesso à internet.",
            "OK");
        Quit();
    }
}