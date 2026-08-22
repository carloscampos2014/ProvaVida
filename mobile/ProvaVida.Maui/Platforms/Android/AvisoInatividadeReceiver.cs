using Android.App;
using Android.Content;
using AndroidX.Core.App;
using System.Runtime.Versioning;

namespace ProvaVida.Maui;

/// <summary>
/// BroadcastReceiver agendado para as 21h.
/// Verifica o SQLite local — se o último check-in for há mais de 48h, dispara
/// notificação de aviso de inatividade para o próprio usuário.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public class AvisoInatividadeReceiver : BroadcastReceiver
{
    private const string ChannelId   = "provavida_inatividade";
    private const int    NotifId     = 1002;
    private const int    HorasLimite = 48;

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null) return;

        // GoAsync libera a main thread — operações de I/O (SQLite) rodam em background.
        // Finish() avisa o Android que o trabalho terminou, evitando ANR.
        var pendingResult = GoAsync();
        Task.Run(async () =>
        {
            try { await ProcessarAsync(context); }
            catch { /* ignora — best effort */ }
            finally { pendingResult?.Finish(); }
        });
    }

    private static async Task ProcessarAsync(Context context)
    {
        try
        {
            if (!DeveDispararNotificacao()) return;

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
                CriarCanal(context);

            var packageManager = context.PackageManager;
            var packageName    = context.PackageName;
            if (packageManager is null || packageName is null) return;

            var notificationIntent = packageManager.GetLaunchIntentForPackage(packageName);
            if (notificationIntent is null) return;

            PendingIntentFlags flags;
            if (OperatingSystem.IsAndroidVersionAtLeast(23))
                flags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable;
            else
                flags = PendingIntentFlags.UpdateCurrent;

            var pendingIntent = PendingIntent.GetActivity(context, NotifId, notificationIntent, flags);
            if (pendingIntent is null) return;

#pragma warning disable CS8602
            var notification = new NotificationCompat.Builder(context, ChannelId)
                .SetContentTitle("Está tudo bem com você? 💙")
                .SetContentText($"Não registramos seu check-in há mais de {HorasLimite}h. Abra o app e confirme que está bem.")
                .SetSmallIcon(Resource.Drawable.ic_notificacao)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityMax)
                .Build()!;
#pragma warning restore CS8602

            var manager = NotificationManagerCompat.From(context);
            manager?.Notify(NotifId, notification);
        }
        catch { /* ignora — best effort */ }

        await Task.CompletedTask;
    }

    private static bool DeveDispararNotificacao()
    {
        try
        {
            var ultimo = CheckInLocalHelper.ObterUltimoCheckIn();
            if (ultimo is null) return true;

            return (DateTimeOffset.UtcNow - ultimo.Value).TotalHours >= HorasLimite;
        }
        catch
        {
            return false;
        }
    }

    [SupportedOSPlatform("android26.0")]
    private static void CriarCanal(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26)) return;

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
