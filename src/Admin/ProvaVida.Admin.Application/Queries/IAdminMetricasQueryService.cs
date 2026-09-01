using ProvaVida.Admin.Application.Dtos;
using ProvaVida.Shared.Common;

namespace ProvaVida.Admin.Application.Queries;

/// <summary>
/// Serviço de consulta de métricas para o painel Admin.
/// </summary>
/// <remarks>
/// Todas as métricas são calculadas diretamente no banco via SQL agregado —
/// nenhum dado bruto é transferido para processamento em memória.
/// </remarks>
public interface IAdminMetricasQueryService
{
    /// <summary>
    /// Retorna o total de usuários cadastrados.
    /// </summary>
    Task<Result<int>> TotalUsuariosAsync();

    /// <summary>
    /// Retorna a contagem de usuários criados nos últimos <paramref name="dias"/> dias.
    /// </summary>
    /// <param name="dias">Janela de tempo em dias (ex: 7 para última semana).</param>
    Task<Result<int>> NovosUsuariosAsync(int dias);

    /// <summary>
    /// Retorna a contagem de usuários distintos que realizaram check-in hoje (UTC).
    /// </summary>
    Task<Result<int>> UsuariosComCheckinHojeAsync();

    /// <summary>
    /// Retorna a contagem de usuários sem check-in nos últimos <paramref name="dias"/> dias.
    /// </summary>
    /// <param name="dias">Janela de inatividade em dias (ex: 2 para alerta de atraso).</param>
    Task<Result<int>> UsuariosSemCheckinAsync(int dias);

    /// <summary>
    /// Retorna o DTO consolidado com todas as métricas do painel, calculadas em paralelo.
    /// </summary>
    Task<Result<MetricasAdminDto>> MetricasAsync();
}
