using ProvaVida.Maui.Models;
using SQLite;

namespace ProvaVida.Maui;

/// <summary>
/// Helpers síncronos para leitura do SQLite local em BroadcastReceivers e AppWidgetProviders.
/// Esses componentes Android não têm contexto async — usam SQLiteConnection síncrono.
/// DataHora é DateTimeOffset serializado como texto com sufixo Z — comparação usa janela UTC.
/// </summary>
internal static class CheckInLocalHelper
{
    private static readonly string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "provavida.db3");

    /// <summary>
    /// Verifica se o usuário fez check-in hoje, comparando em UTC com o dia local do dispositivo.
    /// </summary>
    public static bool FezCheckInHoje()
    {
        try
        {
            if (!File.Exists(DbPath)) return false;

            using var db  = new SQLiteConnection(DbPath);
            var hojeLocal = DateTime.Now.Date;
            var offset    = TimeZoneInfo.Local.GetUtcOffset(hojeLocal);
            var inicio    = new DateTimeOffset(hojeLocal, offset);
            var fim       = new DateTimeOffset(hojeLocal.AddDays(1), offset);

            return db.Table<CheckInLocal>()
                     .Any(c => c.DataHora >= inicio && c.DataHora < fim);
        }
        catch { return false; }
    }

    /// <summary>
    /// Retorna array[7] onde índice 0 = 6 dias atrás, índice 6 = hoje.
    /// Cada posição indica se houve check-in naquele dia local.
    /// </summary>
    public static bool[] ObterCheckInsSemana()
    {
        var resultado = new bool[7];
        try
        {
            if (!File.Exists(DbPath)) return resultado;

            using var db      = new SQLiteConnection(DbPath);
            var hojeLocal     = DateTime.Now.Date;
            var offset        = TimeZoneInfo.Local.GetUtcOffset(hojeLocal);

            for (int i = 0; i < 7; i++)
            {
                var dia   = hojeLocal.AddDays(-(6 - i));
                var inicio = new DateTimeOffset(dia, TimeZoneInfo.Local.GetUtcOffset(dia));
                var fim    = new DateTimeOffset(dia.AddDays(1), TimeZoneInfo.Local.GetUtcOffset(dia.AddDays(1)));
                resultado[i] = db.Table<CheckInLocal>()
                                  .Any(c => c.DataHora >= inicio && c.DataHora < fim);
            }
        }
        catch { }
        return resultado;
    }

    /// <summary>
    /// Retorna o DateTimeOffset do último check-in, ou null se não houver registros.
    /// </summary>
    public static DateTimeOffset? ObterUltimoCheckIn()
    {
        try
        {
            if (!File.Exists(DbPath)) return null;

            using var db = new SQLiteConnection(DbPath);
            var ultimo = db.Table<CheckInLocal>()
                           .OrderByDescending(c => c.DataHora)
                           .FirstOrDefault();
            return ultimo?.DataHora;
        }
        catch { return null; }
    }
}
