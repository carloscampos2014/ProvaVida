using FluentValidation;
using ProvaVida.Aplicacao.Dtos.Usuarios;

namespace ProvaVida.API.Validadores;

/// <summary>
/// Validador para UsuarioRegistroDto.
/// 
/// Validações:
/// - Nome: 2-150 caracteres
/// - Email: formato válido
/// - Telefone: celular brasileiro (11 dígitos com 9º dígito = 9)
/// - Senha: força mínima (8 caracteres, maiúscula, minúscula, número, especial)
/// - ContatoEmergencia: obrigatório com dados válidos
/// </summary>
public class UsuarioRegistroDtoValidator : AbstractValidator<UsuarioRegistroDto>
{
    public UsuarioRegistroDtoValidator()
    {
        // Nome
        RuleFor(u => u.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .Length(2, 150)
            .WithMessage("Nome deve ter entre 2 e 150 caracteres.");

        // Email
        RuleFor(u => u.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório.")
            .EmailAddress()
            .WithMessage("Email inválido.");

        // Telefone - CELULAR BRASILEIRO: (11) 98765-4321 ou 11987654321
        RuleFor(u => u.Telefone)
            .NotEmpty()
            .WithMessage("Telefone é obrigatório.")
            .Matches(@"^(\(?\d{2}\)?\s?)9\d{8}$|^\d{2}9\d{8}$")
            .WithMessage("Telefone deve ser um celular brasileiro válido. Exemplo: 11987654321 ou (11) 98765-4321");

        // Senha - FORÇA MÍNIMA
        RuleFor(u => u.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.")
            .MinimumLength(8)
            .WithMessage("Senha deve ter no mínimo 8 caracteres.")
            .Matches(@"[A-Z]")
            .WithMessage("Senha deve conter pelo menos 1 letra MAIÚSCULA.")
            .Matches(@"[a-z]")
            .WithMessage("Senha deve conter pelo menos 1 letra minúscula.")
            .Matches(@"\d")
            .WithMessage("Senha deve conter pelo menos 1 número.")
            .Matches(@"[!@#$%^&*()_+\-=\[\]{};':""\|,.<>\/?]")
            .WithMessage("Senha deve conter pelo menos 1 caractere especial (!@#$%^&*()...) .");

        // Contato de Emergência - OBRIGATÓRIO
        RuleFor(u => u.ContatoEmergencia)
            .NotNull()
            .WithMessage("Contato de emergência é obrigatório para registro.")
            .DependentRules(() =>
            {
                RuleFor(u => u.ContatoEmergencia.Nome)
                    .NotEmpty()
                    .WithMessage("Nome do contato é obrigatório.")
                    .Length(2, 150)
                    .WithMessage("Nome do contato deve ter entre 2 e 150 caracteres.");

                RuleFor(u => u.ContatoEmergencia.Email)
                    .NotEmpty()
                    .WithMessage("Email do contato é obrigatório.")
                    .EmailAddress()
                    .WithMessage("Email do contato inválido.");

                RuleFor(u => u.ContatoEmergencia.WhatsApp)
                    .NotEmpty()
                    .WithMessage("WhatsApp do contato é obrigatório.")
                    .Matches(@"^(\(?\d{2}\)?\s?)9\d{8}$|^\d{2}9\d{8}$")
                    .WithMessage("WhatsApp do contato deve ser um celular brasileiro válido.");

                RuleFor(u => u.ContatoEmergencia.Prioridade)
                    .InclusiveBetween(1, 10)
                    .WithMessage("Prioridade deve estar entre 1 e 10.")
                    .When(u => u.ContatoEmergencia.Prioridade.HasValue);
            });
    }
}
