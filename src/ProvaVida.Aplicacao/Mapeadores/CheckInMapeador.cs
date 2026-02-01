using ProvaVida.Aplicacao.Dtos.CheckIns;
using ProvaVida.Dominio.Entidades;

namespace ProvaVida.Aplicacao.Mapeadores;

/// <summary>
/// Mapeador manual entre CheckIn (Entidade) e DTOs.
/// 
/// CheckIn é mais simples que Usuario:
/// - Não tem campos sensíveis
/// - Factory CheckIn.Criar() é direto
/// </summary>
public static class CheckInMapeador
{
    /// <summary>
    /// Mapeia CheckIn (Entidade) para CheckInResumoDto (resposta).
    /// 
    /// Inclui: Id, UsuarioId, DataCheckIn, DataProximoVencimento
    /// Mensagem amigável ao usuário confirmando sucesso.
    /// </summary>
    public static CheckInResumoDto ParaResumoDto(this CheckIn checkIn)
    {
        if (checkIn == null)
            throw new ArgumentNullException(nameof(checkIn), "CheckIn não pode ser nulo.");

        return new CheckInResumoDto
        {
            Id = checkIn.Id,
            UsuarioId = checkIn.UsuarioId,
            DataCheckIn = checkIn.DataCheckIn,
            DataProximoVencimento = checkIn.DataProximoVencimento,
            Mensagem = "✅ Check-in realizado com sucesso! Próximo prazo em 48 horas."
        };
    }

    /// <summary>
    /// Mapeia CheckInRegistroDto (entrada) para CheckIn (Entidade).
    /// 
    /// IMPORTANTE:
    /// - DataCheckIn sempre é DateTime.UtcNow (não pode vir do cliente)
    /// - Localização é opcional
    /// - Factory valida: usuarioId não vazio, data não é futura
    /// </summary>
    public static CheckIn ParaDominio(this CheckInRegistroDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO de check-in não pode ser nulo.");

        if (dto.UsuarioId == Guid.Empty)
            throw new ArgumentException("ID do usuário é obrigatório.", nameof(dto.UsuarioId));

        // Factory do Domínio faz a lógica: DataCheckIn + 48h = DataProximoVencimento
        return CheckIn.Criar(
            usuarioId: dto.UsuarioId,
            dataCheckIn: DateTime.UtcNow,  // ✅ Sempre "agora" (servidor decide)
            localizacao: dto.Localizacao
        );
    }

    /// <summary>
    /// Converte lista de CheckIns para DTOs.
    /// Utilitário para respostas em massa (histórico).
    /// </summary>
    public static List<CheckInResumoDto> ParaResumosDtos(
        this IEnumerable<CheckIn> checkIns)
    {
        if (checkIns == null)
            throw new ArgumentNullException(nameof(checkIns));

        return checkIns
            .Select(c => c.ParaResumoDto())
            .ToList();
    }
}
