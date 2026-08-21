using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
namespace ProvaVida.Maui.Services;

/// <summary>
/// Gerencia notificações push locais usando Plugin.LocalNotification.
/// Os alarmes são agendados uma vez no login e se repetem diariamente via AlarmManager.
/// O BootReceiver reagenda após reinicialização do dispositivo.
/// </summary>
public static class LocalNotificationService
{
    private const int LembreteId           = 1001;
    private const int AvisoInatividadeId   = 1002;
    private const int HoraLembrete         = 20; // 20h local
    private const int HoraAvisoInatividade = 21; // 21h local

    /// <summary>
    /// Agenda o lembrete diário de check-in para as 20h.
    /// Deve ser chamado no login — RepeatType.Daily mantém ativo indefinidamente.
    /// </summary>
    public static async Task AgendarLembreteAsync()
    {
        try
        {
            var temPermissao = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            if (!temPermissao) return;

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
                    NotifyTime = horario,
                    RepeatType = NotificationRepeat.Daily,
                    Android    = new AndroidScheduleOptions
                    {
                        AlarmType = AndroidAlarmType.RtcWakeup
                    }
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
            System.Diagnostics.Debug.WriteLine($"[Notif] Lembrete agendado para {horario:HH:mm} (diário)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Notif] Falha ao agendar lembrete: {ex.Message}");
        }
    }

    /// <summary>
    /// Agenda o aviso de inatividade diário para as 21h.
    /// Deve ser chamado no login — RepeatType.Daily mantém ativo indefinidamente.
    /// </summary>
    public static async Task AgendarAvisoInatividadeAsync()
    {
        try
        {
            var temPermissao = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            if (!temPermissao) return;

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
                    NotifyTime = horario,
                    RepeatType = NotificationRepeat.Daily,
                    Android    = new AndroidScheduleOptions
                    {
                        AlarmType = AndroidAlarmType.RtcWakeup
                    }
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
            System.Diagnostics.Debug.WriteLine($"[Notif] Aviso inatividade agendado para {horario:HH:mm} (diário)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Notif] Falha ao agendar aviso inatividade: {ex.Message}");
        }
    }

    // Compatibilidade com chamadas existentes síncronas (BootReceiver)
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
