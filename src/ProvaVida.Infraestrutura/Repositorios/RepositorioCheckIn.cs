using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Contexto;

namespace ProvaVida.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de CheckIns.
/// </summary>
public class RepositorioCheckIn : RepositorioBase<CheckIn>, IRepositorioCheckIn
{
    public RepositorioCheckIn(ProvaVidaDbContext contexto) : base(contexto) { }

    /// <summary>
    /// Obtém os últimos check-ins de um usuário.
    /// </summary>
    public async Task<List<CheckIn>> ObterPorUsuarioAsync(Guid usuarioId, int quantidade = 5, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.DataCheckIn)
            .Take(quantidade)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém check-ins em um intervalo de datas.
    /// </summary>
    public async Task<List<CheckIn>> ObterPorDataAsync(Guid usuarioId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.UsuarioId == usuarioId && c.DataCheckIn >= dataInicio && c.DataCheckIn <= dataFim)
            .OrderBy(c => c.DataCheckIn)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém o último check-in de um usuário.
    /// </summary>
    public async Task<CheckIn?> ObterUltimoAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.DataCheckIn)
            .FirstOrDefaultAsync(cancellationToken);
    }
    /// <summary>
    /// Obtém o check-in mais recente de um usuário.
    /// </summary>
    public async Task<CheckIn?> ObterMaisRecentePorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.DataCheckIn)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém o histórico de check-ins de um usuário com limite.
    /// </summary>
    public async Task<IEnumerable<CheckIn>> ObterHistoricoAsync(Guid usuarioId, int limite = 5, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.DataCheckIn)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Limpa o histórico de check-ins, mantendo apenas os N registros mais recentes.
    /// </summary>
    public async Task LimparHistoricoExcedentesAsync(Guid usuarioId, int limiteRegistros = 5, CancellationToken cancellationToken = default)
    {
        var checkInsExcedentes = await DbSet
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.DataCheckIn)
            .Skip(limiteRegistros)
            .ToListAsync(cancellationToken);

        if (checkInsExcedentes.Count > 0)
        {
            DbSet.RemoveRange(checkInsExcedentes);
            await Contexto.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Remove um check-in pelo ID.
    /// </summary>
    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var checkIn = await ObterPorIdAsync(id, cancellationToken);
        if (checkIn != null)
        {
            await base.RemoverAsync(id, cancellationToken);
        }
    }}
