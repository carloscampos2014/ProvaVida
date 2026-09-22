using FluentValidation;
using ProvaVida.Api.Application.Commands;

namespace ProvaVida.Api.Application.Validators;

/// <summary>
/// Validador FluentValidation para o comando de login.
/// </summary>
public class LoginValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Inicializa as regras de validação.</summary>
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.SenhaHash)
            .NotEmpty().WithMessage("SenhaHash é obrigatório.")
            .Length(64).WithMessage("SenhaHash deve ter exatamente 64 caracteres (SHA-256 hex).");
    }
}
