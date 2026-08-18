using FluentAssertions;
using Moq;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.TestarNotificacao;

namespace ProvaVida.Application.Tests.UseCases.TestarNotificacao;

public class TestarNotificacaoUseCaseTests
{
    private readonly Mock<IEmailService>    _emailMock   = new();
    private readonly Mock<IWhatsAppService> _whatsAppMock = new();
    private readonly TestarNotificacaoUseCase _useCase;

    public TestarNotificacaoUseCaseTests()
    {
        _useCase = new TestarNotificacaoUseCase(_emailMock.Object, _whatsAppMock.Object);
    }

    // ── E-mail ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TestarEmailAsync_Sucesso_RetornaSucessoComDuracao()
    {
        _emailMock.Setup(e => e.EnviarAsync(It.IsAny<EmailMensagem>(), default))
                  .Returns(Task.CompletedTask);

        var resultado = await _useCase.TestarEmailAsync("teste@exemplo.com");

        resultado.Sucesso.Should().BeTrue();
        resultado.Mensagem.Should().Contain("sucesso");
        resultado.DuracaoMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task TestarEmailAsync_Falha_RetornaFalhaComMensagemDeErro()
    {
        _emailMock.Setup(e => e.EnviarAsync(It.IsAny<EmailMensagem>(), default))
                  .ThrowsAsync(new Exception("Authentication failed (535)"));

        var resultado = await _useCase.TestarEmailAsync("teste@exemplo.com");

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Authentication failed (535)");
        resultado.DuracaoMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task TestarEmailAsync_EnviaParaDestinatarioCorreto()
    {
        EmailMensagem? mensagemCapturada = null;
        _emailMock.Setup(e => e.EnviarAsync(It.IsAny<EmailMensagem>(), default))
                  .Callback<EmailMensagem, CancellationToken>((m, _) => mensagemCapturada = m)
                  .Returns(Task.CompletedTask);

        await _useCase.TestarEmailAsync("destino@teste.com");

        mensagemCapturada!.Para.Should().Be("destino@teste.com");
        mensagemCapturada.Assunto.Should().Contain("Teste");
    }

    // ── WhatsApp ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task TestarWhatsAppAsync_Sucesso_RetornaSucessoComDuracao()
    {
        _whatsAppMock.Setup(w => w.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                     .Returns(Task.CompletedTask);

        var resultado = await _useCase.TestarWhatsAppAsync("5511999999999");

        resultado.Sucesso.Should().BeTrue();
        resultado.Mensagem.Should().Contain("sucesso");
        resultado.DuracaoMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task TestarWhatsAppAsync_Falha_RetornaFalhaComMensagemDeErro()
    {
        _whatsAppMock.Setup(w => w.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                     .ThrowsAsync(new Exception("Token inválido"));

        var resultado = await _useCase.TestarWhatsAppAsync("5511999999999");

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Token inválido");
        resultado.DuracaoMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task TestarWhatsAppAsync_EnviaParaTelefoneCorreto()
    {
        string? telefoneCapturado = null;
        _whatsAppMock.Setup(w => w.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                     .Callback<string, string, CancellationToken>((t, _, _) => telefoneCapturado = t)
                     .Returns(Task.CompletedTask);

        await _useCase.TestarWhatsAppAsync("5511988887777");

        telefoneCapturado.Should().Be("5511988887777");
    }
}
