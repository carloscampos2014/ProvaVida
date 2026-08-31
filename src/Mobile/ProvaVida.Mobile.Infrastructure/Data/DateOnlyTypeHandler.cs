using System.Data;
using Dapper;

namespace ProvaVida.Mobile.Infrastructure.Data;

/// <summary>
/// Handler Dapper para converter <see cref="DateOnly"/> de/para TEXT no SQLite.
/// </summary>
/// <remarks>
/// Necessário porque SQLite armazena datas como TEXT no formato <c>yyyy-MM-dd</c>.
/// Dapper não possui suporte nativo a <see cref="DateOnly"/>.
/// Registrar via <c>SqlMapper.AddTypeHandler(new DateOnlyTypeHandler())</c> antes de
/// executar qualquer query que envolva a propriedade <c>Data</c> do check-in.
/// </remarks>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    /// <inheritdoc/>
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString("yyyy-MM-dd");
    }

    /// <inheritdoc/>
    public override DateOnly Parse(object value) =>
        value switch
        {
            DateOnly d => d,
            DateTime dt => DateOnly.FromDateTime(dt),
            string s => DateOnly.Parse(s),
            _ => DateOnly.FromDateTime(Convert.ToDateTime(value))
        };
}
