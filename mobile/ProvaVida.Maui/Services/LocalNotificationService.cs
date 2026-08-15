namespace ProvaVida.Maui.Services;

/// <summary>
/// Gerencia notificações push locais para lembrete de check-in diário.
/// </summary>
public static class LocalNotificationService
{
    private const int LembreteId = 1001;
    private const int HoraLembrete = 20; // 20h

    /// <summary>
    /// Agenda o lembrete diário de check-in se ainda não foi feito hoje.
    /// </summary>
    public static void AgendarLembrete()
    {
        try
        {
            var agora = DateTime.Now;
            var horario = new DateTime(agora.Year, agora.Month, agora.Day, HoraLembrete, 0, 0);

            // Se já passou das 20h hoje, agenda para amanhã
            if (agora >= horario)
                horario = horario.AddDays(1);

#if ANDROID
            AgendarNotificacaoAndroid(horario);
#endif
        }
        catch { /* ignora se permissão negada */ }
    }

    public static void CancelarLembrete()
    {
        try
        {
#if ANDROID
            CancelarNotificacaoAndroid();
#endif
        }
        catch { }
    }

#if ANDROID
    private static void AgendarNotificacaoAndroid(DateTime horario)
    {
        var context = Android.App.Application.Context;
        var intent  = new Android.Content.Intent(context, typeof(LembreteReceiver));

        // PendingIntentFlags.Immutable exige API 23+
        var flags = Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M
            ? Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable
            : Android.App.PendingIntentFlags.UpdateCurrent;

        var pendingIntent = Android.App.PendingIntent.GetBroadcast(context, LembreteId, intent, flags);
        if (pendingIntent is null) return;

        var alarmManager = context.GetSystemService(Android.Content.Context.AlarmService)
            as Android.App.AlarmManager;
        if (alarmManager is null) return;

        var triggerAtMillis = new DateTimeOffset(horario).ToUnixTimeMilliseconds();

        // SetExactAndAllowWhileIdle exige API 23+
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            alarmManager.SetExactAndAllowWhileIdle(
                Android.App.AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
        else
            alarmManager.SetExact(
                Android.App.AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
    }

    private static void CancelarNotificacaoAndroid()
    {
        var context = Android.App.Application.Context;
        var intent  = new Android.Content.Intent(context, typeof(LembreteReceiver));

        var flags = Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M
            ? Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable
            : Android.App.PendingIntentFlags.UpdateCurrent;

        var pendingIntent = Android.App.PendingIntent.GetBroadcast(context, LembreteId, intent, flags);
        if (pendingIntent is null) return;

        var alarmManager = context.GetSystemService(Android.Content.Context.AlarmService)
            as Android.App.AlarmManager;
        alarmManager?.Cancel(pendingIntent);
    }
#endif
}
