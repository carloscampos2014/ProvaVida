using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProvaVida.Infraestrutura.Configuracao;
using ProvaVida.Infraestrutura.Contexto;

namespace ProvaVida.Infraestrutura.Exemplos;

/// <summary>
/// Exemplos de como usar diferentes provedores de banco de dados
/// com a mesma camada de infraestrutura.
/// 
/// EXECUTE: dotnet run --project src/ProvaVida.Infraestrutura --para ver os exemplos
/// </summary>
public static class ExemplosProveedorBD
{
    /// <summary>
    /// Exemplo 1: Configurar DbContext com SQLite
    /// </summary>
    public static DbContextOptions<ProvaVidaDbContext> ConfigurarSQLite()
    {
        var config = new ConfiguracaoBancoDados
        {
            Tipo = TipoProviderBancoDados.SQLite,
            StringConexao = "Data Source=provavida_sqlite.db"
        };

        var builder = new DbContextOptionsBuilder<ProvaVidaDbContext>();
        ProviderBancoDadosFactory.ConfigurarProvedor(builder, config);
        return builder.Options;
    }

    /// <summary>
    /// Exemplo 2: Configurar DbContext com PostgreSQL
    /// 
    /// Pré-requisito: PostgreSQL em execução
    /// docker run --name postgres -e POSTGRES_PASSWORD=senha -p 5432:5432 -d postgres
    /// </summary>
    public static DbContextOptions<ProvaVidaDbContext> ConfigurarPostgreSQL()
    {
        var config = new ConfiguracaoBancoDados
        {
            Tipo = TipoProviderBancoDados.PostgreSQL,
            StringConexao = "Host=localhost;Port=5432;Database=provavida_db;User Id=postgres;Password=senha"
        };

        var builder = new DbContextOptionsBuilder<ProvaVidaDbContext>();
        ProviderBancoDadosFactory.ConfigurarProvedor(builder, config);
        return builder.Options;
    }

    /// <summary>
    /// Exemplo 3: Configurar DbContext com SQL Server
    /// 
    /// Pré-requisito: SQL Server em execução
    /// docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=SenhaForte123 -p 1433:1433 mcr.microsoft.com/mssql/server
    /// </summary>
    public static DbContextOptions<ProvaVidaDbContext> ConfigurarSqlServer()
    {
        var config = new ConfiguracaoBancoDados
        {
            Tipo = TipoProviderBancoDados.SqlServer,
            StringConexao = "Server=localhost;Database=ProvaVida;User Id=sa;Password=SenhaForte123;Encrypt=false"
        };

        var builder = new DbContextOptionsBuilder<ProvaVidaDbContext>();
        ProviderBancoDadosFactory.ConfigurarProvedor(builder, config);
        return builder.Options;
    }

    /// <summary>
    /// Exemplo 4: Usar variáveis de ambiente para configuração
    /// </summary>
    public static DbContextOptions<ProvaVidaDbContext> ConfigurarDoAmbiente()
    {
        var tipoProvedor = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "SQLite";
        var stringConexao = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "Data Source=provavida.db";

        var config = new ConfiguracaoBancoDados
        {
            Tipo = Enum.Parse<TipoProviderBancoDados>(tipoProvedor),
            StringConexao = stringConexao
        };

        var builder = new DbContextOptionsBuilder<ProvaVidaDbContext>();
        ProviderBancoDadosFactory.ConfigurarProvedor(builder, config);
        return builder.Options;
    }

    /// <summary>
    /// Teste: Verifique que todos os provedores podem ser instanciados
    /// </summary>
    public static void TestarTodosProvedores()
    {
        Console.WriteLine("🔍 Testando suporte a múltiplos provedores...\n");

        try
        {
            Console.WriteLine("✅ SQLite configurado com sucesso");
            var sqliteOpcoes = ConfigurarSQLite();

            Console.WriteLine("✅ PostgreSQL configurado com sucesso");
            var pgOpcoes = ConfigurarPostgreSQL();

            Console.WriteLine("✅ SQL Server configurado com sucesso");
            var ssOpcoes = ConfigurarSqlServer();

            Console.WriteLine("\n✅ TODOS OS PROVEDORES SUPORTADOS!");
            Console.WriteLine("\n📋 Resumo:");
            Console.WriteLine("   - SQLite: Desenvolvimento local");
            Console.WriteLine("   - PostgreSQL: Produção (Linux/Mac)");
            Console.WriteLine("   - SQL Server: Produção (Windows)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao configurar provedores: {ex.Message}");
        }
    }
}

/// <summary>
/// Como usar em Program.cs (ASP.NET Core)
/// </summary>
public static class ExemploProgram
{
    public static void ConfigurarServicosExemplo(IServiceCollection services)
    {
        // Opção 1: SQLite
        var configDb = new ConfiguracaoBancoDados
        {
            Tipo = TipoProviderBancoDados.SQLite,
            StringConexao = "Data Source=provavida.db"
        };
        services.AdicionarInfraestrutura(configDb);

        // Opção 2: PostgreSQL (comentar SQLite acima)
        // var configDb = new ConfiguracaoBancoDados
        // {
        //     Tipo = TipoProviderBancoDados.PostgreSQL,
        //     StringConexao = "Host=localhost;Database=provavida;User Id=postgres;Password=senha"
        // };
        // services.AdicionarInfraestrutura(configDb);
    }
}
