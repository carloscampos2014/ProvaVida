using Moq;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Enums;
using ProvaVida.Dominio.ObjetosValor;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Servicos;

namespace ProvaVida.Aplicacao.Tests.Helpers;

/// <summary>
/// Classe auxiliar para criar mocks de repositórios com dados pré-configurados.
/// </summary>
public static class RepositorioMocks
{
    /// <summary>
    /// Cria um mock do repositório de usuários.
    /// </summary>
    public static Mock<IRepositorioUsuario> CriarRepositorioUsuarioMock()
    {
        var mock = new Mock<IRepositorioUsuario>();
        
        // Setup padrão: sem usuários
        mock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);
        
        mock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);
        
        return mock;
    }

    /// <summary>
    /// Cria um mock do repositório de check-ins.
    /// </summary>
    public static Mock<IRepositorioCheckIn> CriarRepositorioCheckInMock()
    {
        var mock = new Mock<IRepositorioCheckIn>();
        
        // Setup padrão: sem check-ins
        mock.Setup(r => r.ObterHistoricoAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheckIn>());
        
        mock.Setup(r => r.ObterMaisRecentePorUsuarioIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckIn?)null);
        
        return mock;
    }

    /// <summary>
    /// Cria um mock do repositório de notificações.
    /// </summary>
    public static Mock<IRepositorioNotificacao> CriarRepositorioNotificacaoMock()
    {
        var mock = new Mock<IRepositorioNotificacao>();
        
        // Setup padrão: sem notificações
        mock.Setup(r => r.ObterPorUsuarioIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notificacao>());
        
        return mock;
    }

    /// <summary>
    /// Cria um mock do repositório de contatos de emergência.
    /// </summary>
    public static Mock<IRepositorioContatoEmergencia> CriarRepositorioContatoEmergenciaMock()
    {
        var mock = new Mock<IRepositorioContatoEmergencia>();
        
        // Setup padrão: sem contatos
        mock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContatoEmergencia?)null);
        
        mock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContatoEmergencia?)null);
        
        mock.Setup(r => r.ObterPorUsuarioIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContatoEmergencia>());
        
        return mock;
    }

    /// <summary>
    /// Cria um mock do serviço de hash de senha.
    /// </summary>
    public static Mock<IServicoHashSenha> CriarServicoHashSenhaMock()
    {
        var mock = new Mock<IServicoHashSenha>();
        
        // Setup padrão: hash sempre diferente da original
        mock.Setup(s => s.Verificar(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        
        mock.Setup(s => s.Hashar(It.IsAny<string>()))
            .Returns((string senha) => $"hash_bcrypt_{senha}_12rounds");
        
        return mock;
    }

    /// <summary>
    /// Cria um usuário válido para testes.
    /// Nota: O ID é sempre gerado automaticamente pelo Factory.
    /// </summary>
    public static Usuario CriarUsuarioValido(
        string email = "usuario@teste.com",
        string telefone = "11999999999")
    {
        var usuario = Usuario.Criar(
            nome: "Usuário Teste",
            email: email,
            telefone: telefone,
            senhaHash: "hash_bcrypt_teste_12rounds"
        );
        
        return usuario;
    }

    /// <summary>
    /// Cria um check-in válido para testes.
    /// </summary>
    public static CheckIn CriarCheckInValido(Guid? usuarioId = null, DateTime? dataCheckIn = null)
    {
        var checkIn = CheckIn.Criar(
            usuarioId: usuarioId ?? Guid.NewGuid(),
            dataCheckIn: dataCheckIn ?? DateTime.UtcNow,
            localizacao: "São Paulo, SP"
        );
        
        return checkIn;
    }

    /// <summary>
    /// Cria uma notificação de lembrete válida para testes.
    /// </summary>
    public static Notificacao CriarNotificacaoValida(Guid? usuarioId = null)
    {
        var notificacao = Notificacao.CriarLembreteUsuario(
            usuarioId: usuarioId ?? Guid.NewGuid(),
            meioNotificacao: MeioNotificacao.Email
        );
        
        return notificacao;
    }
}
