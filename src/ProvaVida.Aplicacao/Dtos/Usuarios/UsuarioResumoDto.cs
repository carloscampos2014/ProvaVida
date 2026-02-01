using ProvaVida.Dominio.Enums;

namespace ProvaVida.Aplicacao.Dtos.Usuarios;

/// <summary>
/// DTO para resposta de usuário (saída do servidor).
/// 
/// Segurança:
/// - ❌ Nunca inclui SenhaHash
/// - ❌ Nunca inclui detalhes do Telefone
/// - ✅ Apenas dados públicos e necessários para o cliente
/// </summary>
public class UsuarioResumoDto
{
    /// <summary>ID único.</summary>
    public Guid Id { get; set; }

    /// <summary>Nome completo.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Email (já validado no servidor).</summary>
    public string Email { get; set; } = null!;

    /// <summary>Status atual (Ativo, EmAtraso, AlertaCritico, Inativo).</summary>
    public StatusUsuario Status { get; set; }

    /// <summary>Próxima data de vencimento do check-in.</summary>
    public DateTime DataProximoVencimento { get; set; }

    /// <summary>Quantidade de contatos de emergência registrados.</summary>
    public int QuantidadeContatos { get; set; }

    /// <summary>Data de criação da conta.</summary>
    public DateTime DataCriacao { get; set; }
}
