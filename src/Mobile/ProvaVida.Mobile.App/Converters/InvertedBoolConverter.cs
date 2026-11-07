using System.Globalization;

namespace ProvaVida.Mobile.App.Converters;

/// <summary>
/// Inverte um valor booleano. Usado para desabilitar controles enquanto <c>IsLoading</c> é verdadeiro.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}
