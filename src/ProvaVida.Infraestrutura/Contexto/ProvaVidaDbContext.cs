using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Infraestrutura.Mappings;

namespace ProvaVida.Infraestrutura.Contexto;

/// <summary>
/// Contexto do Entity Framework Core para ProvaVida.
/// Centraliza a configuração de todas as entidades e relacionamentos.
/// 
/// Princípio: Agnóstico ao banco de dados - pode trocar SQLite por SQL Server sem modificar esta classe.
/// </summary>
public class ProvaVidaDbContext : DbContext
{
    public ProvaVidaDbContext(DbContextOptions<ProvaVidaDbContext> options)
        : base(options)
    {
    }

    // 📊 DbSets - Coleções de entidades
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<CheckIn> CheckIns { get; set; } = null!;
    public DbSet<ContatoEmergencia> ContatosEmergencia { get; set; } = null!;
    public DbSet<Notificacao> Notificacoes { get; set; } = null!;

    /// <summary>
    /// Configura o modelo de dados com Fluent API.
    /// Todas as configurações são delegadas a arquivos de mapping separados.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🔧 Aplica mapeamentos de cada entidade
        modelBuilder.ApplyConfiguration(new UsuarioMapping());
        modelBuilder.ApplyConfiguration(new CheckInMapping());
        modelBuilder.ApplyConfiguration(new ContatoEmergenciaMapping());
        modelBuilder.ApplyConfiguration(new NotificacaoMapping());

        // 🌍 Configurações globais
        modelBuilder.HasDefaultSchema("public");

        // 🔒 Sem tracking por padrão (melhor performance para queries de leitura)
        // Comentado: queremos tracking por padrão
        // modelBuilder.QueryFilterExpression.DefaultTrackingQueryBehavior(QueryTrackingBehavior.NoTracking);
    }
}
