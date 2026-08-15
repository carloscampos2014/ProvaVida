using System.Globalization;

namespace ProvaVida.Maui.Converters;

/// <summary>
/// Converte bool (SenhaOculta) para ícone do olho:
/// true (oculta) → 👁 | false (visível) → 🙈
/// </summary>
public class BoolToEyeIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool oculta && oculta ? "👁" : "🙈";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
