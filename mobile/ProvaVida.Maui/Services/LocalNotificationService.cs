using Plugin.LocalNotification;

namespace ProvaVida.Maui.Services;

/// <summary>
/// Gerencia notificações push locais usando Plugin.LocalNotification.
/// Substitui a implementação manual com BroadcastReceiver/AlarmManager
/// que não funcionava corretamente no Android 15/HyperOS.
/// </summary>
public static class LocalNotificationService
{
    private const int LembreteId          = 1001;
    private const int AvisoInatividadeId  = 1002;
    private const int HoraLembrete        = 20; // 20h local
    private const int HoraAvisoInatividade = 21; // 21h local

    /// <summary>
    /// Agenda o lembrete diário de check-in para as 20h.
    /// </summary>
    public static async Task AgendarLembreteAsync()
    {
        try
        {
            var agora   = DateTime.Now;
            var horario = new DateTime(agora.Year, agora.Month, agora.Day, HoraLembrete, 0, 0);
            if (agora >= horario) horario = horario.AddDays(1);

            var notification = new NotificationRequest
            {
                NotificationId = LembreteId,
                Title          = "Está tudo bem com você?",
                Description    = "Não detectamos seu check-in hoje. Toque para confirmar que está bem.",
                Schedule       = new NotificationRequestSchedule
                {
                    NotifyTime     = horario,
                    RepeatType     = NotificationRepeatInterval.Daily,
                    NotifyRepeatInterval = TimeSpan.FromDays(1)
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
        }
        catch { /* ignora se permissão negada */ }
    }

    /// <summary>
    /// Agenda o aviso de inatividade diário para as 21h.
    /// Só dispara se não houver check-in nas últimas 48h (verificado no momento do disparo).
    /// </summary>
    public static async Task AgendarAvisoInatividadeAsync()
    {
        try
        {
            var agora   = DateTime.Now;
            var horario = new DateTime(agora.Year, agora.Month, agora.Day, HoraAvisoInatividade, 0, 0);
            if (agora >= horario) horario = horario.AddDays(1);

            var notification = new NotificationRequest
            {
                NotificationId = AvisoInatividadeId,
                Title          = "Está tudo bem com você? 💙",
                Description    = "Não registramos seu check-in há mais de 48h. Abra o app e confirme que está bem.",
                Schedule       = new NotificationRequestSchedule
                {
                    NotifyTime     = horario,
                    RepeatType     = NotificationRepeatInterval.Daily,
                    NotifyRepeatInterval = TimeSpan.FromDays(1)
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
        }
        catch { /* ignora se permissão negada */ }
    }

    // Mantido para compatibilidade com chamadas existentes
    public static void AgendarLembrete()
        => _ = AgendarLembreteAsync();

    public static void AgendarAvisoInatividade()
        => _ = AgendarAvisoInatividadeAsync();

    public static void CancelarLembrete()
    {
        try { LocalNotificationCenter.Current.Cancel(LembreteId); }
        catch { }
    }
}
