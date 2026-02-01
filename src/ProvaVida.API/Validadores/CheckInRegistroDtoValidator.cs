using FluentValidation;
using ProvaVida.Aplicacao.Dtos.CheckIns;

namespace ProvaVida.API.Validadores;

/// <summary>
/// Validador para CheckInRegistroDto.
/// </summary>
public class CheckInRegistroDtoValidator : AbstractValidator<CheckInRegistroDto>
{
    public CheckInRegistroDtoValidator()
    {
        RuleFor(c => c.UsuarioId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório.")
            .Must(id => id != Guid.Empty)
            .WithMessage("ID do usuário deve ser um GUID válido.");

        RuleFor(c => c.Localizacao)
            .Must(loc => loc == null || ValidarCoordenadas(loc))
            .WithMessage("Localização deve estar no formato: lat,lon (ex: -23.550519,-46.633309)");
    }

    /// <summary>
    /// Valida se a string contém coordenadas GPS válidas.
    /// </summary>
    private static bool ValidarCoordenadas(string localizacao)
    {
        if (string.IsNullOrWhiteSpace(localizacao))
            return true;

        var partes = localizacao.Split(',');
        if (partes.Length != 2)
            return false;

        return double.TryParse(partes[0], out var lat) &&
               double.TryParse(partes[1], out var lon) &&
               lat >= -90 && lat <= 90 &&
               lon >= -180 && lon <= 180;
    }
}
