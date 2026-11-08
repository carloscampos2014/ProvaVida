using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Contrato para as operações de check-in do app mobile: consulta e registro diário.
/// </summary>
/// <remarks>
/// Implementa estratégia offline-first: o check-in é salvo no SQLite local com
/// <c>Sincronizado=false</c> e, se houver internet, é enviado à API imediatamente.
/// Caso contrário, permanece pendente para sincronização futura.
/// </remarks>
public interface ICheckinMobileService
{
    /// <summary>
    /// Verifica se já existe check-in do dia atual no SQLite local.
    /// </summary>
    /// <returns>
    /// <see cref="Result{T}"/> com <c>Data</c> preenchido se houver check-in hoje;
    /// <c>Data = null</c> se não houver; <c>Fail</c> se não houver usuário logado.
    /// </returns>
    Task<Result<Checkin?>> GetCheckinHojeAsync();

    /// <summary>
    /// Registra o check-in diário: captura localização, salva no SQLite e sincroniza com a API se houver internet.
    /// </summary>
    /// <returns>
    /// <see cref="Result{T}"/> com o <see cref="Checkin"/> criado (ou existente se já havia um hoje);
    /// <c>Fail</c> se a permissão de localização for negada ou se não houver usuário logado.
    /// </returns>
    Task<Result<Checkin>> RegistrarAsync();
}
