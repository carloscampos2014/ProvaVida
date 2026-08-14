using ProvaVida.Maui.Models;

namespace ProvaVida.Maui.Storage;

public interface IUsuarioStorage
{
    void Salvar(UsuarioLocal usuario);
    UsuarioLocal? Obter();
    void Remover();
}
