using FluentValidation;
using ProvaVida.Api.Application.Commands;

namespace ProvaVida.Api.Application.Validators;

/// <summary>
/// Validador FluentValidation para o comando de cadastro de usuário.
/// </summary>
/// <remarks>
/// Valida campos obrigatórios, formato de e-mail, formato de telefone (WhatsApp com 10-11 dígitos)
/// e comprimento mínimo do hash SHA-256 (64 caracteres hexadecimais).
/// </remarks>
public class CadastrarUsuarioValidator : AbstractValidator<CadastrarUsuarioCommand>
{
    /// <summary>Inicializa as regras de validação.</summary>
    public CadastrarUsuarioValidator()
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
            .NotEmpty().WithMessage("SenhaHash é obrigatório.")
            .Length(64).WithMessage("SenhaHash deve ter exatamente 64 caracteres (SHA-256 hex).");

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
