namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Dados necessários para cadastro de um novo usuário no sistema.
/// </summary>
public record RegisterRequest(
    string Nome,
    string Email,
    string Whatsapp,
    string SenhaHash,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsapp
);
