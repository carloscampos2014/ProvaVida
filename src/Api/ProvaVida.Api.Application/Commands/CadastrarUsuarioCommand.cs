using ProvaVida.Shared.Dtos;

namespace ProvaVida.Api.Application.Commands;

/// <summary>
/// Comando para cadastro de novo usuário no sistema.
/// </summary>
public record CadastrarUsuarioCommand(
    string Nome,
    string Email,
    string Whatsapp,
    string SenhaHash,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsapp
)
{
    /// <summary>Converte um <see cref="RegisterRequest"/> em <see cref="CadastrarUsuarioCommand"/>.</summary>
    public static CadastrarUsuarioCommand FromRequest(RegisterRequest request) =>
        new(request.Nome, request.Email, request.Whatsapp, request.SenhaHash,
            request.ContatoEmergenciaNome, request.ContatoEmergenciaEmail, request.ContatoEmergenciaWhatsapp);
}
