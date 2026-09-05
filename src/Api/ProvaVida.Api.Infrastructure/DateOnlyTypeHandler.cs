using System.Data;
using Dapper;

namespace ProvaVida.Api.Infrastructure;

/// <summary>
/// Handler Dapper para converter <see cref="DateOnly"/> de/para <c>DATE</c> no PostgreSQL.
/// </summary>
/// <remarks>
/// Necessário porque Dapper não possui suporte nativo a <see cref="DateOnly"/>.
/// Registrar via <c>SqlMapper.AddTypeHandler(new DateOnlyTypeHandler())</c> antes de
/// executar qualquer query que envolva a propriedade <c>Data</c> do check-in.
/// </remarks>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    /// <inheritdoc/>
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    /// <inheritdoc/>
    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateTime dt => DateOnly.FromDateTime(dt),
            DateOnly d => d,
            _ => DateOnly.FromDateTime(Convert.ToDateTime(value))
        };
    }
}
