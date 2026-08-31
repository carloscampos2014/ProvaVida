using System.Data;
using Dapper;

namespace ProvaVida.Mobile.Infrastructure.Data;

/// <summary>
/// Handler Dapper para converter <see cref="Guid"/> de/para TEXT no SQLite.
/// </summary>
/// <remarks>
/// Necessário porque SQLite não possui um tipo nativo para GUID.
/// O valor é armazenado como TEXT no formato padrão
/// <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c>.
/// Registrar via <c>SqlMapper.AddTypeHandler(new GuidTypeHandler())</c> antes de
/// executar qualquer query que envolva campos <c>id</c> do tipo GUID.
/// </remarks>
public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    /// <inheritdoc/>
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString();
    }

    /// <inheritdoc/>
    public override Guid Parse(object value) =>
        value switch
        {
            Guid g => g,
            string s => Guid.Parse(s),
            _ => Guid.Parse(Convert.ToString(value) ?? string.Empty)
        };
}
