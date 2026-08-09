namespace ProvaVida.Domain.Entities;

public class SessaoLogin
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public bool Ativo { get; private set; }

    // EF Core navigation
    public Usuario? Usuario { get; private set; }

    protected SessaoLogin() { }

    public static SessaoLogin Criar(Guid usuarioId, string token, DateTime expiraEm)
    {
        return new SessaoLogin
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Token = token,
            CriadoEm = DateTime.UtcNow,
            ExpiraEm = expiraEm,
            Ativo = true
        };
    }

    public void Invalidar()
    {
        Ativo = false;
    }

    public bool EstaValida() => Ativo && ExpiraEm > DateTime.UtcNow;
}
