using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.UseCases.RefreshToken;

public class RefreshTokenUseCase
{
    private readonly ISessaoLoginRepository _sessaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _uow;

    public RefreshTokenUseCase(
        ISessaoLoginRepository sessaoRepository,
        IUsuarioRepository usuarioRepository,
        IJwtService jwtService,
        IUnitOfWork uow)
    {
        _sessaoRepository = sessaoRepository;
        _usuarioRepository = usuarioRepository;
        _jwtService = jwtService;
        _uow = uow;
    }

    public async Task<RefreshTokenOutput> ExecutarAsync(RefreshTokenInput input, CancellationToken ct = default)
    {
        var sessaoAntiga = await _sessaoRepository.ObterPorRefreshTokenAsync(input.RefreshToken, ct);

        if (sessaoAntiga is null || !sessaoAntiga.RefreshTokenValido())
            throw AppException.NaoAutorizado("Refresh token inválido ou expirado.");

        var usuario = await _usuarioRepository.ObterPorIdAsync(sessaoAntiga.UsuarioId, ct);

        if (usuario is null || !usuario.Ativo)
            throw AppException.NaoAutorizado("Usuário inativo ou não encontrado.");

        // Invalida a sessão antiga (rotação de refresh token)
        sessaoAntiga.Invalidar();

        // Gera novos tokens
        var novoToken = _jwtService.GerarToken(usuario, out var expiraEm);
        var novoRefreshToken = _jwtService.GerarRefreshToken();
        var refreshTokenExpiraEm = DateTime.UtcNow.AddDays(365);

        var novaSessao = SessaoLogin.Criar(
            usuario.Id,
            novoToken,
            expiraEm,
            novoRefreshToken,
            refreshTokenExpiraEm);

        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            await _sessaoRepository.SalvarAlteracoesAsync(ct); // persiste invalidação da sessão antiga
            await _sessaoRepository.AdicionarAsync(novaSessao, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }

        return new RefreshTokenOutput(novoToken, expiraEm, novoRefreshToken, refreshTokenExpiraEm);
    }
}
