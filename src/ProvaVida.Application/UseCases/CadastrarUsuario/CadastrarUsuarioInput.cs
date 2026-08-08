namespace ProvaVida.Application.UseCases.CadastrarUsuario;

public record CadastrarUsuarioInput(
    string Nome,
    string Email,
    string WhatsApp,
    string Senha,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp
);
