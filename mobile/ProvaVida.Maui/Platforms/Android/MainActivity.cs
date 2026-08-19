using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace ProvaVida.Maui;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(new[] { "com.enzojb.provavida.ACTION_CHECKIN" },
    Categories = new[] { Intent.CategoryDefault })]
[IntentFilter(new[] { "com.enzojb.provavida.ACTION_LOGIN" },
    Categories = new[] { Intent.CategoryDefault })]
[MetaData("android.app.shortcuts", Resource = "@xml/shortcuts")]
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

        _ = Task.Run(async () =>
        {
            await Task.Delay(800);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (intent.Action == ActionCheckIn)
                        await Shell.Current.GoToAsync("//checkin");
                    else if (intent.Action == ActionLogin)
                        await Shell.Current.GoToAsync("//login");
                }
                catch { }
            });
        });
    }
}
