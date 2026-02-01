namespace ProvaVida.Infraestrutura.Configuracao;

/// <summary>
/// Opções de banco de dados suportados
/// </summary>
public enum TipoProviderBancoDados
{
    SQLite = 0,
    PostgreSQL = 1,
    SqlServer = 2
}

/// <summary>
/// Configuração do provedor de banco de dados
/// </summary>
public class ConfiguracaoBancoDados
{
    /// <summary>
    /// Tipo de provedor a usar
    /// </summary>
    public TipoProviderBancoDados Tipo { get; set; } = TipoProviderBancoDados.SQLite;

    /// <summary>
    /// String de conexão do banco de dados
    /// </summary>
    public string StringConexao { get; set; } = string.Empty;
}
