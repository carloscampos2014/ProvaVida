using Hangfire;
using Microsoft.Extensions.Logging;
using ProvaVida.Application.UseCases.VerificarInatividade;

namespace ProvaVida.Infrastructure.Jobs;

/// <summary>
/// Job Hangfire — roda a cada hora.
/// Verifica janelas de graça expiradas e dispara e-mail + WhatsApp ao contato de emergência.
/// </summary>
public class DispararAlertaJob
{
    private readonly VerificarInatividadeUseCase _useCase;
    private readonly ILogger<DispararAlertaJob> _logger;

    public DispararAlertaJob(
        VerificarInatividadeUseCase useCase,
        ILogger<DispararAlertaJob> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task ExecutarAsync()
    {
        _logger.LogInformation("Job DispararAlerta iniciado em {Hora}", DateTime.UtcNow);

        try
        {
            await _useCase.ExecutarDisparoAsync();
            _logger.LogInformation("Job DispararAlerta concluído.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no job DispararAlerta.");
            throw;
        }
    }
}
