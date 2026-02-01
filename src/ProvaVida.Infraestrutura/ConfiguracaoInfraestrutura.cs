using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Configuracao;
using ProvaVida.Infraestrutura.Contexto;
using ProvaVida.Infraestrutura.Repositorios;
using ProvaVida.Infraestrutura.Servicos;

namespace ProvaVida.Infraestrutura;

/// <summary>
/// Extensão para registrar serviços de Infraestrutura na aplicação.
/// </summary>
public static class ConfiguracaoInfraestrutura
{
    /// <summary>
    /// Adiciona a infraestrutura ao contêiner de injeção de dependência.
    /// Permite usar diferentes provedores de banco de dados (SQLite, PostgreSQL, SQL Server).
    /// </summary>
    /// <param name="servicos">Contêiner de injeção de dependência.</param>
    /// <param name="configuracao">Configuração do banco de dados (tipo e string de conexão).</param>
    /// <returns>O contêiner de serviços para encadeamento.</returns>
    /// <example>
    /// // Usar SQLite
    /// var config = new ConfiguracaoBancoDados 
    /// { 
    ///     Tipo = TipoProviderBancoDados.SQLite,
    ///     StringConexao = "Data Source=provavida.db"
    /// };
    /// services.AdicionarInfraestrutura(config);
    /// 
    /// // Usar PostgreSQL
    /// var config = new ConfiguracaoBancoDados 
    /// { 
    ///     Tipo = TipoProviderBancoDados.PostgreSQL,
    ///     StringConexao = "Host=localhost;Database=provavida;User Id=postgres;Password=..."
    /// };
    /// services.AdicionarInfraestrutura(config);
    /// </example>
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos,
        ConfiguracaoBancoDados configuracao)
    {
        if (servicos == null)
            throw new ArgumentNullException(nameof(servicos));

        if (configuracao == null)
            throw new ArgumentNullException(nameof(configuracao));

        // DbContext com provedor flexível
        servicos.AddDbContext<ProvaVidaDbContext>(opcoes =>
            ProviderBancoDadosFactory.ConfigurarProvedor(opcoes, configuracao));

        // Repositórios
        servicos.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        servicos.AddScoped<IRepositorioCheckIn, RepositorioCheckIn>();
        servicos.AddScoped<IRepositorioContatoEmergencia, RepositorioContatoEmergencia>();
        servicos.AddScoped<IRepositorioNotificacao, RepositorioNotificacao>();

        // Serviços
        servicos.AddScoped<IServicoHashSenha, ServicoHashSenha>();

        return servicos;
    }

    /// <summary>
    /// Versão legada: Adiciona a infraestrutura com SQLite (compatibilidade com código antigo).
    /// Use AdicionarInfraestrutura(ConfiguracaoBancoDados) para suportar múltiplos provedores.
    /// </summary>
    [Obsolete("Use AdicionarInfraestrutura(ConfiguracaoBancoDados configuracao) em vez disso")]
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos,
        string stringConexao)
    {
        var configuracao = new ConfiguracaoBancoDados
        {
            Tipo = TipoProviderBancoDados.SQLite,
            StringConexao = stringConexao
        };

        return servicos.AdicionarInfraestrutura(configuracao);
    }
}
