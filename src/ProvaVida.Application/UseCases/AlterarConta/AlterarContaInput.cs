namespace ProvaVida.Application.UseCases.AlterarConta;

public record AlterarContaInput(
    Guid UsuarioId,
    string Nome,
    string WhatsApp,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp
);
