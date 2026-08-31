using System.Data;
using Dapper;

namespace ProvaVida.Mobile.Infrastructure.Data;

/// <summary>
/// Handler Dapper para converter <see cref="DateTimeOffset"/> de/para TEXT no SQLite.
/// </summary>
/// <remarks>
/// Necessário porque SQLite não possui um tipo nativo de data/hora com fuso horário.
/// O valor é armazenado como TEXT no formato ISO 8601 (<c>yyyy-MM-ddTHH:mm:ss.fffffffzzz</c>)
/// garantindo preservação do UTC offset.
/// Registrar via <c>SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler())</c> antes de
/// executar qualquer query que envolva os campos <c>criado_em</c> e <c>atualizado_em</c>.
/// </remarks>
public class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    /// <inheritdoc/>
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString("O");
    }

    /// <inheritdoc/>
    public override DateTimeOffset Parse(object value) =>
        value switch
        {
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(dt, TimeSpan.Zero),
            string s => DateTimeOffset.Parse(s),
            _ => DateTimeOffset.Parse(Convert.ToString(value) ?? string.Empty)
        };
}
