using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExisteAsync(string email, CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken ct = default);
    Task AnonimizarAsync(Guid id, string nomeSubstituto, string emailSubstituto, CancellationToken ct = default);
    Task InvalidarSessoesAsync(Guid usuarioId, CancellationToken ct = default);
    // Mantido para compatibilidade com o padrão — no Dapper o commit é via IUnitOfWork
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
