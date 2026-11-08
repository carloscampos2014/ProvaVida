using System.Net;
using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Http;
using ProvaVida.Mobile.Application.Services;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Dtos;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Implementação de <see cref="ICheckinMobileService"/> com estratégia offline-first.
/// </summary>
/// <remarks>
/// <para>
/// Fluxo de <see cref="RegistrarAsync"/>:
/// <list type="number">
///   <item>Captura localização via <see cref="ILocationService"/> — falha se permissão negada.</item>
///   <item>Obtém usuário da sessão local via <see cref="ISessionManager"/>.</item>
///   <item>Verifica duplicata no SQLite — retorna existente se já houver check-in hoje.</item>
///   <item>Monta e salva o <see cref="Checkin"/> no SQLite com <c>Sincronizado=false</c>.</item>
///   <item>Se tiver internet, envia à API: sucesso → <c>Sincronizado=true</c>;
///         409 → <c>Sincronizado=true</c>; outros erros → mantém <c>false</c>.</item>
/// </list>
/// </para>
/// </remarks>
public class CheckinMobileService : ICheckinMobileService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICheckinRepository _checkinRepository;
    private readonly ILocationService _locationService;
    private readonly IConnectivityService _connectivityService;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly IApiClient _apiClient;
    private readonly ILogger<CheckinMobileService> _logger;

    /// <summary>
    /// Inicializa o serviço com suas dependências.
    /// </summary>
    /// <param name="sessionManager">Gerenciador de sessão local (SQLite).</param>
    /// <param name="checkinRepository">Repositório de check-ins no SQLite.</param>
    /// <param name="locationService">Serviço de localização GPS.</param>
    /// <param name="connectivityService">Serviço de verificação de conectividade.</param>
    /// <param name="deviceInfoService">Serviço de informações do dispositivo.</param>
    /// <param name="apiClient">Cliente HTTP para comunicação com a API ProvaVida.</param>
    /// <param name="logger">Logger para diagnóstico.</param>
    public CheckinMobileService(
        ISessionManager sessionManager,
        ICheckinRepository checkinRepository,
        ILocationService locationService,
        IConnectivityService connectivityService,
        IDeviceInfoService deviceInfoService,
        IApiClient apiClient,
        ILogger<CheckinMobileService> logger)
    {
        _sessionManager = sessionManager;
        _checkinRepository = checkinRepository;
        _locationService = locationService;
        _connectivityService = connectivityService;
        _deviceInfoService = deviceInfoService;
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<Checkin?>> GetCheckinHojeAsync()
    {
        var usuario = await _sessionManager.GetCurrentUserAsync();
        if (usuario is null)
            return Result<Checkin?>.Fail("Nenhum usuário logado.");

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var resultado = await _checkinRepository.GetByUsuarioIdAndDataAsync(usuario.Id, hoje);

        // Repositório retorna Fail quando não encontrado — isso é OK, significa "sem check-in hoje"
        return resultado.Success
            ? Result<Checkin?>.Ok(resultado.Data)
            : Result<Checkin?>.Ok(null);
    }

    /// <inheritdoc/>
    public async Task<Result<Checkin>> RegistrarAsync()
    {
        // 1. Capturar localização — falha se permissão negada ou timeout
        var localizacaoResult = await _locationService.GetCurrentLocationAsync();
        if (!localizacaoResult.Success)
            return Result<Checkin>.Fail(localizacaoResult.MessageErro);

        // 2. Obter usuário da sessão local
        var usuario = await _sessionManager.GetCurrentUserAsync();
        if (usuario is null)
            return Result<Checkin>.Fail("Nenhum usuário logado.");

        // 3. Verificar duplicata no SQLite — retornar existente se já houver check-in hoje
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var checkinExistente = await _checkinRepository.GetByUsuarioIdAndDataAsync(usuario.Id, hoje);
        if (checkinExistente.Success)
        {
            _logger.LogInformation("Check-in do dia já existe no SQLite para usuário {UsuarioId}.", usuario.Id);
            return Result<Checkin>.Ok(checkinExistente.Data!);
        }

        // 4. Montar entidade com Sincronizado=false
        var (latitude, longitude) = localizacaoResult.Data;
        var checkin = new Checkin
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            Data = hoje,
            Latitude = latitude,
            Longitude = longitude,
            IdentificacaoAparelho = _deviceInfoService.GetIdentificacaoAparelho(),
            Sincronizado = false,
            CriadoEm = DateTimeOffset.UtcNow
        };

        // 5. Salvar no SQLite com Sincronizado=false (offline-first)
        await _checkinRepository.UpsertAsync(checkin);
        _logger.LogInformation("Check-in salvo localmente para usuário {UsuarioId}.", usuario.Id);

        // 6. Sincronizar com a API se houver internet
        if (_connectivityService.IsInternetAvailable)
        {
            try
            {
                var request = new CheckinRequest(
                    checkin.Data,
                    checkin.Latitude,
                    checkin.Longitude,
                    checkin.IdentificacaoAparelho);

                await _apiClient.PostAsync<CheckinRequest, CheckinResponse>("checkins", request);

                // Sucesso: marcar como sincronizado no SQLite
                checkin.Sincronizado = true;
                await _checkinRepository.UpsertAsync(checkin);
                _logger.LogInformation("Check-in sincronizado com a API para usuário {UsuarioId}.", usuario.Id);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                // 409 Conflict: já existe no servidor — marcar como sincronizado localmente
                checkin.Sincronizado = true;
                await _checkinRepository.UpsertAsync(checkin);
                _logger.LogInformation(
                    "Check-in já existe no servidor (409) para usuário {UsuarioId}. Marcado como Sincronizado=true.",
                    usuario.Id);
            }
            catch (Exception ex)
            {
                // Outros erros: manter Sincronizado=false — será sincronizado na próxima oportunidade
                _logger.LogWarning(
                    ex,
                    "Falha ao sincronizar check-in com a API para usuário {UsuarioId}. Mantendo Sincronizado=false.",
                    usuario.Id);
            }
        }
        else
        {
            _logger.LogInformation(
                "Sem internet. Check-in salvo localmente para usuário {UsuarioId}. Sincronizado=false.",
                usuario.Id);
        }

        return Result<Checkin>.Ok(checkin);
    }
}
