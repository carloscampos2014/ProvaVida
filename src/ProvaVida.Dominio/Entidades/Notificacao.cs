using ProvaVida.Dominio.Enums;

namespace ProvaVida.Dominio.Entidades;

/// <summary>
/// Entidade de Domínio que representa uma notificação (alerta ou lembrete).
/// 
/// Tipos:
/// - LembreteUsuario: Notificações simples para o próprio usuário (4h antes + no vencimento).
/// - EmergenciaContatos: Notificações para contatos de emergência (reenvios a cada 6h por 48h).
/// 
/// Invariantes:
/// - O histórico de notificações é limitado a 5 registros por contato (FIFO)
/// - Uma notificação passa pelos estados: Pendente → Enviada ou Pendente → Erro
/// - DataProximoReenvio é calculada apenas para EmergenciaContatos (6h após cada envio)
/// - Notificações de emergência param após 48h desde o primeiro envio
/// </summary>
public sealed class Notificacao
{
    /// <summary>
    /// Identificador único da notificação (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID do contato de emergência destinatário (Guid.Empty para lembretes do próprio usuário).
    /// </summary>
    public Guid ContatoEmergenciaId { get; private set; }

    /// <summary>
    /// ID do usuário que gerou esta notificação.
    /// </summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>
    /// Tipo de notificação (LembreteUsuario ou EmergenciaContatos).
    /// </summary>
    public TipoNotificacao TipoNotificacao { get; private set; }

    /// <summary>
    /// Meio de envio (Email, WhatsApp).
    /// </summary>
    public MeioNotificacao MeioNotificacao { get; private set; }

    /// <summary>
    /// Data e hora quando a notificação foi criada.
    /// </summary>
    public DateTime DataCriacao { get; private set; }

    /// <summary>
    /// Status atual da notificação (Pendente, Enviada, Erro, Cancelada).
    /// </summary>
    public StatusNotificacao Status { get; private set; }

    /// <summary>
    /// Data e hora do próximo reenvio (APENAS para EmergenciaContatos).
    /// Calculada como DataEnvio + 6 horas a cada reenvio.
    /// Null para LembreteUsuario.
    /// </summary>
    public DateTime? DataProximoReenvio { get; private set; }

    /// <summary>
    /// Mensagem de erro, se houver.
    /// </summary>
    public string? MensagemErro { get; private set; }

    private Notificacao() { }

    /// <summary>
    /// Factory para criar uma notificação de LEMBRETE para o PRÓPRIO USUÁRIO.
    /// Disparado 4 horas antes do vencimento ou no momento do vencimento.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="meioNotificacao">Meio de envio (Email).</param>
    /// <returns>Nova notificação de lembrete no estado Pendente.</returns>
    public static Notificacao CriarLembreteUsuario(
        Guid usuarioId,
        MeioNotificacao meioNotificacao)
    {
        return new Notificacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            ContatoEmergenciaId = Guid.Empty,  // Sem contato de emergência
            TipoNotificacao = TipoNotificacao.LembreteUsuario,
            MeioNotificacao = meioNotificacao,
            DataCriacao = DateTime.UtcNow,
            Status = StatusNotificacao.Pendente,
            DataProximoReenvio = null,  // Lembretes não são reenviados
            MensagemErro = null
        };
    }

    /// <summary>
    /// Factory para criar uma notificação de EMERGÊNCIA para CONTATOS.
    /// Disparado quando o usuário não faz check-in por 24h.
    /// Reenvios automáticos a cada 6 horas por até 48 horas.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="contatoEmergenciaId">ID do contato de emergência a notificar.</param>
    /// <param name="meioNotificacao">Meio de envio (Email, WhatsApp).</param>
    /// <returns>Nova notificação de emergência no estado Pendente.</returns>
    public static Notificacao CriarEmergenciaContatos(
        Guid usuarioId,
        Guid contatoEmergenciaId,
        MeioNotificacao meioNotificacao)
    {
        if (contatoEmergenciaId == Guid.Empty)
            throw new ArgumentException("ContatoEmergenciaId não pode ser vazio.", nameof(contatoEmergenciaId));

        return new Notificacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            ContatoEmergenciaId = contatoEmergenciaId,
            TipoNotificacao = TipoNotificacao.EmergenciaContatos,
            MeioNotificacao = meioNotificacao,
            DataCriacao = DateTime.UtcNow,
            Status = StatusNotificacao.Pendente,
            DataProximoReenvio = null,  // Será calculado ao marcar como enviada
            MensagemErro = null
        };
    }

    /// <summary>
    /// Marca a notificação como enviada com sucesso.
    /// Para EmergenciaContatos, agenda o próximo reenvio para 6 horas depois.
    /// </summary>
    public void MarcarComoEnviada()
    {
        Status = StatusNotificacao.Enviada;
        MensagemErro = null;

        // Se é emergência, agenda próximo reenvio
        if (TipoNotificacao == TipoNotificacao.EmergenciaContatos)
        {
            DataProximoReenvio = DateTime.UtcNow.AddHours(6);
        }
    }

    /// <summary>
    /// Marca a notificação como tendo falhado no envio.
    /// </summary>
    /// <param name="mensagem">Descrição do erro ocorrido.</param>
    public void MarcarComErro(string mensagem)
    {
        Status = StatusNotificacao.Erro;
        MensagemErro = mensagem;
    }

    /// <summary>
    /// Cancela a notificação (ex: quando o usuário faz check-in antes do envio).
    /// </summary>
    public void Cancelar()
    {
        Status = StatusNotificacao.Cancelada;
        DataProximoReenvio = null;
    }

    /// <summary>
    /// Verifica se a notificação de EMERGÊNCIA deve ser reenviada.
    /// Retorna true se:
    /// - É uma EmergenciaContatos
    /// - Status é Enviada
    /// - Passou o horário agendado (DataProximoReenvio)
    /// - Não ultrapassou 48 horas desde a criação
    /// </summary>
    /// <returns>True se deve ser reenviada.</returns>
    public bool DeveSerReenviada()
    {
        // Só aplica a emergências
        if (TipoNotificacao != TipoNotificacao.EmergenciaContatos)
            return false;

        // Deve estar enviada
        if (Status != StatusNotificacao.Enviada)
            return false;

        // Deve ter DataProximoReenvio agendada
        if (!DataProximoReenvio.HasValue)
            return false;

        // Deve ter passado o horário agendado
        if (DateTime.UtcNow < DataProximoReenvio.Value)
            return false;

        // Não deve ter ultrapassado 48 horas desde a criação
        var horasDesdeOrigem = (DateTime.UtcNow - DataCriacao).TotalHours;
        return horasDesdeOrigem <= 48;
    }

    /// <summary>
    /// Calcula quantas horas se passaram desde a criação da notificação.
    /// </summary>
    /// <returns>Número de horas decorridas.</returns>
    public double HorasDesciasCriacao()
    {
        return (DateTime.UtcNow - DataCriacao).TotalHours;
    }

    /// <summary>
    /// Verifica se a notificação de emergência já atingiu o limite de 48 horas.
    /// </summary>
    /// <returns>True se passou mais de 48 horas desde a criação.</returns>
    public bool Ultrapassa48Horas()
    {
        return HorasDesciasCriacao() > 48;
    }
}
