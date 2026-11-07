using FluentValidation;
using ProvaVida.Mobile.Application.Auth;

namespace ProvaVida.Mobile.Application.Validators;

/// <summary>
/// Validador FluentValidation para o <see cref="LoginMobileRequest"/> do app mobile.
/// </summary>
/// <remarks>
/// Valida o e-mail e a senha em texto puro <em>antes</em> da geração do hash SHA-256,
/// garantindo feedback imediato ao usuário sem custos de computação desnecessários.
/// </remarks>
public class LoginMobileValidator : AbstractValidator<LoginMobileRequest>
{
    /// <summary>Inicializa as regras de validação do login mobile.</summary>
    public LoginMobileValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres.");
    }
}
