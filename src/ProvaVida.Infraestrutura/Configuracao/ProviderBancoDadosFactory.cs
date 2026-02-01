using Microsoft.EntityFrameworkCore;
using ProvaVida.Infraestrutura.Contexto;

namespace ProvaVida.Infraestrutura.Configuracao;

/// <summary>
/// Factory para configurar DbContext com diferentes provedores de banco de dados.
/// Permite trocar de SQLite para PostgreSQL sem modificar o código dos repositórios.
/// </summary>
public static class ProviderBancoDadosFactory
{
    /// <summary>
    /// Configura o DbContext com o provedor de banco especificado
    /// </summary>
    /// <param name="opcoes">Builder de opções do DbContext</param>
    /// <param name="configuracao">Configuração do banco de dados</param>
    public static void ConfigurarProvedor(
        DbContextOptionsBuilder opcoes,
        ConfiguracaoBancoDados configuracao)
    {
        if (opcoes == null)
            throw new ArgumentNullException(nameof(opcoes));

        if (configuracao == null)
            throw new ArgumentNullException(nameof(configuracao));

        if (string.IsNullOrWhiteSpace(configuracao.StringConexao))
            throw new ArgumentException("String de conexão não pode estar vazia", nameof(configuracao));

        switch (configuracao.Tipo)
        {
            case TipoProviderBancoDados.SQLite:
                opcoes.UseSqlite(
                    configuracao.StringConexao,
                    sqliteOpcoes => sqliteOpcoes.MigrationsAssembly("ProvaVida.Infraestrutura"));
                break;

            case TipoProviderBancoDados.PostgreSQL:
                opcoes.UseNpgsql(
                    configuracao.StringConexao,
                    postgresOpcoes => postgresOpcoes.MigrationsAssembly("ProvaVida.Infraestrutura"));
                break;

            case TipoProviderBancoDados.SqlServer:
                // Pré-requisito: dotnet add package Microsoft.EntityFrameworkCore.SqlServer
                // opcoes.UseSqlServer(
                //     configuracao.StringConexao,
                //     sqlServerOpcoes => sqlServerOpcoes.MigrationsAssembly("ProvaVida.Infraestrutura"));
                throw new NotSupportedException("SQL Server requer pacote Microsoft.EntityFrameworkCore.SqlServer. Execute: dotnet add package Microsoft.EntityFrameworkCore.SqlServer");

            default:
                throw new NotSupportedException($"Provedor de banco '{configuracao.Tipo}' não é suportado");
        }
    }
}
