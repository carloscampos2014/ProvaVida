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

    private void AtualizarSemanaVisual()
    {
        SemanaLayout.Children.Clear();

        var dias = new[] { "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb", "Dom" };
        var semana = _vm.Semana;

        for (int i = 0; i < 7; i++)
        {
            var feito = i < semana.Count && semana[i];
            var stack = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };

            var dot = new Border
            {
                BackgroundColor = feito
                    ? (Color)Application.Current!.Resources["Primary"]
                    : (Color)Application.Current!.Resources["Border"],
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
                Text = dias[i],
                FontSize = 10,
                FontAttributes = feito ? FontAttributes.Bold : FontAttributes.None,
                TextColor = feito
                    ? (Color)Application.Current!.Resources["Primary"]
                    : (Color)Application.Current!.Resources["TextHint"],
                HorizontalOptions = LayoutOptions.Center
            };

            stack.Children.Add(dot);
            stack.Children.Add(label);
            SemanaLayout.Children.Add(stack);
        }
    }
}
