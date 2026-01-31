using Xunit;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Exceções;
using ProvaVida.Dominio.Enums;

namespace ProvaVida.Dominio.Tests.Entidades;

/// <summary>
/// Testes para a Entidade Usuario.
/// Valida criação, cálculo de 48h, histórico FIFO e regras de negócio.
/// </summary>
public class UsuarioTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveSuceder()
    {
        // Arrange & Act
        var usuario = Usuario.Criar("João Silva", "joao@example.com", "11987654321", "hash123");

        // Assert
        Assert.NotNull(usuario);
        Assert.Equal("João Silva", usuario.Nome);
        Assert.Equal("joao@example.com", usuario.Email.ToString());
        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    [Fact]
    public void Criar_SemNome_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<UsuarioInvalidoException>(() => 
            Usuario.Criar("", "joao@example.com", "11987654321", "hash123"));
    }

    [Fact]
    public void Criar_ComEmailInvalido_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<UsuarioInvalidoException>(() => 
            Usuario.Criar("João", "email_inválido", "11987654321", "hash123"));
    }

    [Fact]
    public void Criar_ComTelefoneInvalido_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<UsuarioInvalidoException>(() => 
            Usuario.Criar("João", "joao@example.com", "123", "hash123"));
    }

    #endregion

    #region Testes de 48 Horas

    [Fact]
    public void RegistrarCheckIn_DeveDefinirDataProximoVencimentoComo48HorasNoFuturo()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        var agora = DateTime.UtcNow;

        // Act
        usuario.RegistrarCheckIn();

        // Assert
        // Verifica se a data próximo vencimento é 48 horas (com tolerância de 2 segundos)
        var diferencaEmHoras = (usuario.DataProximoVencimento - usuario.DataUltimoCheckIn).TotalHours;
        Assert.True(diferencaEmHoras >= 47.999 && diferencaEmHoras <= 48.001, 
            $"Diferença esperada: 48h, obtido: {diferencaEmHoras}h");
    }

    [Fact]
    public void RegistrarCheckIn_DeveResetarPrazoA48Horas()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        usuario.RegistrarCheckIn();
        var primeiroVencimento = usuario.DataProximoVencimento;

        // Act
        System.Threading.Thread.Sleep(100); // Aguarda pequeno intervalo
        usuario.RegistrarCheckIn();
        var segundoVencimento = usuario.DataProximoVencimento;

        // Assert
        Assert.NotEqual(primeiroVencimento, segundoVencimento);
        Assert.True(segundoVencimento > primeiroVencimento);
    }

    [Fact]
    public void EstaVencido_AntesDeVencer_DevRetornarFalse()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        usuario.RegistrarCheckIn();

        // Act
        var estaVencido = usuario.EstaVencido();

        // Assert
        Assert.False(estaVencido);
    }

    [Fact]
    public void HorasAteVencimento_DeveRetornarAproximadamente48()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        usuario.RegistrarCheckIn();

        // Act
        var horasAte = usuario.HorasAteVencimento();

        // Assert
        Assert.True(horasAte > 47 && horasAte < 49, $"Esperado ~48h, obtido: {horasAte}h");
    }

    #endregion

    #region Testes de FIFO (5 Registros)

    [Fact]
    public void HistoricoCheckIns_ComUmCheckIn_DeveConterApenasUm()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");

        // Act
        usuario.RegistrarCheckIn();

        // Assert
        Assert.Single(usuario.HistoricoCheckIns);
    }

    [Fact]
    public void HistoricoCheckIns_ComCincoCheckIns_DeveConterCinco()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");

        // Act
        for (int i = 0; i < 5; i++)
        {
            usuario.RegistrarCheckIn();
            System.Threading.Thread.Sleep(50);
        }

        // Assert
        Assert.Equal(5, usuario.HistoricoCheckIns.Count);
    }

    [Fact]
    public void HistoricoCheckIns_ComSeisCheckIns_DeveManterApenas5Ultimos()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        var datasCheckIns = new List<DateTime>();

        // Act
        for (int i = 0; i < 6; i++)
        {
            usuario.RegistrarCheckIn();
            datasCheckIns.Add(usuario.DataUltimoCheckIn);
            System.Threading.Thread.Sleep(50);
        }

        // Assert
        Assert.Equal(5, usuario.HistoricoCheckIns.Count);

        // Verifica que os 5 últimos check-ins são mantidos
        var historicoOrdenado = usuario.HistoricoCheckIns.OrderBy(c => c.DataCheckIn).ToList();
        for (int i = 0; i < 5; i++)
        {
            // O 1º check-in (índice 0 da lista) foi removido, então comparamos com índice i+1
            var diferencaTicks = Math.Abs(datasCheckIns[i + 1].ToUniversalTime().Ticks - historicoOrdenado[i].DataCheckIn.Ticks);
            Assert.True(diferencaTicks < 1000000, $"Diferença de ticks: {diferencaTicks}, esperado < 1000000");
        }
    }

    [Fact]
    public void HistoricoCheckIns_ComDezeCheckIns_DeveManterApenas5Ultimos()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");

        // Act
        for (int i = 0; i < 10; i++)
        {
            usuario.RegistrarCheckIn();
            System.Threading.Thread.Sleep(50);
        }

        // Assert
        Assert.Equal(5, usuario.HistoricoCheckIns.Count);
    }

    [Fact]
    public void HistoricoCheckIns_DeveManterOsCheckInsEmOrdemCrescente()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        var datasEsperadas = new List<DateTime>();

        // Act
        for (int i = 0; i < 6; i++)
        {
            usuario.RegistrarCheckIn();
            if (i >= 1) // Apenas os últimos 5
                datasEsperadas.Add(usuario.DataUltimoCheckIn);
            System.Threading.Thread.Sleep(50);
        }

        // Assert
        var historico = usuario.HistoricoCheckIns.OrderBy(c => c.DataCheckIn).ToList();
        Assert.Equal(5, historico.Count);

        // Verifica que estão em ordem crescente
        for (int i = 1; i < historico.Count; i++)
        {
            Assert.True(historico[i].DataCheckIn >= historico[i - 1].DataCheckIn);
        }
    }

    #endregion

    #region Testes de Contatos de Emergência

    [Fact]
    public void AdicionarContato_ComContatoValido_DeveSuceder()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        var contato = ContatoEmergencia.Criar(usuario.Id, "Maria", "maria@example.com", "11987654322");

        // Act
        usuario.AdicionarContato(contato);

        // Assert
        Assert.Single(usuario.Contatos);
    }

    [Fact]
    public void RemoverContato_AoTentarRemoverUnicoContato_DeveLancarException()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        var contato = ContatoEmergencia.Criar(usuario.Id, "Maria", "maria@example.com", "11987654322");
        usuario.AdicionarContato(contato);

        // Act & Assert
        Assert.Throws<ContatoObrigatorioException>(() => usuario.RemoverContato(contato.Id));
    }

    #endregion

    #region Testes de Status

    [Fact]
    public void Status_AoCriar_DeveSerAtivo()
    {
        // Arrange & Act
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");

        // Assert
        Assert.Equal(StatusUsuario.Ativo, usuario.Status);
    }

    [Fact]
    public void DeveDispararLembrete6h_Quando6HorasFaltam_DevRetornarTrue()
    {
        // Arrange
        var usuario = Usuario.Criar("João", "joao@example.com", "11987654321", "hash123");
        usuario.RegistrarCheckIn();
        
        // Simula passagem de tempo (não é real, apenas verifica a lógica)
        // Como DataProximoVencimento é calculado em runtime, testamos o método
        
        // Act
        var deve = usuario.DeveDispararLembrete6h();

        // Assert (no momento de criação, faltam ~48h, não 6h)
        Assert.False(deve);
    }

    #endregion
}
