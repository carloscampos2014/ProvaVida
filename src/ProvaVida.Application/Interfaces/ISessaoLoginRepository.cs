using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Interfaces;

public interface ISessaoLoginRepository
{
    Task<SessaoLogin?> ObterPorTokenAsync(string token, CancellationToken ct = default);
    Task<SessaoLogin?> ObterPorRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<IEnumerable<SessaoLogin>> ListarAtivasPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(SessaoLogin sessao, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
