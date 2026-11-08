namespace ProvaVida.Api.Application.Commands;

/// <summary>Command para criar um novo check-in diário.</summary>
public record CriarCheckinCommand(
    Guid UsuarioId,
    DateOnly Data,
    double Latitude,
    double Longitude,
    string IdentificacaoAparelho
);
