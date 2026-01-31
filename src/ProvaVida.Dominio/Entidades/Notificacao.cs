using ProvaVida.Dominio.Enums;

namespace ProvaVida.Dominio.Entidades;

/// <summary>
/// Entidade de Domínio que representa uma notificação (alerta ou lembrete).
/// Notificações podem ser: lembretes (-6h, -2h) ou emergências (pós-vencimento).
/// 
/// Invariantes:
/// - O histórico de notificações é limitado a 5 registros por contato (FIFO)
/// - Uma notificação passa pelos estados: Pendente → Enviada ou Pendente → Erro
/// - Uma notificação pode ser Cancelada se o usuário fizer check-in antes do envio
/// </summary>
public sealed class Notificacao
{
    /// <summary>
    /// Identificador único da notificação (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID do contato de emergência destinatário.
    /// </summary>
    public Guid ContatoEmergenciaId { get; private set; }

    /// <summary>
    /// ID do usuário que gerou esta notificação.
    /// </summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>
    /// Tipo de notificação (Lembrete6h, Lembrete2h, Emergencia).
    /// </summary>
    public TipoNotificacao TipoNotificacao { get; private set; }

    /// <summary>
    /// Meio de envio (Email, WhatsApp).
    /// </summary>
    public MeioNotificacao MeioNotificacao { get; private set; }

    /// <summary>
    /// Data e hora quando a notificação foi criada/enviada.
    /// </summary>
    public DateTime DataEnvio { get; private set; }

    /// <summary>
    /// Status atual da notificação (Pendente, Enviada, Erro, Cancelada).
    /// </summary>
    public StatusNotificacao Status { get; private set; }

    /// <summary>
    /// Mensagem de erro, se houver.
    /// </summary>
    public string? MensagemErro { get; private set; }

    private Notificacao() { }

    /// <summary>
    /// Factory para criar uma nova notificação de lembrete.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="contatoEmergenciaId">ID do contato a notificar.</param>
    /// <param name="tipoNotificacao">Tipo do lembrete (Lembrete6h ou Lembrete2h).</param>
    /// <param name="meioNotificacao">Meio de envio (Email ou WhatsApp).</param>
    /// <returns>Nova notificação de lembrete no estado Pendente.</returns>
    public static Notificacao CriarLembrete(
        Guid usuarioId,
        Guid contatoEmergenciaId,
        TipoNotificacao tipoNotificacao,
        MeioNotificacao meioNotificacao)
    {
        if (tipoNotificacao == TipoNotificacao.Emergencia)
            throw new InvalidOperationException("Use CriarEmergencia para notificações de emergência.");

        return new Notificacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            ContatoEmergenciaId = contatoEmergenciaId,
            TipoNotificacao = tipoNotificacao,
            MeioNotificacao = meioNotificacao,
            DataEnvio = DateTime.UtcNow,
            Status = StatusNotificacao.Pendente,
            MensagemErro = null
        };
    }

    /// <summary>
    /// Factory para criar uma nova notificação de emergência.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="contatoEmergenciaId">ID do contato a notificar.</param>
    /// <param name="meioNotificacao">Meio de envio (Email ou WhatsApp).</param>
    /// <returns>Nova notificação de emergência no estado Pendente.</returns>
    public static Notificacao CriarEmergencia(
        Guid usuarioId,
        Guid contatoEmergenciaId,
        MeioNotificacao meioNotificacao)
    {
        return new Notificacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            ContatoEmergenciaId = contatoEmergenciaId,
            TipoNotificacao = TipoNotificacao.Emergencia,
            MeioNotificacao = meioNotificacao,
            DataEnvio = DateTime.UtcNow,
            Status = StatusNotificacao.Pendente,
            MensagemErro = null
        };
    }

    /// <summary>
    /// Marca a notificação como enviada com sucesso.
    /// </summary>
    public void MarcarComoEnviada()
    {
        Status = StatusNotificacao.Enviada;
        MensagemErro = null;
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
    }

    /// <summary>
    /// Verifica se a notificação pode ser reenviada (está em erro ou pendente).
    /// </summary>
    /// <returns>True se a notificação está em estado "reenvável".</returns>
    public bool PodeSerReenviada()
    {
        return Status == StatusNotificacao.Erro || Status == StatusNotificacao.Pendente;
    }

    /// <summary>
    /// Calcula quantas horas se passaram desde o envio da notificação.
    /// </summary>
    /// <returns>Número de horas decorridas.</returns>
    public double HorasDesdeEnvio()
    {
        return (DateTime.UtcNow - DataEnvio).TotalHours;
    }

    /// <summary>
    /// Verifica se a notificação é antiga (enviada há mais de 24h) e pode ser descartada do histórico.
    /// </summary>
    /// <returns>True se a notificação tem mais de 24 horas.</returns>
    public bool EhAntiga()
    {
        return HorasDesdeEnvio() > 24;
    }
}
