namespace ProvaVida.Domain.Entities;

public class SessaoLogin
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public bool Ativo { get; private set; }

    // Refresh token — armazena SHA-256 do token original, expira em 365 dias
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiraEm { get; private set; }

    // EF Core navigation
    public Usuario? Usuario { get; private set; }

    protected SessaoLogin() { }

    public static SessaoLogin Criar(
        Guid usuarioId,
        string token,
        DateTime expiraEm,
        string refreshTokenHash,
        DateTime refreshTokenExpiraEm)
    {
        return new SessaoLogin
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Token = token,
            CriadoEm = DateTime.UtcNow,
            ExpiraEm = expiraEm,
            Ativo = true,
            RefreshTokenHash = refreshTokenHash,
            RefreshTokenExpiraEm = refreshTokenExpiraEm
        };
    }

    public void Invalidar()
    {
        Ativo = false;
    }

    public bool EstaValida() => Ativo && ExpiraEm > DateTime.UtcNow;

    public bool RefreshTokenValido() =>
        Ativo &&
        RefreshTokenHash is not null &&
        RefreshTokenExpiraEm.HasValue &&
        RefreshTokenExpiraEm.Value > DateTime.UtcNow;
}
