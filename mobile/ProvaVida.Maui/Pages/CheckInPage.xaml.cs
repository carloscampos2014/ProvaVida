using Plugin.LocalNotification;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.ViewModels;

namespace ProvaVida.Maui.Pages;

public partial class CheckInPage : ContentPage
{
    private readonly CheckInViewModel _vm;
    private readonly IHeartbeatService _heartbeatService;
    private readonly SyncService _syncService;
    private bool _sincronizando = false; // evita sincronizações simultâneas ao reconectar

    public CheckInPage(CheckInViewModel vm, IHeartbeatService heartbeatService, SyncService syncService)
    {
        InitializeComponent();
        _vm = vm;
        _heartbeatService = heartbeatService;
        _syncService = syncService;
        BindingContext = vm;

        // Retry ao recuperar conexão
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InicializarAsync();
        AtualizarSemanaVisual();

        // Solicita permissão de notificação via Plugin.LocalNotification
        await LocalNotificationCenter.Current.RequestNotificationPermission();

        // Solicita permissões proativamente na primeira abertura do dia
        await SolicitarPermissoesAsync();

        // Heartbeat ao abrir a tela — best effort, falha silenciosa intencional
        _ = Task.Run(async () =>
        {
            try { await _heartbeatService.EnviarAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckInPage] Heartbeat falhou: {ex.Message}");
            }
        });

        // Atualiza App Shortcuts conforme estado atual (Android 7.1+)
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(25))
            AppShortcutsService.Atualizar();
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        // Flag evita sincronizações simultâneas quando a conexão oscila em sequência
        if (e.NetworkAccess != NetworkAccess.Internet || _sincronizando) return;
        _sincronizando = true;
        _ = Task.Run(async () =>
        {
            try   { await _syncService.SincronizarAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckInPage] Sync falhou: {ex.Message}");
            }
            finally { _sincronizando = false; }
        });
    }

    private async void OnHamburgerTapped(object? sender, TappedEventArgs e)
        => Shell.Current.FlyoutIsPresented = true;


    private async void OnPerfilTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//perfil");

    /// <summary>
    /// Solicita permissões de localização e notificações com dialog explicativo (rationale).
    /// Cada permissão é solicitada no máximo uma vez por dia enquanto não for concedida.
    /// </summary>
    private async Task SolicitarPermissoesAsync()
    {
        const string PrefUltimaLocalizacao  = "perm_loc_ultima_solicitacao";
        const string PrefUltimaNotificacao  = "perm_notif_ultima_solicitacao";
        //const string PrefUltimaNotificacao  = "perm_notif_ultima_solicitacao";
        var hoje = DateTime.Today.ToString("yyyy-MM-dd");

        // ── Localização ──────────────────────────────────────────────────────
        var statusLoc = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (statusLoc != PermissionStatus.Granted)
        {
            var ultimaLoc = Preferences.Get(PrefUltimaLocalizacao, string.Empty);
            if (ultimaLoc != hoje)
            {
                Preferences.Set(PrefUltimaLocalizacao, hoje);

                var permitir = await DisplayAlertAsync(
                        "📍 Localização desativada",
                        "A localização é registrada junto com o check-in para confirmar sua presença. O check-in funciona mesmo sem ela.",
                        "Permitir agora",
                        "Mais tarde");

                if (permitir)
                {
                    var resultado = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    // Android não mostra mais o dialog se a permissão foi negada permanentemente.
                    // Nesse caso, a única opção é direcionar para as configurações do app.
                    if (resultado == PermissionStatus.Denied)
                    {
                        var irConfiguracoes = await DisplayAlertAsync(
                            "📍 Permissão necessária",
                            "A permissão de localização foi negada. Para ativar, vá em Configurações > Permissões > Localização.",
                            "Abrir configurações",
                            "Agora não");

                        if (irConfiguracoes)
                            AppInfo.Current.ShowSettingsUI();
                    }
                }
            }
        }

        // ── Notificações (Android 13+) ───────────────────────────────────────
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var statusNotif = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (statusNotif != PermissionStatus.Granted)
            {
                var ultimaNotif = Preferences.Get(PrefUltimaNotificacao, string.Empty);
                if (ultimaNotif != hoje)
                {
                    Preferences.Set(PrefUltimaNotificacao, hoje);

                    var permitir = await DisplayAlertAsync(
                            "🔔 Notificações desativadas",
                            "O ProvaVida precisa enviar lembretes diários de check-in. Sem isso, você pode esquecer e acionar o alerta de emergência sem querer.",
                            "Permitir agora",
                            "Mais tarde");

                    if (permitir)
                    {
                        var resultado = await Permissions.RequestAsync<Permissions.PostNotifications>();
                        if (resultado == PermissionStatus.Denied)
                        {
                            var irConfiguracoes = await DisplayAlertAsync(
                                "🔔 Permissão necessária",
                                "A permissão de notificações foi negada. Para ativar, vá em Configurações > Permissões > Notificações.",
                                "Abrir configurações",
                                "Agora não");

                            if (irConfiguracoes)
                                AppInfo.Current.ShowSettingsUI();
                        }
                    }
                }
            }
        }

        // ── Alarmes exatos (Android 12+) — necessário para notificações no horário certo ─
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            var alarmManager = Android.App.Application.Context
                .GetSystemService(Android.Content.Context.AlarmService) as Android.App.AlarmManager;

            if (alarmManager != null && !alarmManager.CanScheduleExactAlarms())
            {
                var ultimaAlarme = Preferences.Get("perm_alarme_ultima_solicitacao", string.Empty);
                if (ultimaAlarme != hoje)
                {
                    Preferences.Set("perm_alarme_ultima_solicitacao", hoje);

                    var ir = await DisplayAlertAsync(
                        "⏰ Permissão de alarme necessária",
                        "Para receber o lembrete exatamente às 20h, o ProvaVida precisa de permissão para alarmes precisos. Toque em Ir para ativar nas configurações.",
                        "Ir para configurações",
                        "Agora não");

                    if (ir)
                    {
                        var intent = new Android.Content.Intent(
                            Android.Provider.Settings.ActionRequestScheduleExactAlarm);
                        intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                        Android.App.Application.Context.StartActivity(intent);
                    }
                }
            }
        }
