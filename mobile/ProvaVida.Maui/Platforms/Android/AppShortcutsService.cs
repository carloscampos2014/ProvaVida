using Android.Content;
using Android.Content.PM;
using System.Runtime.Versioning;

namespace ProvaVida.Maui;

/// <summary>
/// Gerencia os App Shortcuts dinâmicos exibidos ao segurar o ícone do ProvaVida.
/// Requer Android 7.1+ (API 25).
/// </summary>
[SupportedOSPlatform("android25.0")]
public static class AppShortcutsService
{
    private const string ActionCheckIn = "com.enzojb.provavida.ACTION_CHECKIN";
    private const string ActionLogin   = "com.enzojb.provavida.ACTION_LOGIN";

    /// <summary>
    /// Atualiza os shortcuts dinâmicos conforme o estado atual:
    /// - Não autenticado → shortcut de Login
    /// - Autenticado + check-in feito → shortcut "Feito hoje" (desabilitado)
    /// - Autenticado + sem check-in → shortcut "Fazer Check-in"
    /// </summary>
    public static void Atualizar()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(25)) return;

        try
        {
            var context      = Android.App.Application.Context;
            var manager      = context.GetSystemService(Context.ShortcutService)
                               as Android.Content.PM.ShortcutManager;
            if (manager is null) return;

            var autenticado  = VerificarAutenticado();
            var fezCheckIn   = autenticado && VerificarCheckInHoje();

            var shortcuts    = new List<ShortcutInfo>();

            if (!autenticado)
            {
                shortcuts.Add(CriarShortcut(context, "login", ActionLogin,
                    "Login", "Fazer Login no ProvaVida",
                    Android.Resource.Drawable.IcMenuMyCalendar));
            }
            else if (fezCheckIn)
            {
                var info = new ShortcutInfo.Builder(context, "checkin_feito")
                    .SetShortLabel("✅ Feito hoje")
                    .SetLongLabel("Check-in registrado hoje")
                    .SetIcon(Android.Graphics.Drawables.Icon.CreateWithResource(
                        context, Android.Resource.Drawable.IcMenuAgenda))
                    .SetIntent(new Intent(ActionCheckIn).SetPackage(context.PackageName))
                    .SetDisabledMessage("Check-in já feito hoje")
                    .Build();
                shortcuts.Add(info);
                manager.SetDynamicShortcuts(shortcuts);
                // Desabilita o shortcut pois já foi feito
                manager.DisableShortcuts(new[] { "checkin_feito" });
                return;
            }
            else
            {
                shortcuts.Add(CriarShortcut(context, "checkin", ActionCheckIn,
                    "💙 Check-in", "Fazer Check-in agora",
                    Android.Resource.Drawable.IcMenuSend));
            }

            manager.SetDynamicShortcuts(shortcuts);
        }
        catch { /* ignora — shortcuts são best effort */ }
    }

    private static ShortcutInfo CriarShortcut(
        Context context, string id, string action,
        string shortLabel, string longLabel, int iconRes)
    {
        var intent = new Intent(action)
            .SetPackage(context.PackageName)
            .SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

        return new ShortcutInfo.Builder(context, id)
            .SetShortLabel(shortLabel)
            .SetLongLabel(longLabel)
            .SetIcon(Android.Graphics.Drawables.Icon.CreateWithResource(context, iconRes))
            .SetIntent(intent)
            .Build();
    }

    private static bool VerificarAutenticado()
    {
        try
        {
            // Verifica se há token salvo no SecureStorage
            var token = SecureStorage.Default.GetAsync("auth_token").GetAwaiter().GetResult();
            return !string.IsNullOrWhiteSpace(token);
        }
        catch { return false; }
    }

    private static bool VerificarCheckInHoje()
    {
        return CheckInLocalHelper.FezCheckInHoje();
    }
}
