namespace ProvaVida.Aplicacao.Dtos.Usuarios;

/// <summary>
/// DTO para requisição de registro de novo usuário.
/// Entrada: Cliente envia dados para criar conta.
/// 
/// REGRA DE NEGÓCIO: Obrigatório registrar pelo menos 1 contato de emergência
/// no mesmo fluxo de registro (não é possível ter usuário sem contato).
/// 
/// Campos:
/// - Nome: obrigatório (2-150 caracteres)
/// - Email: obrigatório, deve ser único
/// - Telefone: obrigatório (formato: 11 dígitos brasileiro celular)
/// - Senha: obrigatório (mín 8 caracteres, com requisitos de força)
/// - ContatoEmergencia: obrigatório (pelo menos um contato)
/// </summary>
public class UsuarioRegistroDto
{
    /// <summary>Nome completo.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Email único para autenticação.</summary>
    public string Email { get; set; } = null!;

    /// <summary>Telefone para contato (WhatsApp) - celular brasileiro: 11-9XXXXXX.</summary>
    public string Telefone { get; set; } = null!;

    /// <summary>Senha em texto plano (será criptografada no servidor).
    /// Requisitos: mín 8 caracteres, 1 maiúscula, 1 minúscula, 1 número, 1 especial.</summary>
    public string Senha { get; set; } = null!;

    /// <summary>
    /// Contato de emergência obrigatório no registro.
    /// PREMISSA: Usuário não pode ter conta sem pelo menos 1 contato.
    /// </summary>
    public ContatoEmergenciaNovoDto ContatoEmergencia { get; set; } = null!;

    /// <summary>
    /// Validação estrutural básica (sem regras pesadas).
    /// Regras pesadas ficam no Domínio (Usuario.Criar).
    /// </summary>
    public bool EhValido() => 
        !string.IsNullOrWhiteSpace(Nome) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Telefone) &&
        !string.IsNullOrWhiteSpace(Senha) &&
        Senha.Length >= 8 &&
        ContatoEmergencia != null;
}

/// <summary>
/// DTO aninhado para contato de emergência no registro de usuário.
/// </summary>
public class ContatoEmergenciaNovoDto
{
    /// <summary>Nome do contato.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Email do contato.</summary>
    public string Email { get; set; } = null!;

    /// <summary>WhatsApp do contato - celular brasileiro.</summary>
    public string WhatsApp { get; set; } = null!;

    /// <summary>Prioridade de notificação (1-10).</summary>
    public int? Prioridade { get; set; } = 1;
}
