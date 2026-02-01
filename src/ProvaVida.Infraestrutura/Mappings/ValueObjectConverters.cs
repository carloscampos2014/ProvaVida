using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProvaVida.Dominio.ObjetosValor;

namespace ProvaVida.Infraestrutura.Mappings;

/// <summary>
/// Converters globais para Value Objects.
/// Centraliza a conversão entre Value Objects e tipos de banco primitivos (string, int, etc).
/// 
/// Uso em IEntityTypeConfiguration:
/// builder.Property(e => e.Email).HasConversion(ValueObjectConverters.EmailConverter());
/// </summary>
public static class ValueObjectConverters
{
    /// <summary>
    /// Converter para Email: Email → string (Valor) no banco.
    /// </summary>
    public static ValueConverter<Email, string> EmailConverter() =>
        new(
            v => v.Valor,                   // Para banco: Email.Valor
            v => new Email(v)               // Do banco: reconstrói Email
        );

    /// <summary>
    /// Converter para Telefone: Telefone → string (Valor) no banco.
    /// </summary>
    public static ValueConverter<Telefone, string> TelefoneConverter() =>
        new(
            v => v.Valor,                   // Para banco: Telefone.Valor
            v => new Telefone(v)            // Do banco: reconstrói Telefone
        );
}
