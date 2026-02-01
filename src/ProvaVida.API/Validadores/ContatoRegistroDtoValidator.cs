using FluentValidation;
using ProvaVida.Aplicacao.Dtos.ContatosEmergencia;

namespace ProvaVida.API.Validadores;

/// <summary>
/// Validador para ContatoRegistroDto.
/// </summary>
public class ContatoRegistroDtoValidator : AbstractValidator<ContatoRegistroDto>
{
    public ContatoRegistroDtoValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .Length(2, 150)
            .WithMessage("Nome deve ter entre 2 e 150 caracteres.");

        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório.")
            .EmailAddress()
            .WithMessage("Email inválido.");

        RuleFor(c => c.WhatsApp)
            .NotEmpty()
            .WithMessage("WhatsApp é obrigatório.")
            .Matches(@"^\d{10,15}$")
            .WithMessage("WhatsApp deve conter 10-15 dígitos (sem formatação).");

        RuleFor(c => c.Prioridade)
            .InclusiveBetween(1, 10)
            .WithMessage("Prioridade deve estar entre 1 e 10.")
            .When(c => c.Prioridade.HasValue);
    }
}
