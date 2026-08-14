namespace ProvaVida.Maui.Services;

/// <summary>
/// Gerencia notificações push locais para lembrete de check-in diário.
/// Usa a API nativa do MAUI via LocalNotification.
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
        var intent = new Android.Content.Intent(
            Android.App.Application.Context,
            typeof(LembreteReceiver));

        var pendingIntent = Android.App.PendingIntent.GetBroadcast(
            Android.App.Application.Context,
            LembreteId,
            intent,
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

        var alarmManager = (Android.App.AlarmManager)Android.App.Application.Context
            .GetSystemService(Android.Content.Context.AlarmService)!;

        var triggerAtMillis = new DateTimeOffset(horario).ToUnixTimeMilliseconds();
        alarmManager.SetExactAndAllowWhileIdle(
            Android.App.AlarmType.RtcWakeup,
            triggerAtMillis,
            pendingIntent!);
    }

    private static void CancelarNotificacaoAndroid()
    {
        var intent = new Android.Content.Intent(
            Android.App.Application.Context,
            typeof(LembreteReceiver));

        var pendingIntent = Android.App.PendingIntent.GetBroadcast(
            Android.App.Application.Context,
            LembreteId,
            intent,
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

        var alarmManager = (Android.App.AlarmManager)Android.App.Application.Context
            .GetSystemService(Android.Content.Context.AlarmService)!;

        alarmManager.Cancel(pendingIntent!);
    }
#endif
}
