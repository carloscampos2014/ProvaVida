using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaVida.Dominio.Entidades;

namespace ProvaVida.Infraestrutura.Mappings;

/// <summary>
/// Configuração do mapeamento ORM para a entidade Usuario.
/// Define como a entidade é persistida no banco de dados SQLite.
/// </summary>
public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // 🔑 Tabela e Chave Primária
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);

        // 📋 Propriedades Escalares
        builder
            .Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(150);

        // 📧 Value Object: Email → String
        builder
            .Property(u => u.Email)
            .HasConversion(ValueObjectConverters.EmailConverter())
            .IsRequired()
            .HasMaxLength(255);

        // 📱 Value Object: Telefone → String
        builder
            .Property(u => u.Telefone)
            .HasConversion(ValueObjectConverters.TelefoneConverter())
            .IsRequired()
            .HasMaxLength(20);

        builder
            .Property(u => u.SenhaHash)
            .IsRequired()
            .HasMaxLength(256);

        // ⏰ Datas
        builder
            .Property(u => u.DataUltimoCheckIn)
            .IsRequired();

        builder
            .Property(u => u.DataProximoVencimento)
            .IsRequired();

        builder
            .Property(u => u.DataCriacao)
            .IsRequired();

        builder
            .Property(u => u.DataUltimaAtualizacao)
            .IsRequired();

        // 🎯 Status (Enum as int)
        builder
            .Property(u => u.Status)
            .HasConversion<int>()
            .IsRequired();

        // 🔗 Relacionamentos
        builder
            .HasMany<CheckIn>()
            .WithOne()
            .HasForeignKey("UsuarioId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany<ContatoEmergencia>()
            .WithOne()
            .HasForeignKey("UsuarioId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany<Notificacao>()
            .WithOne()
            .HasForeignKey("UsuarioId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // 📍 Índices para performance
        builder
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_usuarios_email");
    }
}
