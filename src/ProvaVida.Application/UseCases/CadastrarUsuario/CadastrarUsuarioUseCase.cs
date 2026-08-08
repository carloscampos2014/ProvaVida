using FluentValidation;
using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.UseCases.CadastrarUsuario;

public class CadastrarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CadastrarUsuarioInput> _validator;

    public CadastrarUsuarioUseCase(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork uow,
        IValidator<CadastrarUsuarioInput> validator)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _uow = uow;
        _validator = validator;
    }

    public async Task<Guid> ExecutarAsync(CadastrarUsuarioInput input, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(input, ct);
        if (!validacao.IsValid)
            throw new ValidationException(validacao.Errors);

        var emailExiste = await _usuarioRepository.EmailExisteAsync(input.Email, ct);
        if (emailExiste)
            throw AppException.Conflito("E-mail já cadastrado.");

        var senhaHash = _passwordHasher.Hash(input.Senha);

        var usuario = Usuario.Criar(
            input.Nome,
            input.Email,
            input.WhatsApp,
            senhaHash,
            input.ContatoEmergenciaNome,
            input.ContatoEmergenciaEmail,
            input.ContatoEmergenciaWhatsApp);

        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            await _usuarioRepository.AdicionarAsync(usuario, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }

        return usuario.Id;
    }
}
