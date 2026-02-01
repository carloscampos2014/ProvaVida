using Microsoft.Extensions.DependencyInjection;
using ProvaVida.Aplicacao.Servicos;

namespace ProvaVida.Aplicacao.Configuracao;

/// <summary>
/// Extensão para registrar serviços da Aplicação no container DI.
/// 
/// PADRÃO: Extension method no IServiceCollection para manter código limpo.
/// 
/// USO em Program.cs:
///   services.AdicionarAplicacao();
///   
/// Esta classe centraliza todos os registros de DI da camada de Aplicação,
/// tornando fácil adicionar/remover serviços.
/// </summary>
public static class ConfiguracaoAplicacao
{
    /// <summary>
    /// Registra todos os serviços da camada de Aplicação no container DI.
    /// 
    /// Serviços registrados:
    /// - IAutenticacaoService → AutenticacaoService (Scoped)
    /// - ICheckInService → CheckInService (Scoped)
    /// - INotificacaoService → NotificacaoService (Scoped)
    /// - IContatoEmergenciaService → ContatoEmergenciaService (Scoped)
    /// </summary>
    /// <param name="servicos">Coleção de serviços do .NET</param>
    /// <returns>Coleção para fluency</returns>
    /// <exception cref="ArgumentNullException">Se servicos é nulo</exception>
    public static IServiceCollection AdicionarAplicacao(
        this IServiceCollection servicos)
    {
        if (servicos == null)
            throw new ArgumentNullException(nameof(servicos), "Coleção de serviços não pode ser nula.");

        // 📝 Serviços de Aplicação (Scoped = uma instância por requisição HTTP)
        servicos.AddScoped<IAutenticacaoService, AutenticacaoService>();
        servicos.AddScoped<ICheckInService, CheckInService>();
        // Serviços futuros:
        // servicos.AddScoped<INotificacaoService, NotificacaoService>();
        // servicos.AddScoped<IContatoEmergenciaService, ContatoEmergenciaService>();

        return servicos;
    }
}
