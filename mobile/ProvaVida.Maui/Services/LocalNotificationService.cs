namespace ProvaVida.Maui.Services;

/// <summary>
/// Gerencia notificações push locais para lembrete de check-in diário.
/// </summary>
public static class LocalNotificationService
{
    private const int LembreteId       = 1001;
    private const int AvisoInatividadeId = 1002;
    private const int HoraLembrete     = 20; // 20h
    private const int HoraAvisoInatividade = 21; // 21h

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
            AgendarNotificacaoAndroid(horario, LembreteId, typeof(LembreteReceiver));
#endif
        }
        catch { /* ignora se permissão negada */ }
    }

    /// <summary>
    /// Agenda o aviso de inatividade diário para as 21h.
    /// Verifica localmente se há check-in nas últimas 48h antes de disparar.
    /// </summary>
    public static void AgendarAvisoInatividade()
    {
        try
        {
            var agora = DateTime.Now;
            var horario = new DateTime(agora.Year, agora.Month, agora.Day, HoraAvisoInatividade, 0, 0);

            // Se já passou das 21h hoje, agenda para amanhã
            if (agora >= horario)
                horario = horario.AddDays(1);

#if ANDROID
            AgendarNotificacaoAndroid(horario, AvisoInatividadeId, typeof(AvisoInatividadeReceiver));
#endif
        }
        catch { /* ignora se permissão negada */ }
    }

    public static void CancelarLembrete()
    {
        try
        {
#if ANDROID
            CancelarNotificacaoAndroid(LembreteId, typeof(LembreteReceiver));
#endif
        }
        catch { }
    }

#if ANDROID
    private static void AgendarNotificacaoAndroid(DateTime horario, int requestCode, Type receiverType)
    {
        var context = Android.App.Application.Context;
        var intent  = new Android.Content.Intent(context, receiverType);

        Android.App.PendingIntentFlags flags;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            flags = Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable;
        else
            flags = Android.App.PendingIntentFlags.UpdateCurrent;

        var pendingIntent = Android.App.PendingIntent.GetBroadcast(context, requestCode, intent, flags);
        if (pendingIntent is null) return;

        var alarmManager = context.GetSystemService(Android.Content.Context.AlarmService)
            as Android.App.AlarmManager;
        if (alarmManager is null) return;

        var triggerAtMillis = new DateTimeOffset(horario).ToUnixTimeMilliseconds();

        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            alarmManager.SetExactAndAllowWhileIdle(
                Android.App.AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
        else
            alarmManager.SetExact(
                Android.App.AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
    }

    private static void CancelarNotificacaoAndroid(int requestCode, Type receiverType)
    {
        var context = Android.App.Application.Context;
        var intent  = new Android.Content.Intent(context, receiverType);

        Android.App.PendingIntentFlags flags;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            flags = Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable;
        else
            flags = Android.App.PendingIntentFlags.UpdateCurrent;

        var pendingIntent = Android.App.PendingIntent.GetBroadcast(context, requestCode, intent, flags);
        if (pendingIntent is null) return;

        var alarmManager = context.GetSystemService(Android.Content.Context.AlarmService)
            as Android.App.AlarmManager;
        alarmManager?.Cancel(pendingIntent);
    }
#endif
}
