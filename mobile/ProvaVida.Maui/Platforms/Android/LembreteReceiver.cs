using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace ProvaVida.Maui;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class LembreteReceiver : BroadcastReceiver
{
    private const string ChannelId = "provavida_lembrete";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null) return;

        CriarCanal(context);

        var notificationIntent = context.PackageManager!
            .GetLaunchIntentForPackage(context.PackageName!);

        var pendingIntent = PendingIntent.GetActivity(
            context, 0, notificationIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

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

    private static void CriarCanal(Context context)
    {
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O) return;

        var channel = new NotificationChannel(
            ChannelId,
            "Lembrete de Check-in",
            NotificationImportance.High)
        {
            Description = "Lembrete diário para fazer o check-in de bem-estar"
        };

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        manager.CreateNotificationChannel(channel);
    }
}
