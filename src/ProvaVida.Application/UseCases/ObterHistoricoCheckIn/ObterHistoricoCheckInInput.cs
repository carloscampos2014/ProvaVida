namespace ProvaVida.Application.UseCases.ObterHistoricoCheckIn;

public record ObterHistoricoCheckInInput(
    Guid UsuarioId,
    DateTimeOffset? DataInicio = null,
    DateTimeOffset? DataFim = null
);
