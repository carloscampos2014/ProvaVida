using System.Globalization;

namespace ProvaVida.Maui.Converters;

/// <summary>
/// Converte bool para texto hint abaixo do botão de check-in.
/// </summary>
public class BoolToCheckInHintConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool feito && feito
            ? "Check-in registrado hoje ✓"
            : "Toque para registrar sua presença hoje";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
