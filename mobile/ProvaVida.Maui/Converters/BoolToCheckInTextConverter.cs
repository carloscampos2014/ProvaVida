using System.Globalization;

namespace ProvaVida.Maui.Converters;

/// <summary>
/// Converte bool para texto do botão de check-in:
/// true → "Feito hoje" | false → "Check-in"
/// </summary>
public class BoolToCheckInTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool feito && feito ? "Feito hoje" : "Check-in";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
