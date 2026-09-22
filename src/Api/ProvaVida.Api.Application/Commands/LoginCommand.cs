using ProvaVida.Shared.Dtos;

namespace ProvaVida.Api.Application.Commands;

/// <summary>
/// Comando para autenticação de usuário existente.
/// </summary>
public record LoginCommand(string Email, string SenhaHash)
{
    /// <summary>Converte um <see cref="LoginRequest"/> em <see cref="LoginCommand"/>.</summary>
    public static LoginCommand FromRequest(LoginRequest request) =>
        new(request.Email, request.SenhaHash);
}
