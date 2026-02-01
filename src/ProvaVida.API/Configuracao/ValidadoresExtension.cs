using FluentValidation;
using ProvaVida.API.Validadores;

namespace ProvaVida.API.Configuracao;

/// <summary>
/// Extensão para registrar todos os validadores do FluentValidation.
/// </summary>
public static class ValidadoresExtension
{
    /// <summary>
    /// Adiciona validadores ao container de DI.
    /// </summary>
    public static IServiceCollection AdicionarValidadores(this IServiceCollection servicos)
    {
        // 📦 Registrar validadores automaticamente
        servicos.AddValidatorsFromAssemblyContaining<UsuarioRegistroDtoValidator>();

        return servicos;
    }
}