#endif
    }

    private void AtualizarSemanaVisual()
    {
        SemanaLayout.Children.Clear();

        var semana = _vm.Semana;
        var hoje   = DateTime.Now.Date;

        for (int i = 0; i < 7; i++)
        {
            var dia   = hoje.AddDays(-(6 - i));   // mesmo cálculo do ViewModel
            var feito = i < semana.Count && semana[i];
            var ehHoje = dia == hoje;

            var stack = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };

            // Cor do círculo: verde se feito hoje, roxo nos outros dias feitos, cinza se não feito
            Color corCirculo;
            if (feito && ehHoje)
                corCirculo = Colors.Green;
            else if (feito)
                corCirculo = (Color)Application.Current!.Resources["Primary"];
            else
                corCirculo = (Color)Application.Current!.Resources["Border"];

            var dot = new Border
            {
                BackgroundColor = corCirculo,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 17 },
                StrokeThickness = 0,
                WidthRequest = 34,
                HeightRequest = 34
            };

            if (feito)
            {
                dot.Content = new Label
                {
                    Text = "✓",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
            }

            var label = new Label
            {
                Text = dia.ToString("dd/MM"),
                FontSize = 10,
                FontAttributes = feito ? FontAttributes.Bold : FontAttributes.None,
                TextColor = feito
                    ? (ehHoje ? Colors.Green : (Color)Application.Current!.Resources["Primary"])
                    : (Color)Application.Current!.Resources["TextHint"],
                HorizontalOptions = LayoutOptions.Center
            };

            stack.Children.Add(dot);
            stack.Children.Add(label);
            SemanaLayout.Children.Add(stack);
        }
    }
}
