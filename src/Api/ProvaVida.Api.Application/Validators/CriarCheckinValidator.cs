using FluentValidation;
using ProvaVida.Api.Application.Commands;

namespace ProvaVida.Api.Application.Validators;

/// <summary>
/// Validador FluentValidation para o comando de criação de check-in.
/// </summary>
/// <remarks>
/// Valida campos obrigatórios, coordenadas geográficas dentro dos limites globais
/// e tamanho máximo da identificação do aparelho.
/// </remarks>
public class CriarCheckinValidator : AbstractValidator<CriarCheckinCommand>
{
    /// <summary>Inicializa as regras de validação.</summary>
    public CriarCheckinValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("UsuarioId é obrigatório.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Data é obrigatória.")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data não pode ser futura.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude deve estar entre -90 e 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude deve estar entre -180 e 180.");

        RuleFor(x => x.IdentificacaoAparelho)
            .NotEmpty().WithMessage("IdentificacaoAparelho é obrigatória.")
            .MaximumLength(200).WithMessage("IdentificacaoAparelho deve ter no máximo 200 caracteres.");
    }
}
