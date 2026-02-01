using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaVida.Dominio.Entidades;

namespace ProvaVida.Infraestrutura.Mappings;

/// <summary>
/// Configuração do mapeamento ORM para a entidade ContatoEmergencia.
/// </summary>
public class ContatoEmergenciaMapping : IEntityTypeConfiguration<ContatoEmergencia>
{
    public void Configure(EntityTypeBuilder<ContatoEmergencia> builder)
    {
        builder.ToTable("contatos_emergencia");
        builder.HasKey(c => c.Id);

        builder
            .Property(c => c.UsuarioId)
            .IsRequired();

        builder
            .Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(150);

        // Value Object: Email
        builder
            .Property(c => c.Email)
            .HasConversion(ValueObjectConverters.EmailConverter())
            .IsRequired()
            .HasMaxLength(255);

        // Value Object: Telefone (WhatsApp)
        builder
            .Property(c => c.WhatsApp)
            .HasConversion(ValueObjectConverters.TelefoneConverter())
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("whatsapp");

        // Índices
        builder
            .HasIndex(c => c.UsuarioId)
            .HasDatabaseName("ix_contatos_usuario");

        builder
            .HasIndex(c => new { c.UsuarioId, c.Email })
            .IsUnique()
            .HasDatabaseName("ix_contatos_usuario_email");
    }
}
