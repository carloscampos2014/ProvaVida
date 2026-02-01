using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaVida.Dominio.Entidades;

namespace ProvaVida.Infraestrutura.Mappings;

/// <summary>
/// Configuração do mapeamento ORM para a entidade CheckIn.
/// </summary>
public class CheckInMapping : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("checkins");
        builder.HasKey(c => c.Id);

        builder
            .Property(c => c.UsuarioId)
            .IsRequired();

        builder
            .Property(c => c.DataCheckIn)
            .IsRequired();

        // Índices
        builder
            .HasIndex(c => c.UsuarioId)
            .HasDatabaseName("ix_checkins_usuario");

        builder
            .HasIndex(c => c.DataCheckIn)
            .HasDatabaseName("ix_checkins_data");
    }
}
