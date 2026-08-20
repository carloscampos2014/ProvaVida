using Android.App;
using Android.Content;
using ProvaVida.Maui.Services;

namespace ProvaVida.Maui;

/// <summary>
/// Reagenda os alarmes de notificação após reinicialização do dispositivo.
/// O Android cancela todos os alarmes do AlarmManager e do Plugin.LocalNotification
/// ao reiniciar — este receiver restaura o lembrete das 20h e o aviso de inatividade das 21h.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted },
              Priority = (int)IntentFilterPriority.LowPriority)]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent?.Action != Intent.ActionBootCompleted) return;

        // OnReceive não suporta async — fire-and-forget via método síncrono do serviço
        LocalNotificationService.AgendarLembrete();
        LocalNotificationService.AgendarAvisoInatividade();
    }
}
