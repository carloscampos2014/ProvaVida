using ProvaVida.Dominio.ObjetosValor;

namespace ProvaVida.Dominio.Entidades;

/// <summary>
/// Entidade de Domínio que representa um contato de emergência.
/// Um contato é uma pessoa que será notificada quando o usuário não fizer check-in dentro de 48 horas.
/// 
/// Invariantes:
/// - Um contato DEVE estar vinculado a um usuário.
/// - Email e WhatsApp são obrigatórios e validados como Value Objects.
/// - A prioridade determina a ordem de disparo das notificações.
/// </summary>
public sealed class ContatoEmergencia
{
    /// <summary>
    /// Identificador único do contato (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID do usuário proprietário deste contato.
    /// </summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>
    /// Nome do contato de emergência.
    /// </summary>
    public string Nome { get; private set; } = null!;

    /// <summary>
    /// Email do contato (Value Object com validação).
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Número de WhatsApp do contato (Value Object com validação).
    /// </summary>
    public Telefone WhatsApp { get; private set; } = null!;

    /// <summary>
    /// Prioridade de disparo (1 = mais importante, será notificado primeiro).
    /// </summary>
    public int Prioridade { get; private set; }

    /// <summary>
    /// Indica se este contato está ativo e deve receber notificações.
    /// </summary>
    public bool Ativo { get; private set; }

    /// <summary>
    /// Data de criação do contato.
    /// </summary>
    public DateTime DataCriacao { get; private set; }

    private ContatoEmergencia() { }

    /// <summary>
    /// Factory para criar um novo contato de emergência com validação.
    /// </summary>
    /// <param name="usuarioId">ID do usuário proprietário.</param>
    /// <param name="nome">Nome do contato.</param>
    /// <param name="email">Email do contato (será validado).</param>
    /// <param name="whatsapp">Telefone com WhatsApp (será validado).</param>
    /// <param name="prioridade">Ordem de disparo (padrão: 1).</param>
    /// <returns>Novo contato de emergência.</returns>
    public static ContatoEmergencia Criar(
        Guid usuarioId,
        string nome,
        string email,
        string whatsapp,
        int prioridade = 1)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("ID do usuário é inválido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do contato não pode ser vazio.", nameof(nome));

        if (prioridade < 1 || prioridade > 10)
            throw new ArgumentException("Prioridade deve estar entre 1 e 10.", nameof(prioridade));

        var contatoEmail = new Email(email);
        var contatoWhatsApp = new Telefone(whatsapp);

        return new ContatoEmergencia
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Nome = nome.Trim(),
            Email = contatoEmail,
            WhatsApp = contatoWhatsApp,
            Prioridade = prioridade,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Desativa este contato de emergência (não receberá mais notificações).
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
    }

    /// <summary>
    /// Reativa este contato de emergência.
    /// </summary>
    public void Reativar()
    {
        Ativo = true;
    }

    /// <summary>
    /// Atualiza a prioridade do contato.
    /// </summary>
    /// <param name="novaPrioridade">Nova prioridade (1-10).</param>
    public void AtualizarPrioridade(int novaPrioridade)
    {
        if (novaPrioridade < 1 || novaPrioridade > 10)
            throw new ArgumentException("Prioridade deve estar entre 1 e 10.", nameof(novaPrioridade));

        Prioridade = novaPrioridade;
    }

    /// <summary>
    /// Verifica se o contato está disponível para receber notificações.
    /// </summary>
    /// <returns>True se contato está ativo.</returns>
    public bool PodeReceberNotificacoes()
    {
        return Ativo;
    }
}
