namespace ProvaVida.Application.UseCases.ObterHistoricoCheckIn;

public record ObterHistoricoCheckInInput(
    Guid UsuarioId,
    DateTime? DataInicio = null,
    DateTime? DataFim = null
);
