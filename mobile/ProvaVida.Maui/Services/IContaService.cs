namespace ProvaVida.Maui.Services;

public record AlterarContaRequest(
    string Nome,
    string WhatsApp,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp);

public record ExcluirContaRequest(string Senha);

public record ContaResponse(
    string Nome,
    string Email,
    string WhatsApp,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp);

public record AlterarSenhaRequest(string SenhaAtual, string NovaSenha);

public interface IContaService
{
    Task<ContaResponse?> ObterPerfilAsync(CancellationToken ct = default);
    Task AlterarAsync(AlterarContaRequest request, CancellationToken ct = default);
    Task AlterarSenhaAsync(AlterarSenhaRequest request, CancellationToken ct = default);
    Task ExcluirAsync(ExcluirContaRequest request, CancellationToken ct = default);
}
