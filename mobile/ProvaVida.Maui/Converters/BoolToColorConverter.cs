using System.Globalization;

namespace ProvaVida.Maui.Converters;

/// <summary>
/// Converte bool para cor do botão de check-in:
/// true (feito hoje) → verde | false → roxo primário
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool feito && feito)
            return Colors.SeaGreen;

        return Application.Current!.Resources["Primary"] as Color
               ?? Colors.Purple;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
