using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaVida.Dominio.Entidades;

namespace ProvaVida.Infraestrutura.Mappings;

/// <summary>
/// Configuração do mapeamento ORM para a entidade Notificacao.
/// </summary>
public class NotificacaoMapping : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.ToTable("notificacoes");
        builder.HasKey(n => n.Id);

        builder
            .Property(n => n.UsuarioId)
            .IsRequired();

        builder
            .Property(n => n.ContatoEmergenciaId);

        // Enum: TipoNotificacao as int
        builder
            .Property(n => n.TipoNotificacao)
            .HasConversion<int>()
            .IsRequired();

        // Enum: MeioNotificacao as int
        builder
            .Property(n => n.MeioNotificacao)
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(n => n.DataCriacao)
            .IsRequired();

        // Enum: StatusNotificacao as int
        builder
            .Property(n => n.Status)
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(n => n.DataProximoReenvio)
            .IsRequired(false);  // Null para lembretes

        builder
            .Property(n => n.MensagemErro)
            .HasMaxLength(500);

        // Índices
        builder
            .HasIndex(n => n.UsuarioId)
            .HasDatabaseName("ix_notificacoes_usuario");

        builder
            .HasIndex(n => n.ContatoEmergenciaId)
            .HasDatabaseName("ix_notificacoes_contato");

        builder
            .HasIndex(n => n.Status)
            .HasDatabaseName("ix_notificacoes_status");

        builder
            .HasIndex(n => n.DataProximoReenvio)
            .HasDatabaseName("ix_notificacoes_proxreenvio");
    }
}
