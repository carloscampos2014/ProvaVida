using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Contexto;

namespace ProvaVida.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de Contatos de Emergência.
/// </summary>
public class RepositorioContatoEmergencia : RepositorioBase<ContatoEmergencia>, IRepositorioContatoEmergencia
{
    public RepositorioContatoEmergencia(ProvaVidaDbContext contexto) : base(contexto) { }

    /// <summary>
    /// Obtém os contatos de emergência de um usuário.
    /// </summary>
    public async Task<List<ContatoEmergencia>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém todos os contatos de um usuário (implementa IRepositorioContatoEmergencia).
    /// </summary>
    public async Task<IEnumerable<ContatoEmergencia>> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await ObterPorUsuarioAsync(usuarioId, cancellationToken);
    }

    /// <summary>
    /// Obtém um contato específico pelo email (busca global, sem filtrar por usuário).
    /// Útil para validar se um email já está registrado como contato em qualquer usuário.
    /// </summary>
    public async Task<ContatoEmergencia?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await DbSet
            .FirstOrDefaultAsync(c => c.Email.Valor == email, cancellationToken);
    }

    /// <summary>
    /// Obtém um contato específico pelo email dentro de um usuário.
    /// </summary>
    public async Task<ContatoEmergencia?> ObterPorEmailAsync(Guid usuarioId, string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await DbSet
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Email.Valor == email, cancellationToken);
    }

    /// <summary>
    /// Conta quantos contatos um usuário tem.
    /// </summary>
    public async Task<int> ContarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.UsuarioId == usuarioId)
            .CountAsync(cancellationToken);
    }
    /// <summary>
    /// Obtém um contato pelo ID.
    /// </summary>
    public async Task<ContatoEmergencia?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Atualiza um contato existente.
    /// </summary>
    public override async Task AtualizarAsync(ContatoEmergencia contato, CancellationToken cancellationToken = default)
    {
        if (contato == null) throw new ArgumentNullException(nameof(contato));
        await base.AtualizarAsync(contato, cancellationToken);
    }

    /// <summary>
    /// Remove um contato pelo ID.
    /// </summary>
    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await base.RemoverAsync(id, cancellationToken);
    }}
