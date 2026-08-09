using FluentValidation;
using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;

namespace ProvaVida.Application.UseCases.AlterarConta;

public class AlterarContaUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<AlterarContaInput> _validator;

    public AlterarContaUseCase(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork uow,
        IValidator<AlterarContaInput> validator)
    {
        _usuarioRepository = usuarioRepository;
        _uow = uow;
        _validator = validator;
    }

    public async Task ExecutarAsync(AlterarContaInput input, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(input, ct);
        if (!validacao.IsValid)
            throw new ValidationException(validacao.Errors);

        var usuario = await _usuarioRepository.ObterPorIdAsync(input.UsuarioId, ct);
        if (usuario is null || !usuario.Ativo)
            throw AppException.NaoEncontrado("Usuário não encontrado.");

        usuario.AtualizarDados(
            input.Nome,
            input.WhatsApp,
            input.ContatoEmergenciaNome,
            input.ContatoEmergenciaEmail,
            input.ContatoEmergenciaWhatsApp);

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
