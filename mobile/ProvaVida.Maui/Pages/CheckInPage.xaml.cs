using ProvaVida.Maui.Services;
using ProvaVida.Maui.ViewModels;

namespace ProvaVida.Maui.Pages;

public partial class CheckInPage : ContentPage
{
    private readonly CheckInViewModel _vm;
    private readonly IHeartbeatService _heartbeatService;
    private readonly SyncService _syncService;

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

        // Solicita permissões proativamente na primeira abertura
        _ = Task.Run(async () =>
        {
            var statusLoc = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (statusLoc != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

#if ANDROID
            // Permissão de notificações obrigatória no Android 13+ (API 33)
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                var statusNotif = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                if (statusNotif != PermissionStatus.Granted)
                    await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
#endif
        });

        // Heartbeat ao abrir a tela — best effort
        _ = Task.Run(() => _heartbeatService.EnviarAsync());

        // Agenda lembrete se ainda não fez check-in hoje
        if (!_vm.FezCheckInHoje)
            LocalNotificationService.AgendarLembrete();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
            _ = Task.Run(() => _syncService.SincronizarAsync());
    }

    private async void OnPerfilTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//perfil");

    private void AtualizarSemanaVisual()
    {
        SemanaLayout.Children.Clear();

        var semana = _vm.Semana;
        var hoje   = DateTime.Now.Date;

        // Nomes dos dias alinhados com DayOfWeek (.NET: 0=Dom, 1=Seg... 6=Sab)
        var nomesDias = new[] { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };

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
                Text = nomesDias[(int)dia.DayOfWeek], // nome correto pelo dia real
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
