using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Enums;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Contexto;

namespace ProvaVida.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de Notificações.
/// </summary>
public class RepositorioNotificacao : RepositorioBase<Notificacao>, IRepositorioNotificacao
{
    public RepositorioNotificacao(ProvaVidaDbContext contexto) : base(contexto) { }

    /// <summary>
    /// Obtém uma notificação pelo ID (implementa IRepositorioNotificacao).
    /// </summary>
    public async Task<Notificacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    /// <summary>
    /// Obtém as notificações de um usuário.
    /// </summary>
    public async Task<List<Notificacao>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém as notificações pendentes de um contato de emergência.
    /// </summary>
    public async Task<List<Notificacao>> ObterPorContatoAsync(Guid contatoId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.ContatoEmergenciaId == contatoId)
            .OrderByDescending(n => n.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém notificações que devem ser reenviadas (emergência com data próximo reenvio vencida).
    /// </summary>
    public async Task<List<Notificacao>> ObterParaReenvioAsync(CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;

        return await DbSet
            .Where(n => n.TipoNotificacao == TipoNotificacao.EmergenciaContatos
                && n.Status == StatusNotificacao.Enviada
                && n.DataProximoReenvio != null
                && n.DataProximoReenvio <= agora
                && (agora - n.DataCriacao).TotalHours <= 48)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Conta notificações não enviadas de um usuário.
    /// </summary>
    public async Task<int> ContarPendentesAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.UsuarioId == usuarioId && n.Status == StatusNotificacao.Pendente)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém todas as notificações de um usuário.
    /// </summary>
    public async Task<IEnumerable<Notificacao>> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém notificações de um contato específico.
    /// </summary>
    public async Task<IEnumerable<Notificacao>> ObterPorContatoIdAsync(Guid contatoEmergenciaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.ContatoEmergenciaId == contatoEmergenciaId)
            .OrderByDescending(n => n.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Conta quantas notificações de emergência um contato possui.
    /// </summary>
    public async Task<int> ContarNotificacoesEmergenciaAsync(Guid contatoEmergenciaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.ContatoEmergenciaId == contatoEmergenciaId && n.TipoNotificacao == TipoNotificacao.EmergenciaContatos)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Limpa o histórico de notificações, mantendo apenas os N registros mais recentes.
    /// </summary>
    public async Task LimparHistoricoExcedentesAsync(Guid contatoEmergenciaId, int limiteRegistros = 5, CancellationToken cancellationToken = default)
    {
        var notificacoesExcedentes = await DbSet
            .Where(n => n.ContatoEmergenciaId == contatoEmergenciaId)
            .OrderByDescending(n => n.DataCriacao)
            .Skip(limiteRegistros)
            .ToListAsync(cancellationToken);

        if (notificacoesExcedentes.Count > 0)
        {
            DbSet.RemoveRange(notificacoesExcedentes);
            await Contexto.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Atualiza uma notificação existente.
    /// </summary>
    public override async Task AtualizarAsync(Notificacao notificacao, CancellationToken cancellationToken = default)
    {
        if (notificacao == null) throw new ArgumentNullException(nameof(notificacao));
        await base.AtualizarAsync(notificacao, cancellationToken);
    }

    /// <summary>
    /// Remove uma notificação pelo ID.
    /// </summary>
    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await base.RemoverAsync(id, cancellationToken);
    }
}
