using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;

namespace ProvaVida.Maui;

/// <summary>
/// Widget completo (4x2) — mostra os 7 dias da semana e status do check-in.
/// </summary>
public class CheckInWidgetCompleto : AppWidgetProvider
{
    private static readonly int[] DiaIds =
    {
        Resource.Id.dia_0, Resource.Id.dia_1, Resource.Id.dia_2,
        Resource.Id.dia_3, Resource.Id.dia_4, Resource.Id.dia_5,
        Resource.Id.dia_6
    };

    private static readonly string[] NomesDias = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };

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

        var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(CheckInWidgetCompleto)));
        var ids       = manager.GetAppWidgetIds(component);
        if (ids is null) return;

        foreach (var id in ids)
            AtualizarWidget(context, manager, id);
    }

    private static void AtualizarWidget(Context context, AppWidgetManager manager, int widgetId)
    {
        try
        {
            var autenticado = VerificarAutenticado();
            var semana      = autenticado ? ObterCheckInsSemana() : new bool[7];
            var fezHoje     = semana[6]; // índice 6 = hoje

            var views = new RemoteViews(context.PackageName!, Resource.Layout.widget_completo);

            // Status e label
            if (!autenticado)
            {
                views.SetTextViewText(Resource.Id.widget_completo_status, "");
                views.SetTextViewText(Resource.Id.widget_completo_label, "Toque para fazer login");
            }
            else if (fezHoje)
            {
                views.SetTextViewText(Resource.Id.widget_completo_status, "✅ Feito hoje");
                views.SetTextColor(Resource.Id.widget_completo_status, new Android.Graphics.Color(unchecked((int)0xFF2E9E6B)));
                views.SetTextViewText(Resource.Id.widget_completo_label, "Check-in registrado");
            }
            else
            {
                views.SetTextViewText(Resource.Id.widget_completo_status, "");
                views.SetTextViewText(Resource.Id.widget_completo_label, "Toque para fazer check-in");
            }

            // Dias da semana — colore cada bolinha
            var hoje = DateTime.Today;
            for (int i = 0; i < 7; i++)
            {
                var dia    = hoje.AddDays(-(6 - i));
                var feito  = i < semana.Length && semana[i];
                var ehHoje = i == 6;

                int bgColor;
                if (!autenticado)
                    bgColor = unchecked((int)0xFFE4E0F2);
                else if (feito && ehHoje)
                    bgColor = unchecked((int)0xFF2E9E6B);
                else if (feito)
                    bgColor = unchecked((int)0xFF774CCC);
                else
                    bgColor = unchecked((int)0xFFE4E0F2);

                views.SetTextViewText(DiaIds[i], NomesDias[(int)dia.DayOfWeek]);
                views.SetInt(DiaIds[i], "setBackgroundColor", bgColor);
            }

            // Intent de toque
            var action = autenticado ? "com.enzojb.provavida.ACTION_CHECKIN"
                                     : "com.enzojb.provavida.ACTION_LOGIN";
            var intent = new Intent(action).SetPackage(context.PackageName);
            var pendingFlags = Android.App.PendingIntentFlags.UpdateCurrent |
                               (OperatingSystem.IsAndroidVersionAtLeast(23)
                                   ? Android.App.PendingIntentFlags.Immutable
                                   : 0);
            var pendingIntent = Android.App.PendingIntent.GetActivity(context, widgetId, intent, pendingFlags);
            views.SetOnClickPendingIntent(Resource.Layout.widget_completo, pendingIntent);

            manager.UpdateAppWidget(widgetId, views);
        }
        catch { /* best effort */ }
    }

    private static bool[] ObterCheckInsSemana()
    {
        return CheckInLocalHelper.ObterCheckInsSemana();
    }

    private static bool VerificarAutenticado()
    {
        try
        {
            var token = SecureStorage.Default.GetAsync("auth_token").GetAwaiter().GetResult();
            return !string.IsNullOrWhiteSpace(token);
        }
        catch { return false; }
    }
}
