using Dapper;
using Microsoft.Data.Sqlite;
using ProvaVida.Maui.Models;

namespace ProvaVida.Maui;

/// <summary>
/// Helpers síncronos para leitura do SQLite local e SecureStorage em BroadcastReceivers e AppWidgetProviders.
/// Esses componentes Android não têm contexto async completo — usam APIs síncronas.
/// </summary>
internal static class CheckInLocalHelper
{
    private static readonly string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "provavida.db3");

    /// <summary>
    /// Verifica se o usuário está autenticado com token não expirado.
    /// Apenas leitura local do SecureStorage — sem chamada à API.
    /// </summary>
    public static bool VerificarAutenticado()
    {
        try
        {
            var token = SecureStorage.Default.GetAsync("auth_token").GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(token)) return false;

            var expiraEmStr = SecureStorage.Default.GetAsync("auth_token_expira_em").GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(expiraEmStr)) return false;

            if (!DateTime.TryParse(expiraEmStr, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var expiraEm))
                return false;

            return expiraEm > DateTime.UtcNow;
        }
        catch { return false; }
    }

    /// <summary>
    /// Verifica se o usuário fez check-in hoje, comparando em UTC com o dia local do dispositivo.
    /// </summary>
    public static bool FezCheckInHoje()
    {
        try
        {
            if (!File.Exists(DbPath)) return false;

            using var db  = new SqliteConnection($"Data Source={DbPath}");
            db.Open();
            // UTC puro — evita ambiguidade de comparação lexicográfica entre offsets
            var inicioUtc = DateTime.UtcNow.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var fimUtc    = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ");

            return db.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM checkins_local WHERE data_hora >= @Inicio AND data_hora < @Fim",
                new { Inicio = inicioUtc, Fim = fimUtc }) > 0;
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

            using var db      = new SqliteConnection($"Data Source={DbPath}");
            db.Open();
            var hojeLocal     = DateTime.Now.Date;

            for (int i = 0; i < 7; i++)
            {
                var dia       = hojeLocal.AddDays(-(6 - i));
                // UTC puro — evita ambiguidade de comparação lexicográfica entre offsets
                var inicioUtc = dia.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var fimUtc    = dia.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                resultado[i] = db.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM checkins_local WHERE data_hora >= @Inicio AND data_hora < @Fim",
                    new { Inicio = inicioUtc, Fim = fimUtc }) > 0;
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

            using var db = new SqliteConnection($"Data Source={DbPath}");
            db.Open();
            var valor = db.ExecuteScalar<string?>(
                "SELECT data_hora FROM checkins_local ORDER BY data_hora DESC LIMIT 1");
            if (valor is null) return null;
            return DateTimeOffset.TryParse(valor, out var dt) ? dt : null;
        }
        catch { return null; }
    }
}
