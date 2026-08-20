using Android.App;
using Android.Content;
using AndroidX.Core.App;
using System.Runtime.Versioning;

namespace ProvaVida.Maui;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class LembreteReceiver : BroadcastReceiver
{
    private const string ChannelId = "provavida_lembrete";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null) return;

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
            CriarCanal(context);

        var packageManager = context.PackageManager;
        if (packageManager is null) return;

        var packageName = context.PackageName;
        if (packageName is null) return;

        var notificationIntent = packageManager.GetLaunchIntentForPackage(packageName);
        if (notificationIntent is null) return;

        PendingIntentFlags flags;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            flags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable;
        else
            flags = PendingIntentFlags.UpdateCurrent;

        var pendingIntent = PendingIntent.GetActivity(context, 0, notificationIntent, flags);
        if (pendingIntent is null) return;

        var notification = new NotificationCompat.Builder(context, ChannelId)
            .SetContentTitle("Está tudo bem com você?")
            .SetContentText("Não detectamos seu check-in hoje. Toque para confirmar que está bem.")
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
            .SetContentIntent(pendingIntent)
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityHigh)
            .Build();

        var manager = NotificationManagerCompat.From(context);
        manager.Notify(1001, notification);
    }

    [SupportedOSPlatform("android26.0")]
    private static void CriarCanal(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26)) return;

        var channel = new NotificationChannel(
            ChannelId,
            "Lembrete de Check-in",
            NotificationImportance.High)
        {
            Description = "Lembrete diário para fazer o check-in de bem-estar"
        };

        var manager = context.GetSystemService(Context.NotificationService) as NotificationManager;
        manager?.CreateNotificationChannel(channel);
    }
}
