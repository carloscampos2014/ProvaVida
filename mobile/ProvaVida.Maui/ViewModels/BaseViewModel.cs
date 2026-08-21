using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProvaVida.Maui.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private int _executando = 0; // 0 = livre, 1 = ocupado — usado pelo ExecutarSeOcioso

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetProperty(ref _errorMessage, value);
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Executa a ação apenas se o ViewModel estiver ocioso.
    /// Usa Interlocked.CompareExchange para garantir atomicidade — dois toques simultâneos
    /// não conseguem ambos avançar, mesmo antes de IsLoading propagar para a UI.
    /// </summary>
    protected async Task ExecutarSeOcioso(Func<Task> acao)
    {
        if (Interlocked.CompareExchange(ref _executando, 1, 0) != 0) return;
        IsLoading = true;
        try
        {
            await acao();
        }
        finally
        {
            IsLoading = false;
            Interlocked.Exchange(ref _executando, 0);
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        SetPropertyInternal(ref field, value, propertyName);
    }

    protected bool SetPropertyInternal<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected void LimparErro() => ErrorMessage = string.Empty;
}
