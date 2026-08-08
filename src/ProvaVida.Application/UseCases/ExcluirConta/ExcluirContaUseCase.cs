using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;

namespace ProvaVida.Application.UseCases.ExcluirConta;

public class ExcluirContaUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public ExcluirContaUseCase(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork uow)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task ExecutarAsync(ExcluirContaInput input, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(input.UsuarioId, ct);
        if (usuario is null || !usuario.Ativo)
            throw AppException.NaoEncontrado("Usuário não encontrado.");

        var senhaValida = _passwordHasher.Verificar(input.Senha, usuario.SenhaHash);
        if (!senhaValida)
            throw AppException.NaoAutorizado("Senha incorreta.");

        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            var anonId = input.UsuarioId.ToString("N")[..8];
            await _usuarioRepository.AnonimizarAsync(
                input.UsuarioId,
                $"[removido-{anonId}]",
                $"removido-{anonId}@anonimizado.invalid",
                ct);
            await _usuarioRepository.InvalidarSessoesAsync(input.UsuarioId, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
