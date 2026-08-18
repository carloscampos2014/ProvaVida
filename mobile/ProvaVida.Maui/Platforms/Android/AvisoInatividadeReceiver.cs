using Android.App;
using Android.Content;
using AndroidX.Core.App;
using ProvaVida.Maui.Models;
using SQLite;

namespace ProvaVida.Maui;

/// <summary>
/// BroadcastReceiver agendado para as 21h.
/// Verifica o SQLite local — se o último check-in for há mais de 48h, dispara
/// notificação de aviso de inatividade para o próprio usuário.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public class AvisoInatividadeReceiver : BroadcastReceiver
{
    private const string ChannelId  = "provavida_inatividade";
    private const int    NotifId    = 1002;
    private const int    HorasLimite = 48;

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null) return;

        try
        {
            if (!DevePararNotificacao()) return;

            CriarCanal(context);

            var packageManager = context.PackageManager;
            var packageName    = context.PackageName;
            if (packageManager is null || packageName is null) return;

            var notificationIntent = packageManager.GetLaunchIntentForPackage(packageName);

            var flags = Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;

            var pendingIntent = PendingIntent.GetActivity(context, NotifId, notificationIntent, flags);

            var notification = new NotificationCompat.Builder(context, ChannelId)
                .SetContentTitle("Está tudo bem com você? 💙")
                .SetContentText($"Não registramos seu check-in há mais de {HorasLimite}h. Abra o app e confirme que está bem.")
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityMax)
                .Build();

            var manager = NotificationManagerCompat.From(context);
            manager.Notify(NotifId, notification);
        }
        catch { /* ignora — best effort */ }
    }

    /// <summary>
    /// Retorna true se o último check-in local for há mais de 48h (ou nunca houve).
    /// </summary>
    private static bool DevePararNotificacao()
    {
        try
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "provavida.db3");

            if (!File.Exists(dbPath)) return true; // banco não existe = nunca fez check-in

            using var db = new SQLiteConnection(dbPath);
            var ultimo = db.Table<CheckInLocal>()
                           .OrderByDescending(c => c.DataHora)
                           .FirstOrDefault();

            if (ultimo is null) return true; // nenhum check-in local

            return (DateTime.UtcNow - ultimo.DataHora.ToUniversalTime()).TotalHours >= HorasLimite;
        }
        catch
        {
            return false; // em caso de erro, não dispara
        }
    }

    private static void CriarCanal(Context context)
    {
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O) return;

        var channel = new NotificationChannel(
            ChannelId,
            "Aviso de Inatividade",
            NotificationImportance.Max)
        {
            Description = "Notificação quando o usuário não faz check-in por mais de 48 horas"
        };

        var manager = context.GetSystemService(Context.NotificationService) as NotificationManager;
        manager?.CreateNotificationChannel(channel);
    }
}
