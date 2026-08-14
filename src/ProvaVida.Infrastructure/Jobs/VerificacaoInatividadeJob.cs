using Hangfire;
using Microsoft.Extensions.Logging;
using ProvaVida.Application.UseCases.VerificarInatividade;

namespace ProvaVida.Infrastructure.Jobs;

/// <summary>
/// Job Hangfire — roda às 23h50 diariamente.
/// Detecta usuários inativos e executa as camadas 1 e 2 (heartbeat + push de aviso).
/// </summary>
public class VerificacaoInatividadeJob
{
    private readonly VerificarInatividadeUseCase _useCase;
    private readonly ILogger<VerificacaoInatividadeJob> _logger;

    public VerificacaoInatividadeJob(
        VerificarInatividadeUseCase useCase,
        ILogger<VerificacaoInatividadeJob> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecutarAsync()
    {
        _logger.LogInformation("Job VerificacaoInatividade iniciado em {Hora}", DateTime.UtcNow);

        try
        {
            await _useCase.ExecutarDeteccaoAsync();
            _logger.LogInformation("Job VerificacaoInatividade concluído com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no job VerificacaoInatividade.");
            throw; // Re-throw para Hangfire registrar a falha e tentar novamente
        }
    }
}
