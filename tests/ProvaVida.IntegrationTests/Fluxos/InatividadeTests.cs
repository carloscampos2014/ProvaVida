using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.VerificarInatividade;
using ProvaVida.Domain.Entities;
using ProvaVida.IntegrationTests.Helpers;
using ProvaVida.IntegrationTests.Infrastructure;

namespace ProvaVida.IntegrationTests.Fluxos;

/// <summary>
/// Testa o fluxo de inatividade diretamente via UseCase (sem HTTP),
/// pois o job Hangfire está desabilitado nos testes.
/// O banco real é usado para verificar persistência.
/// </summary>
public class InatividadeTests : IClassFixture<ProvaVidaWebFactory>, IAsyncLifetime
{
    private readonly ProvaVidaWebFactory _factory;
    private readonly HttpClient _client;
    private readonly DatabaseCleaner _cleaner;

    public InatividadeTests(ProvaVidaWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cleaner = new DatabaseCleaner("Host=localhost;Port=5432;Database=provavida_dev;Username=postgres;Password=12345678");
    }

    public async Task InitializeAsync()
    {
        await _cleaner.LimparAsync();
        // Reset mocks para evitar acumulação de chamadas entre testes
        _factory.EmailServiceMock.Reset();
        _factory.WhatsAppServiceMock.Reset();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Inatividade_ComHeartbeat_SuspendeAlerta()
    {
        // Arrange — cria usuário sem check-in
        var (token, _) = await AuthHelper.CriarUsuarioELogarAsync(_client);
        AuthHelper.SetBearerToken(_client, token);

        // Envia heartbeat
        await _client.PostAsync("/heartbeat", null);

        // Act — executa detecção via UseCase com banco real
        using var scope = _factory.Services.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<VerificarInatividadeUseCase>();

        await useCase.ExecutarDeteccaoAsync();

        // Assert — e-mail ao usuário NÃO deve ter sido chamado (heartbeat suspendeu)
        _factory.EmailServiceMock.Verify(
            s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Inatividade_SemHeartbeatSemCheckIn_EnviaAvisoAoUsuario()
    {
        // Arrange — cria usuário sem check-in e sem heartbeat
        await AuthHelper.CriarUsuarioELogarAsync(_client);

        // Mock do e-mail configurado para aceitar envio
        _factory.EmailServiceMock
            .Setup(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default))
            .Returns(Task.CompletedTask);

        // Act
        using var scope = _factory.Services.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<VerificarInatividadeUseCase>();

        await useCase.ExecutarDeteccaoAsync();

        // Assert — e-mail de aviso enviado ao próprio usuário
        _factory.EmailServiceMock.Verify(
            s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Inatividade_JanelaExpirada_DispararEmailWhatsAppAoContato()
    {
        // Arrange
        await AuthHelper.CriarUsuarioELogarAsync(_client);

        _factory.EmailServiceMock
            .Setup(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default))
            .Returns(Task.CompletedTask);
        _factory.WhatsAppServiceMock
            .Setup(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        using var scope = _factory.Services.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<VerificarInatividadeUseCase>();

        // Primeiro roda detecção → cria registro aguardando_resposta
        await useCase.ExecutarDeteccaoAsync();

        // Simula janela expirada atualizando diretamente no banco
        var db = new DatabaseCleaner("Host=localhost;Port=5432;Database=provavida_dev;Username=postgres;Password=12345678");
        await ExpirateJanelasAsync();

        // Act — roda disparo
        await useCase.ExecutarDisparoAsync();

        // Assert — e-mail e WhatsApp ao contato disparados
        _factory.EmailServiceMock.Verify(
            s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default),
            Times.AtLeast(2)); // aviso ao usuário + alerta ao contato

        _factory.WhatsAppServiceMock.Verify(
            s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Once);
    }

    private async Task ExpirateJanelasAsync()
    {
        // Força expiração das janelas pendentes no banco para simular passagem do tempo
        using var conn = new Npgsql.NpgsqlConnection(
            "Host=localhost;Port=5432;Database=provavida_dev;Username=postgres;Password=12345678");
        await conn.OpenAsync();
        await Dapper.SqlMapper.ExecuteAsync(conn,
            "UPDATE notificacoes_emergencia SET janela_expira_em = NOW() - INTERVAL '1 second' WHERE status = 'aguardando_resposta'");
    }
}
