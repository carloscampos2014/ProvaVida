using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Contexto;

namespace ProvaVida.Infraestrutura.Repositorios;

/// <summary>
/// Implementação concreta do repositório de usuários.
/// Persiste e recupera dados usando Entity Framework Core.
/// 
/// Estende RepositorioBase<Usuario> para herdar operações CRUD comuns,
/// adicionando métodos específicos para Usuario (buscar por email, etc).
/// </summary>
public class RepositorioUsuario : RepositorioBase<Usuario>, IRepositorioUsuario
{
    public RepositorioUsuario(ProvaVidaDbContext contexto)
        : base(contexto)
    {
    }

    /// <summary>
    /// Obtém um usuário pelo email (com includes para relações).
    /// </summary>
    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await DbSet
            .Include(u => u.HistoricoCheckIns)
            .Include(u => u.Contatos)
            .FirstOrDefaultAsync(u => u.Email.Valor == email, cancellationToken);
    }

    /// <summary>
    /// Obtém um usuário pelo ID com todas as relações carregadas (implementa IRepositorioUsuario).
    /// </summary>
    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.HistoricoCheckIns)
            .Include(u => u.Contatos)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Obtém um usuário pelo ID (override da base para includes de relações).
    /// </summary>
    public override async Task<Usuario?> ObterPorIdAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id == null)
            return null;

        var usuarioId = (Guid)id;
        return await ObterPorIdAsync(usuarioId, cancellationToken);
    }

    /// <summary>
    /// Obtém todos os usuários com relações.
    /// </summary>
    public override async Task<List<Usuario>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.HistoricoCheckIns)
            .Include(u => u.Contatos)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca usuários vencidos (sem check-in há mais de 48h).
    /// </summary>
    public async Task<List<Usuario>> ObterUsuariosVencidosAsync(CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;

        return await DbSet
            .Where(u => u.DataProximoVencimento < agora)
            .Include(u => u.Contatos)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca usuários que devem receber lembrete (próximo à data de vencimento).
    /// </summary>
    public async Task<List<Usuario>> ObterUsuariosComLembreteAsync(int horasAntes, CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        var dataLembrete = agora.AddHours(horasAntes);

        return await DbSet
            .Where(u => u.DataProximoVencimento <= dataLembrete && u.DataProximoVencimento > agora)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Atualiza um usuário existente.
    /// </summary>
    public override async Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        if (usuario == null) throw new ArgumentNullException(nameof(usuario));
        await base.AtualizarAsync(usuario, cancellationToken);
    }

    /// <summary>
    /// Remove um usuário pelo ID.
    /// </summary>
    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await base.RemoverAsync(id, cancellationToken);
    }
}
