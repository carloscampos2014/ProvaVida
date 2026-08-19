using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using ProvaVida.Maui.Models;
using SQLite;

namespace ProvaVida.Maui;

/// <summary>
/// Widget simples (2x1) — mostra status e permite acesso rápido ao app.
/// </summary>
[BroadcastReceiver(Label = "ProvaVida — Simples", Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[MetaData("android.appwidget.provider", Resource = "@xml/widget_simples_info")]
public class CheckInWidgetSimples : AppWidgetProvider
{
    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context is null || appWidgetManager is null || appWidgetIds is null) return;

        foreach (var id in appWidgetIds)
            AtualizarWidget(context, appWidgetManager, id);
    }

    public static void AtualizarTodos(Context context)
    {
        var manager = AppWidgetManager.GetInstance(context);
        if (manager is null) return;

        var component  = new ComponentName(context, Java.Lang.Class.FromType(typeof(CheckInWidgetSimples)));
        var ids        = manager.GetAppWidgetIds(component);
        if (ids is null) return;

        foreach (var id in ids)
            AtualizarWidget(context, manager, id);
    }

    private static void AtualizarWidget(Context context, AppWidgetManager manager, int widgetId)
    {
        try
        {
            var autenticado = VerificarAutenticado();
            var fezCheckIn  = autenticado && VerificarCheckInHoje();

            var views = new RemoteViews(context.PackageName!, Resource.Layout.widget_simples);

            string status, label;
            int statusColor;

            if (!autenticado)
            {
                status     = "🔒 ProvaVida";
                label      = "Toque para fazer login";
                statusColor = unchecked((int)0xFF774CCC);
            }
            else if (fezCheckIn)
            {
                status     = "✅ Feito hoje!";
                label      = "Check-in registrado";
                statusColor = unchecked((int)0xFF2E9E6B);
            }
            else
            {
                status     = "💙 ProvaVida";
                label      = "Toque para fazer check-in";
                statusColor = unchecked((int)0xFF774CCC);
            }

            views.SetTextViewText(Resource.Id.widget_simples_status, status);
            views.SetTextViewText(Resource.Id.widget_simples_label, label);
            views.SetTextColor(Resource.Id.widget_simples_status, new Android.Graphics.Color(statusColor));

            // Intent de toque — abre check-in ou login
            var action = autenticado ? "com.enzojb.provavida.ACTION_CHECKIN"
                                     : "com.enzojb.provavida.ACTION_LOGIN";
            var intent = new Intent(action).SetPackage(context.PackageName);
            var pendingFlags = Android.App.PendingIntentFlags.UpdateCurrent |
                               (OperatingSystem.IsAndroidVersionAtLeast(23)
                                   ? Android.App.PendingIntentFlags.Immutable
                                   : 0);
            var pendingIntent = Android.App.PendingIntent.GetActivity(context, widgetId, intent, pendingFlags);
            views.SetOnClickPendingIntent(Resource.Layout.widget_simples, pendingIntent);

            manager.UpdateAppWidget(widgetId, views);
        }
        catch { /* best effort */ }
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

            using var db = new SQLiteConnection(dbPath);
            var hoje     = DateTime.Today;
            return db.Table<CheckInLocal>().Any(c => c.DataHora >= hoje && c.DataHora < hoje.AddDays(1));
        }
        catch { return false; }
    }
}
