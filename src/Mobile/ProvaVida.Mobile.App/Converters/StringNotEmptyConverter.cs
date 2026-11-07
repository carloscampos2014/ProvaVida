using System.Globalization;

namespace ProvaVida.Mobile.App.Converters;

/// <summary>
/// Converte uma <see cref="string"/> em <see cref="bool"/>:
/// retorna <c>true</c> quando a string não é nula nem vazia.
/// Usado para controlar a visibilidade do painel de erro no formulário de login.
/// </summary>
public sealed class StringNotEmptyConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s);

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
