namespace ProvaVida.Maui.Storage;

public interface ITokenStorage
{
    Task SalvarAsync(string token);
    Task<string?> ObterAsync();
    Task RemoverAsync();
    Task<bool> ExisteAsync();
}
