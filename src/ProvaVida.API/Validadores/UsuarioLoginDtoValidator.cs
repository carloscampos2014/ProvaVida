using FluentValidation;
using ProvaVida.Aplicacao.Dtos.Usuarios;

namespace ProvaVida.API.Validadores;

/// <summary>
/// Validador para UsuarioLoginDto.
/// </summary>
public class UsuarioLoginDtoValidator : AbstractValidator<UsuarioLoginDto>
{
    public UsuarioLoginDtoValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório.")
            .EmailAddress()
            .WithMessage("Email inválido.");

        RuleFor(u => u.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.");
    }
}
