using FluentValidation;

namespace ProvaVida.Application.UseCases.CadastrarUsuario;

public class CadastrarUsuarioValidator : AbstractValidator<CadastrarUsuarioInput>
{
    public CadastrarUsuarioValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(300).WithMessage("E-mail deve ter no máximo 300 caracteres.");

        RuleFor(x => x.WhatsApp)
            .NotEmpty().WithMessage("WhatsApp é obrigatório.")
            .MaximumLength(20).WithMessage("WhatsApp deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.")
            .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres.");

        RuleFor(x => x.ContatoEmergenciaNome)
            .NotEmpty().WithMessage("Nome do contato de emergência é obrigatório.")
            .MaximumLength(200).WithMessage("Nome do contato deve ter no máximo 200 caracteres.");

        RuleFor(x => x.ContatoEmergenciaEmail)
            .NotEmpty().WithMessage("E-mail do contato de emergência é obrigatório.")
            .EmailAddress().WithMessage("E-mail do contato de emergência inválido.")
            .MaximumLength(300).WithMessage("E-mail do contato deve ter no máximo 300 caracteres.");

        RuleFor(x => x.ContatoEmergenciaWhatsApp)
            .NotEmpty().WithMessage("WhatsApp do contato de emergência é obrigatório.")
            .MaximumLength(20).WithMessage("WhatsApp do contato deve ter no máximo 20 caracteres.");
    }
}
