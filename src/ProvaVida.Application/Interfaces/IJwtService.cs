using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Interfaces;

public interface IJwtService
{
    string GerarToken(Usuario usuario, out DateTime expiraEm);
}
