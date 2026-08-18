namespace ProvaVida.Application.UseCases.ObterMetricasAdmin;

public class EventoNotificacaoDto
{
    public Guid Id { get; init; }
    public string NomeUsuario { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Canal { get; init; } = string.Empty;
    public DateTime DataDisparo { get; init; }
    public DateTime? JanelaExpiraEm { get; init; }
}
