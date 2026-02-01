using ProvaVida.Aplicacao.Dtos.CheckIns;
using ProvaVida.Aplicacao.Exceções;
using ProvaVida.Aplicacao.Mapeadores;
using ProvaVida.Dominio.Enums;
using ProvaVida.Dominio.Repositorios;

namespace ProvaVida.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de check-in.
/// 
/// Responsabilidades:
/// - Validar entrada
/// - Orquestrar Domínio + Infraestrutura
/// - Persistir histórico (máx 5 registros via FIFO)
/// - Limpar alertas pendentes (lógica do Domínio)
/// 
/// REGRAS PESADAS ficam no Domínio (Usuario.RegistrarCheckIn()).
/// Este serviço apenas ORQUESTRA.
/// </summary>
public class CheckInService : ICheckInService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioCheckIn _repositorioCheckIn;
    private readonly IRepositorioNotificacao _repositorioNotificacao;

    /// <summary>
    /// Construtor com injeção de dependência.
    /// </summary>
    /// <exception cref="ArgumentNullException">Se alguma dependência é nula.</exception>
    public CheckInService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioCheckIn repositorioCheckIn,
        IRepositorioNotificacao repositorioNotificacao)
    {
        _repositorioUsuario = repositorioUsuario ?? 
            throw new ArgumentNullException(nameof(repositorioUsuario));
        _repositorioCheckIn = repositorioCheckIn ?? 
            throw new ArgumentNullException(nameof(repositorioCheckIn));
        _repositorioNotificacao = repositorioNotificacao ?? 
            throw new ArgumentNullException(nameof(repositorioNotificacao));
    }

    /// <summary>
    /// Registra um novo check-in para o usuário.
    /// 
    /// FLUXO:
    /// 1️⃣ Validar entrada
    /// 2️⃣ Buscar usuário por ID
    /// 3️⃣ Verificar se usuário está ativo
    /// 4️⃣ Invocar Usuario.RegistrarCheckIn() (Domínio limpa e atualiza)
    /// 5️⃣ Persistir atualização do usuário
    /// 6️⃣ Limpar notificações pendentes
    /// 7️⃣ Retornar confirmação
    /// </summary>
    public async Task<CheckInResumoDto> RegistrarCheckInAsync(
        CheckInRegistroDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1️⃣ Validar entrada
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO de check-in não pode ser nulo.");

        if (dto.UsuarioId == Guid.Empty)
            throw new AplicacaoException("ID do usuário é obrigatório.");

        // 2️⃣ Buscar usuário
        var usuario = await _repositorioUsuario.ObterPorIdAsync(
            dto.UsuarioId,
            cancellationToken);

        if (usuario == null)
            throw new UsuarioNaoEncontradoException(
                $"Usuário com ID '{dto.UsuarioId}' não encontrado.");

        // 3️⃣ Verificar se está ativo
        if (usuario.Status == StatusUsuario.Inativo)
            throw new UsuarioInativoException(
                "Usuário inativo não pode fazer check-in.");

        // 4️⃣ Invocar método do Domínio
        // O Domínio reseta a data e o histórico interno (máx 5 registros FIFO)
        usuario.RegistrarCheckIn();

        // 5️⃣ Persistir atualização do usuário
        await _repositorioUsuario.AtualizarAsync(usuario, cancellationToken);

        // 6️⃣ Limpar notificações pendentes
        await LimparNotificacoesPendentesAsync(usuario.Id, cancellationToken);

        // 7️⃣ Retornar confirmação com último check-in
        var ultimoCheckIn = usuario.HistoricoCheckIns.LastOrDefault();
        
        if (ultimoCheckIn == null)
            throw new AplicacaoException("Erro ao registrar check-in - histórico vazio.");

        return ultimoCheckIn.ParaResumoDto();
    }

    /// <summary>
    /// Obtém o histórico de check-ins (últimos 5) do usuário.
    /// </summary>
    public async Task<List<CheckInResumoDto>> ObterHistoricoAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty)
            throw new AplicacaoException("ID do usuário é obrigatório.");

        // Buscar últimos 5 check-ins
        var checkIns = await _repositorioCheckIn.ObterHistoricoAsync(
            usuarioId,
            limite: 5,
            cancellationToken: cancellationToken);

        // Mapear para DTOs
        return checkIns.ParaResumosDtos();
    }

    /// <summary>
    /// Helper privado: limpa notificações pendentes deste usuário.
    /// 
    /// Quando um usuário faz check-in, as notificações de lembrete/emergência
    /// pendentes devem ser canceladas (status = Cancelada).
    /// </summary>
    private async Task LimparNotificacoesPendentesAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        // Buscar todas as notificações do usuário
        var notificacoes = await _repositorioNotificacao
            .ObterPorUsuarioIdAsync(usuarioId, cancellationToken);

        // Filtrar as que estão pendentes e cancelar
        var notificacoesPendentes = notificacoes
            .Where(n => n.Status == Dominio.Enums.StatusNotificacao.Pendente);

        foreach (var notif in notificacoesPendentes)
        {
            notif.Cancelar();
            // Nota: Atualizar no banco não é necessário aqui pois é uma operação
            // que pode ser feita em background job na Sprint 4 (Quartz Scheduler)
        }
    }
}
