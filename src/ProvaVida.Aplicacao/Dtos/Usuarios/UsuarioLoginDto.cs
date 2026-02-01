namespace ProvaVida.Aplicacao.Dtos.Usuarios;

/// <summary>
/// DTO para requisição de login.
/// Entrada: email + senha em texto plano.
/// </summary>
public class UsuarioLoginDto
{
    /// <summary>Email do usuário.</summary>
    public string Email { get; set; } = null!;

    /// <summary>Senha em texto plano.</summary>
    public string Senha { get; set; } = null!;
}
