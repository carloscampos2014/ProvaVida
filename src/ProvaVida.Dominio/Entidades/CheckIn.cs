using ProvaVida.Dominio.Exceções;

namespace ProvaVida.Dominio.Entidades;

/// <summary>
/// Entidade de Domínio que representa um registro de check-in (prova de vida).
/// Cada check-in estende o prazo de 48 horas a partir da sua data.
/// 
/// Invariantes:
/// - DataProximoVencimento = DataCheckIn + 48 horas
/// - O histórico de check-ins é limitado a 5 registros por usuário (FIFO)
/// </summary>
public sealed class CheckIn
{
    /// <summary>
    /// Identificador único do check-in (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID do usuário que realizou este check-in.
    /// </summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>
    /// Data e hora quando o check-in foi registrado (em UTC).
    /// </summary>
    public DateTime DataCheckIn { get; private set; }

    /// <summary>
    /// Data e hora do próximo vencimento (sempre DataCheckIn + 48 horas).
    /// </summary>
    public DateTime DataProximoVencimento { get; private set; }

    /// <summary>
    /// Localização GPS opcional do usuário no momento do check-in.
    /// </summary>
    public string? Localizacao { get; private set; }

    private CheckIn() { }

    /// <summary>
    /// Factory para criar um novo check-in com cálculo automático de 48h.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="dataCheckIn">Data/hora do check-in (padrão: agora em UTC).</param>
    /// <param name="localizacao">Localização GPS opcional.</param>
    /// <returns>Novo check-in com vencimento calculado.</returns>
    /// <exception cref="CheckInInvalidoException">Se os dados forem inválidos.</exception>
    public static CheckIn Criar(
        Guid usuarioId,
        DateTime? dataCheckIn = null,
        string? localizacao = null)
    {
        if (usuarioId == Guid.Empty)
            throw new CheckInInvalidoException("ID do usuário é inválido.");

        var data = dataCheckIn ?? DateTime.UtcNow;

        if (data > DateTime.UtcNow.AddHours(1))
            throw new CheckInInvalidoException("A data do check-in não pode ser mais de 1 hora no futuro.");

        return new CheckIn
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            DataCheckIn = data,
            DataProximoVencimento = data.AddHours(48),
            Localizacao = localizacao?.Trim()
        };
    }

    /// <summary>
    /// Factory alternativo que aceita DataProximoVencimento já calculada.
    /// Usado internamente ou em cenários específicos.
    /// </summary>
    internal CheckIn(Guid id, Guid usuarioId, DateTime dataCheckIn, DateTime dataProximoVencimento)
    {
        Id = id;
        UsuarioId = usuarioId;
        DataCheckIn = dataCheckIn;
        DataProximoVencimento = dataProximoVencimento;
    }

    /// <summary>
    /// Calcula se este check-in já expirou.
    /// </summary>
    /// <returns>True se agora > DataProximoVencimento.</returns>
    public bool EstaVencido()
    {
        return DateTime.UtcNow > DataProximoVencimento;
    }

    /// <summary>
    /// Calcula quantas horas faltam até o vencimento deste check-in.
    /// </summary>
    /// <returns>Número de horas (pode ser negativo se vencido).</returns>
    public double HorasAteVencimento()
    {
        return (DataProximoVencimento - DateTime.UtcNow).TotalHours;
    }

    /// <summary>
    /// Calcula quantos dias faltam até o vencimento deste check-in.
    /// </summary>
    /// <returns>Número de dias (arredondado para cima).</returns>
    public int DiasAteVencimento()
    {
        var horas = HorasAteVencimento();
        return horas > 0 ? (int)Math.Ceiling(horas / 24.0) : 0;
    }

    /// <summary>
    /// Retorna uma representação amigável do tempo até vencimento.
    /// </summary>
    /// <returns>String descritiva como "1 dia e 5 horas".</returns>
    public string TempoRestante()
    {
        var horasAte = HorasAteVencimento();

        if (horasAte <= 0)
            return "Vencido";

        var dias = (int)(horasAte / 24);
        var horas = (int)(horasAte % 24);

        if (dias > 0)
            return $"{dias} dia{(dias > 1 ? "s" : "")} e {horas} hora{(horas != 1 ? "s" : "")}";

        return $"{horas} hora{(horas != 1 ? "s" : "")}";
    }
}
