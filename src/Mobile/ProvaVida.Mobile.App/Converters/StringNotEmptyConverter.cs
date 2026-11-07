using System.Globalization;

namespace ProvaVida.Mobile.App.Converters;

/// <summary>
/// Converte uma string para <c>true</c> se não for nula ou vazia.
/// Usado para controlar a visibilidade de mensagens de erro via binding.
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s);

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
