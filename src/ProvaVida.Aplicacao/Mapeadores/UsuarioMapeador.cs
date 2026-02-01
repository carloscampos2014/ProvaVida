using ProvaVida.Aplicacao.Dtos.Usuarios;
using ProvaVida.Dominio.Entidades;

namespace ProvaVida.Aplicacao.Mapeadores;

/// <summary>
/// Mapeador manual entre Usuario (Entidade) e DTOs.
/// 
/// PADRÃO: Métodos de extensão estáticos para legibilidade fluente.
/// EXEMPLO: usuario.ParaResumoDto() ou dto.ParaDominio(senhaHashBcrypt)
/// 
/// PRINCÍPIOS:
/// - Controle total sobre cada campo (sem "magic" de AutoMapper)
/// - Segurança: SenhaHash NUNCA é exposto
/// - Validações: Nulos são verificados explicitamente
/// - Factories: Sempre usam Usuario.Criar() para respeitar regras de negócio
/// </summary>
public static class UsuarioMapeador
{
    /// <summary>
    /// Mapeia Usuario (Entidade do Domínio) para UsuarioResumoDto (resposta HTTP).
    /// 
    /// ✅ INCLUÍDO: Id, Nome, Email, Status, DataProximoVencimento, QuantidadeContatos, DataCriacao
    /// ❌ EXCLUÍDO: SenhaHash (NUNCA expõe), Telefone (privacidade), Histórico detalhado
    /// 
    /// Segurança:
    /// - Cria um novo DTO vazio
    /// - Copia apenas dados públicos
    /// - Value Objects são convertidos para string (Email.Valor)
    /// </summary>
    /// <param name="usuario">Entidade do Domínio (não nula).</param>
    /// <returns>DTO de resposta seguro para HTTP.</returns>
    /// <exception cref="ArgumentNullException">Se usuario é nulo.</exception>
    public static UsuarioResumoDto ParaResumoDto(this Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario), "Usuário não pode ser nulo ao mapear para DTO.");

        return new UsuarioResumoDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email.Valor,  // Value Object → string primitivo
            Status = usuario.Status,
            DataProximoVencimento = usuario.DataProximoVencimento,
            QuantidadeContatos = usuario.Contatos.Count,
            DataCriacao = usuario.DataCriacao
            // ❌ Jamais: SenhaHash, histórico completo, telefone
        };
    }

    /// <summary>
    /// Mapeia UsuarioRegistroDto (entrada HTTP) para Usuario (Entidade do Domínio).
    /// 
    /// FLUXO:
    /// 1. Valida se DTO é nulo
    /// 2. Chama Usuario.Criar() (Factory do Domínio)
    ///    → Factory valida: nome, email, telefone, senha
    ///    → Lança UsuarioInvalidoException se inválido
    /// 3. Retorna Entidade pronta para persistência
    /// 
    /// ⚠️ IMPORTANTE: senha no DTO deve estar em texto plano (virá do HTTP).
    /// O Service de Aplicação será responsável por:
    ///    → Criptografar com BCrypt
    ///    → Passar senhaHash para este método
    /// </summary>
    /// <param name="dto">Dados de registro (entrada).</param>
    /// <param name="senhaHashBcrypt">Senha já criptografada com BCrypt (12 rounds).</param>
    /// <returns>Entidade Usuario pronta para persistência.</returns>
    /// <exception cref="ArgumentNullException">Se DTO é nulo.</exception>
    /// <exception cref="Dominio.Exceções.UsuarioInvalidoException">Se Factory rejeita dados.</exception>
    public static Usuario ParaDominio(this UsuarioRegistroDto dto, string senhaHashBcrypt)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO de registro não pode ser nulo.");

        if (string.IsNullOrWhiteSpace(senhaHashBcrypt))
            throw new ArgumentException("Senha criptografada é obrigatória.", nameof(senhaHashBcrypt));

        // Factory do Domínio faz as validações pesadas
        // Pode lançar UsuarioInvalidoException
        return Usuario.Criar(
            nome: dto.Nome,
            email: dto.Email,
            telefone: dto.Telefone,
            senhaHash: senhaHashBcrypt  // ✅ Já criptografada
        );
    }

    /// <summary>
    /// Converte UsuarioLoginDto para verificação de credenciais.
    /// (Mapeamento trivial - apenas para padronização)
    /// </summary>
    public static (string Email, string Senha) ParaTuple(this UsuarioLoginDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        return (dto.Email, dto.Senha);
    }
}
