using FluentValidation;
using ProvaVida.Shared.Dtos;

namespace ProvaVida.Mobile.Application.Validators;

/// <summary>
/// Validador FluentValidation para o <see cref="RegisterRequest"/> enviado pelo app mobile.
/// </summary>
/// <remarks>
/// Valida os dados do formulário de cadastro após a geração do hash SHA-256 da senha.
/// As regras espelham as do servidor (<c>CadastrarUsuarioValidator</c> na API) para dar
/// feedback imediato ao usuário sem uma roundtrip desnecessária.
/// </remarks>
public class CadastrarUsuarioMobileValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>Inicializa as regras de validação do cadastro mobile.</summary>
    public CadastrarUsuarioMobileValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(254).WithMessage("E-mail deve ter no máximo 254 caracteres.");

        RuleFor(x => x.Whatsapp)
            .NotEmpty().WithMessage("WhatsApp é obrigatório.")
            .Matches(@"^\d{10,11}$").WithMessage("WhatsApp deve conter 10 ou 11 dígitos numéricos.");

        RuleFor(x => x.SenhaHash)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .Length(64).WithMessage("Hash da senha inválido. Verifique o preenchimento da senha.");

        RuleFor(x => x.ContatoEmergenciaNome)
            .NotEmpty().WithMessage("Nome do contato de emergência é obrigatório.")
            .MaximumLength(200).WithMessage("Nome do contato deve ter no máximo 200 caracteres.");

        RuleFor(x => x.ContatoEmergenciaEmail)
            .NotEmpty().WithMessage("E-mail do contato de emergência é obrigatório.")
            .EmailAddress().WithMessage("E-mail do contato de emergência inválido.")
            .MaximumLength(254).WithMessage("E-mail do contato deve ter no máximo 254 caracteres.");

        RuleFor(x => x.ContatoEmergenciaWhatsapp)
            .NotEmpty().WithMessage("WhatsApp do contato de emergência é obrigatório.")
            .Matches(@"^\d{10,11}$").WithMessage("WhatsApp do contato deve conter 10 ou 11 dígitos numéricos.");
    }
}
