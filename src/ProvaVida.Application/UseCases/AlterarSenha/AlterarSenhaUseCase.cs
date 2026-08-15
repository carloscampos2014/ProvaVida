using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;

namespace ProvaVida.Application.UseCases.AlterarSenha;

public class AlterarSenhaUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public AlterarSenhaUseCase(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork uow)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task ExecutarAsync(AlterarSenhaInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.NovaSenha) || input.NovaSenha.Length < 8)
            throw new InvalidOperationException("Nova senha deve ter no mínimo 8 caracteres.");

        var usuario = await _usuarioRepository.ObterPorIdAsync(input.UsuarioId, ct);
        if (usuario is null || !usuario.Ativo)
            throw AppException.NaoEncontrado("Usuário não encontrado.");

        if (!_passwordHasher.Verificar(input.SenhaAtual, usuario.SenhaHash))
            throw AppException.NaoAutorizado("Senha atual incorreta.");

        var novoHash = _passwordHasher.Hash(input.NovaSenha);
        usuario.AlterarSenha(novoHash);

        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            await _usuarioRepository.AtualizarAsync(usuario, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
