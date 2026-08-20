using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.UseCases.Login;

public class LoginUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISessaoLoginRepository _sessaoRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _uow;
    private readonly IRefreshTokenHasher _hasher;

    public LoginUseCase(
        IUsuarioRepository usuarioRepository,
        ISessaoLoginRepository sessaoRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork uow,
        IRefreshTokenHasher hasher)
    {
        _usuarioRepository = usuarioRepository;
        _sessaoRepository = sessaoRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<LoginOutput> ExecutarAsync(LoginInput input, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(input.Email, ct);

        if (usuario is null || !usuario.Ativo)
            throw AppException.NaoAutorizado("Credenciais inválidas.");

        var senhaValida = _passwordHasher.Verificar(input.Senha, usuario.SenhaHash);
        if (!senhaValida)
            throw AppException.NaoAutorizado("Credenciais inválidas.");

        var token = _jwtService.GerarToken(usuario, out var expiraEm);
        var refreshToken = _jwtService.GerarRefreshToken();
        var refreshTokenExpiraEm = DateTime.UtcNow.AddDays(365);
        var refreshTokenHash = _hasher.Hash(refreshToken);

        var sessao = SessaoLogin.Criar(usuario.Id, token, expiraEm, refreshTokenHash, refreshTokenExpiraEm);

        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            await _sessaoRepository.AdicionarAsync(sessao, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }

        return new LoginOutput(token, expiraEm, refreshToken, refreshTokenExpiraEm);
    }
}
