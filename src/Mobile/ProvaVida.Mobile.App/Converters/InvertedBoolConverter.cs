using System.Globalization;

namespace ProvaVida.Mobile.App.Converters;

/// <summary>
/// Inverte um valor <see cref="bool"/>: <c>true</c> → <c>false</c> e vice-versa.
/// Usado para desabilitar controles enquanto <c>IsLoading</c> é <c>true</c>.
/// </summary>
public sealed class InvertedBoolConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}
