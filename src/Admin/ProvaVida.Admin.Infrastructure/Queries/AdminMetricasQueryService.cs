using Dapper;
using ProvaVida.Admin.Application.Dtos;
using ProvaVida.Admin.Application.Queries;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Admin.Infrastructure.Queries;

/// <summary>
/// Implementação do serviço de métricas do painel Admin.
/// </summary>
/// <remarks>
/// Todas as queries executam SQL agregado diretamente no PostgreSQL —
/// nenhum dado bruto é carregado em memória para processamento.
/// </remarks>
public class AdminMetricasQueryService : IAdminMetricasQueryService
{
    private readonly IDbConnectionFactory _factory;

    /// <summary>
    /// Inicializa o serviço com a fábrica de conexões PostgreSQL do Admin.
    /// </summary>
    /// <param name="factory">Fábrica de conexões PostgreSQL.</param>
    public AdminMetricasQueryService(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public virtual async Task<Result<int>> TotalUsuariosAsync()
    {
        try
        {
            using var conn = _factory.Create();
            var total = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*)::int FROM usuarios");
            return Result<int>.Ok(total);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex.Message);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Result<int>> NovosUsuariosAsync(int dias)
    {
        try
        {
            using var conn = _factory.Create();
            var total = await conn.ExecuteScalarAsync<int>(
                """
                SELECT COUNT(*)::int
                FROM usuarios
                WHERE criado_em >= NOW() - (@dias || ' days')::INTERVAL
                """,
                new { dias });
            return Result<int>.Ok(total);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex.Message);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Result<int>> UsuariosComCheckinHojeAsync()
    {
        try
        {
            using var conn = _factory.Create();
            var total = await conn.ExecuteScalarAsync<int>(
                """
                SELECT COUNT(DISTINCT usuario_id)::int
                FROM checkins
                WHERE data = CURRENT_DATE
                """);
            return Result<int>.Ok(total);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex.Message);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Result<int>> UsuariosSemCheckinAsync(int dias)
    {
        try
        {
            using var conn = _factory.Create();
            var total = await conn.ExecuteScalarAsync<int>(
                """
                SELECT COUNT(*)::int
                FROM usuarios u
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM checkins c
                    WHERE c.usuario_id = u.id
                      AND c.data >= CURRENT_DATE - (@dias || ' days')::INTERVAL
                )
                """,
                new { dias });
            return Result<int>.Ok(total);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex.Message);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Result<MetricasAdminDto>> MetricasAsync()
    {
        try
        {
            var totalTask          = TotalUsuariosAsync();
            var novosTask          = NovosUsuariosAsync(7);
            var checkinHojeTask    = UsuariosComCheckinHojeAsync();
            var semCheckinTask     = UsuariosSemCheckinAsync(2);

            await Task.WhenAll(totalTask, novosTask, checkinHojeTask, semCheckinTask);

            if (!totalTask.Result.Success)       return Result<MetricasAdminDto>.Fail(totalTask.Result.MessageErro!);
            if (!novosTask.Result.Success)        return Result<MetricasAdminDto>.Fail(novosTask.Result.MessageErro!);
            if (!checkinHojeTask.Result.Success)  return Result<MetricasAdminDto>.Fail(checkinHojeTask.Result.MessageErro!);
            if (!semCheckinTask.Result.Success)   return Result<MetricasAdminDto>.Fail(semCheckinTask.Result.MessageErro!);

            var dto = new MetricasAdminDto
            {
                TotalUsuarios          = totalTask.Result.Data,
                NovosUltimos7Dias      = novosTask.Result.Data,
                UsuariosComCheckinHoje = checkinHojeTask.Result.Data,
                UsuariosSemCheckin2Dias = semCheckinTask.Result.Data,
                GeradoEm               = DateTimeOffset.UtcNow
            };

            return Result<MetricasAdminDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return Result<MetricasAdminDto>.Fail(ex.Message);
        }
    }
}
