namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Credenciais de autenticação do usuário.
/// </summary>
/// <remarks>
/// O <see cref="SenhaHash"/> deve ser o hash SHA-256 da senha, gerado no app mobile antes do envio.
/// A API nunca recebe nem armazena a senha em texto puro.
/// </remarks>
public record LoginRequest(
    string Email,
    string SenhaHash
);
