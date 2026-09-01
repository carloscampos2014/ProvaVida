namespace ProvaVida.Admin.Application.Dtos;

/// <summary>
/// DTO consolidado com todas as métricas do painel Admin.
/// </summary>
/// <remarks>
/// Todos os valores são calculados diretamente no banco (SQL agregado).
/// <see cref="GeradoEm"/> indica o instante UTC em que as métricas foram coletadas.
/// </remarks>
public class MetricasAdminDto
{
    /// <summary>Total de usuários cadastrados.</summary>
    public int TotalUsuarios { get; init; }

    /// <summary>Usuários criados nos últimos 7 dias.</summary>
    public int NovosUltimos7Dias { get; init; }

    /// <summary>Usuários distintos com check-in registrado hoje (UTC).</summary>
    public int UsuariosComCheckinHoje { get; init; }

    /// <summary>Usuários sem check-in nos últimos 2 dias (em alerta).</summary>
    public int UsuariosSemCheckin2Dias { get; init; }

    /// <summary>Instante UTC em que as métricas foram geradas.</summary>
    public DateTimeOffset GeradoEm { get; init; } = DateTimeOffset.UtcNow;
}
