namespace ProvaVida.Maui.Services;

public record AlterarContaRequest(
    string Nome,
    string WhatsApp,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp);

public record ExcluirContaRequest(string Senha);

public interface IContaService
{
    Task AlterarAsync(AlterarContaRequest request, CancellationToken ct = default);
    Task ExcluirAsync(ExcluirContaRequest request, CancellationToken ct = default);
}
