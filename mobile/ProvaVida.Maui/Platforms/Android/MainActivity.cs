using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace ProvaVida.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const string ActionCheckIn = "com.enzojb.provavida.ACTION_CHECKIN";
    private const string ActionLogin   = "com.enzojb.provavida.ACTION_LOGIN";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        TratarShortcutIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        TratarShortcutIntent(intent);
    }

    private static void TratarShortcutIntent(Intent? intent)
    {
        if (intent?.Action is null) return;

        // Aguarda o Shell estar pronto antes de navegar
        _ = Task.Run(async () =>
        {
            await Task.Delay(800); // tempo para o Shell inicializar
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (intent.Action == ActionCheckIn)
                        await Shell.Current.GoToAsync("//checkin");
                    else if (intent.Action == ActionLogin)
                        await Shell.Current.GoToAsync("//login");
                }
                catch { /* ignora se Shell não estiver pronto */ }
            });
        });
    }
}
