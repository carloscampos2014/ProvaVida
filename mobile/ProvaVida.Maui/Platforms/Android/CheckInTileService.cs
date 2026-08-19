using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Service.QuickSettings;
using ProvaVida.Maui.Models;
using SQLite;
using System.Runtime.Versioning;

namespace ProvaVida.Maui;

/// <summary>
/// Quick Settings Tile para check-in rápido.
/// Exibido na área de configurações rápidas do Android (onde ficam Wi-Fi, Bluetooth, etc.).
/// Requer Android 7.0+ (API 24).
/// </summary>
[Service(
    Label = "ProvaVida",
    Icon  = "@mipmap/appicon",
    Exported = true,
    Permission = "android.permission.BIND_QUICK_SETTINGS_TILE")]
[IntentFilter(new[] { ActionQsTile })]
[SupportedOSPlatform("android24.0")]
public class CheckInTileService : TileService
{
    public override void OnStartListening()
    {
        base.OnStartListening();
        AtualizarTile();
    }

    public override void OnTileAdded()
    {
        base.OnTileAdded();
        AtualizarTile();
    }

    public override void OnClick()
    {
        base.OnClick();

        var autenticado = VerificarAutenticado();
        var action      = autenticado ? "com.enzojb.provavida.ACTION_CHECKIN"
                                      : "com.enzojb.provavida.ACTION_LOGIN";

        var intent = new Intent(action)
            .SetPackage(PackageName)
            .SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

        StartActivityAndCollapse(intent);
    }

    private void AtualizarTile()
    {
        try
        {
            var tile         = QsTile;
            if (tile is null) return;

            var autenticado  = VerificarAutenticado();
            var fezCheckIn   = autenticado && VerificarCheckInHoje();

            if (!autenticado)
            {
                tile.Label       = "ProvaVida — Login";
                tile.ContentDescription = "Toque para fazer login";
                tile.State       = TileState.Inactive;
            }
            else if (fezCheckIn)
            {
                tile.Label       = "Check-in feito ✓";
                tile.ContentDescription = "Check-in registrado hoje";
                tile.State       = TileState.Active;
            }
            else
            {
                tile.Label       = "Fazer Check-in";
                tile.ContentDescription = "Toque para registrar seu check-in";
                tile.State       = TileState.Inactive;
            }

            tile.UpdateTile();
        }
        catch { /* ignora — best effort */ }
    }

    private static bool VerificarAutenticado()
    {
        try
        {
            var token = SecureStorage.Default.GetAsync("jwt_token").GetAwaiter().GetResult();
            return !string.IsNullOrWhiteSpace(token);
        }
        catch { return false; }
    }

    private static bool VerificarCheckInHoje()
    {
        try
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "provavida.db3");

            if (!File.Exists(dbPath)) return false;

            using var db   = new SQLiteConnection(dbPath);
            var hoje       = DateTime.Today;
            var amanha     = hoje.AddDays(1);

            return db.Table<CheckInLocal>()
                     .Any(c => c.DataHora >= hoje && c.DataHora < amanha);
        }
        catch { return false; }
    }
}
