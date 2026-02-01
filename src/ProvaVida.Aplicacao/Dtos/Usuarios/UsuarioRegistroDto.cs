namespace ProvaVida.Aplicacao.Dtos.Usuarios;

/// <summary>
/// DTO para requisição de registro de novo usuário.
/// Entrada: Cliente envia dados para criar conta.
/// 
/// Campos:
/// - Nome: obrigatório (2-150 caracteres)
/// - Email: obrigatório, deve ser único
/// - Telefone: obrigatório (formato: 11 dígitos brasileiro)
/// - Senha: obrigatório (mín 6 caracteres em texto plano)
/// </summary>
public class UsuarioRegistroDto
{
    /// <summary>Nome completo.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Email único para autenticação.</summary>
    public string Email { get; set; } = null!;

    /// <summary>Telefone para contato (WhatsApp).</summary>
    public string Telefone { get; set; } = null!;

    /// <summary>Senha em texto plano (será criptografada no servidor).</summary>
    public string Senha { get; set; } = null!;

    /// <summary>
    /// Validação estrutural básica (sem regras pesadas).
    /// Regras pesadas ficam no Domínio (Usuario.Criar).
    /// </summary>
    public bool EhValido() => 
        !string.IsNullOrWhiteSpace(Nome) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Telefone) &&
        !string.IsNullOrWhiteSpace(Senha) &&
        Senha.Length >= 6;
}
