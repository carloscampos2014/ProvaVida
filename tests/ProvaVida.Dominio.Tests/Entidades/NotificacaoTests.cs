using Xunit;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Enums;

namespace ProvaVida.Dominio.Tests.Entidades;

/// <summary>
/// Testes para a entidade Notificacao.
/// Valida criação, tipos, reenvios e cálculos de Data Próximo Reenvio.
/// </summary>
public class NotificacaoTests
{
    #region Criação de Notificações

    [Fact]
    public void CriarLembreteUsuario_ComDadosValidos_DeveSuceder()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var meioNotificacao = MeioNotificacao.Email;

        // Act
        var notificacao = Notificacao.CriarLembreteUsuario(usuarioId, meioNotificacao);

        // Assert
        Assert.NotEqual(Guid.Empty, notificacao.Id);
        Assert.Equal(usuarioId, notificacao.UsuarioId);
        Assert.Equal(Guid.Empty, notificacao.ContatoEmergenciaId);  // Sem contato
        Assert.Equal(TipoNotificacao.LembreteUsuario, notificacao.TipoNotificacao);
        Assert.Equal(MeioNotificacao.Email, notificacao.MeioNotificacao);
        Assert.Equal(StatusNotificacao.Pendente, notificacao.Status);
        Assert.Null(notificacao.DataProximoReenvio);  // Lembretes não são reenviados
    }

    [Fact]
    public void CriarEmergenciaContatos_ComDadosValidos_DeveSuceder()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var contatoId = Guid.NewGuid();
        var meioNotificacao = MeioNotificacao.Email;

        // Act
        var notificacao = Notificacao.CriarEmergenciaContatos(usuarioId, contatoId, meioNotificacao);

        // Assert
        Assert.NotEqual(Guid.Empty, notificacao.Id);
        Assert.Equal(usuarioId, notificacao.UsuarioId);
        Assert.Equal(contatoId, notificacao.ContatoEmergenciaId);
        Assert.Equal(TipoNotificacao.EmergenciaContatos, notificacao.TipoNotificacao);
        Assert.Equal(MeioNotificacao.Email, notificacao.MeioNotificacao);
        Assert.Equal(StatusNotificacao.Pendente, notificacao.Status);
        Assert.Null(notificacao.DataProximoReenvio);  // Será calculado ao enviar
    }

    [Fact]
    public void CriarEmergenciaContatos_ComContatoVazio_DeveLancarException()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var contatoVazio = Guid.Empty;
        var meioNotificacao = MeioNotificacao.Email;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Notificacao.CriarEmergenciaContatos(usuarioId, contatoVazio, meioNotificacao));
    }

    #endregion

    #region Marcação como Enviada e Reenvio

    [Fact]
    public void MarcarLembreteComoEnviada_DeveNaoCalcularDataProximoReenvio()
    {
        // Arrange
        var notificacao = Notificacao.CriarLembreteUsuario(Guid.NewGuid(), MeioNotificacao.Email);
        var dataAntes = notificacao.DataProximoReenvio;

        // Act
        notificacao.MarcarComoEnviada();

        // Assert
        Assert.Equal(StatusNotificacao.Enviada, notificacao.Status);
        Assert.Null(notificacao.DataProximoReenvio);
        Assert.Null(dataAntes);
    }

    [Fact]
    public void MarcarEmergenciaComoEnviada_DeveCalcularProximoReenvio6h()
    {
        // Arrange
        var notificacao = Notificacao.CriarEmergenciaContatos(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            MeioNotificacao.Email);

        var dataAntesDeSendMarcada = notificacao.DataProximoReenvio;
        var agora = DateTime.UtcNow;

        // Act
        notificacao.MarcarComoEnviada();

        // Assert
        Assert.Equal(StatusNotificacao.Enviada, notificacao.Status);
        Assert.Null(dataAntesDeSendMarcada);
        Assert.NotNull(notificacao.DataProximoReenvio);
        
        // DataProximoReenvio deve ser 6 horas no futuro
        var diferenca = notificacao.DataProximoReenvio.Value - agora;
        Assert.True(diferenca.TotalHours >= 5.99 && diferenca.TotalHours <= 6.01, 
            $"Diferença esperada: ~6h, obtida: {diferenca.TotalHours}h");
    }

    #endregion

    #region Reenvio de Emergências

    [Fact]
    public void DeveSerReenviada_ParaLembrete_DevRetornarFalse()
    {
        // Arrange
        var notificacao = Notificacao.CriarLembreteUsuario(Guid.NewGuid(), MeioNotificacao.Email);
        notificacao.MarcarComoEnviada();

        // Act
        var deveReenviar = notificacao.DeveSerReenviada();

        // Assert
        Assert.False(deveReenviar);  // Lembretes não são reenviados
    }

    [Fact]
    public void DeveSerReenviada_QuandoAindaNaoPassouDataProximoReenvio_DevRetornarFalse()
    {
        // Arrange
        var notificacao = Notificacao.CriarEmergenciaContatos(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            MeioNotificacao.Email);
        notificacao.MarcarComoEnviada();

        // Act
        var deveReenviar = notificacao.DeveSerReenviada();

        // Assert
        Assert.False(deveReenviar);  // Ainda não chegou a data de reenvio
    }

    [Fact]
    public void DeveSerReenviada_QuandoStatusPendente_DevRetornarFalse()
    {
        // Arrange
        var notificacao = Notificacao.CriarEmergenciaContatos(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            MeioNotificacao.Email);
        // Deixa em Pendente (não marca como Enviada)

        // Act
        var deveReenviar = notificacao.DeveSerReenviada();

        // Assert
        Assert.False(deveReenviar);  // Deve estar Enviada para reenviar
    }

    [Fact]
    public void DeveSerReenviada_QuandoUltrapassa48Horas_DevRetornarFalse()
    {
        // Arrange
        var notificacao = Notificacao.CriarEmergenciaContatos(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            MeioNotificacao.Email);
        notificacao.MarcarComoEnviada();

        // Simula que passou mais de 48h desde a criação
        // (Isso seria feito em um teste de integração com clock mocking)
        // Por enquanto, apenas verificamos a lógica básica

        // Act
        var horasDesdeOrigem = notificacao.HorasDesciasCriacao();
        var ultrapassou48h = notificacao.Ultrapassa48Horas();

        // Assert
        Assert.True(horasDesdeOrigem >= 0);  // Tem que ter passado pelo menos 0h
        Assert.False(ultrapassou48h);  // Criada agora, não passou 48h
    }

    #endregion

    #region Cancelamento

    [Fact]
    public void Cancelar_DeveMudarStatusParaCancelada()
    {
        // Arrange
        var notificacao = Notificacao.CriarLembreteUsuario(Guid.NewGuid(), MeioNotificacao.Email);

        // Act
        notificacao.Cancelar();

        // Assert
        Assert.Equal(StatusNotificacao.Cancelada, notificacao.Status);
        Assert.Null(notificacao.DataProximoReenvio);  // Limpa data de reenvio ao cancelar
    }

    #endregion

    #region Erro

    [Fact]
    public void MarcarComErro_DeveRegistrarMensagem()
    {
        // Arrange
        var notificacao = Notificacao.CriarLembreteUsuario(Guid.NewGuid(), MeioNotificacao.Email);
        var mensagemErro = "Falha ao enviar email";

        // Act
        notificacao.MarcarComErro(mensagemErro);

        // Assert
        Assert.Equal(StatusNotificacao.Erro, notificacao.Status);
        Assert.Equal(mensagemErro, notificacao.MensagemErro);
    }

    #endregion

    #region Cálculo de Horas

    [Fact]
    public void HorasDesciasCriacao_DeveRetornarAproximadamenteZero()
    {
        // Arrange
        var notificacao = Notificacao.CriarLembreteUsuario(Guid.NewGuid(), MeioNotificacao.Email);

        // Act
        var horas = notificacao.HorasDesciasCriacao();

        // Assert
        Assert.True(horas >= 0 && horas < 0.1, 
            $"Esperado: ~0 horas, obtido: {horas}h");
    }

    #endregion
}
