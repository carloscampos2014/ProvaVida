using Microsoft.Extensions.DependencyInjection;

namespace ProvaVida.API.Configuracao;

/// <summary>
/// Configuração centralizada do Swagger/OpenAPI.
/// </summary>
public static class ConfiguracaoSwagger
{
    /// <summary>
    /// Adiciona e configura Swagger/OpenAPI.
    /// </summary>
    public static IServiceCollection AddSwagger(this IServiceCollection servicos)
    {
        servicos.AddSwaggerGen(opcoes =>
        {
            opcoes.SwaggerDoc("v1", new()
            {
                Title = "ProvaVida API",
                Version = "v1",
                Description = "API REST para gerenciamento de usuários e check-ins de saúde.",
                Contact = new()
                {
                    Name = "ProvaVida Team",
                    Email = "contato@provavida.com.br"
                },
                License = new()
                {
                    Name = "MIT"
                }
            });

            // 📝 Incluir comentários XML
            var arquivoXml = Path.Combine(AppContext.BaseDirectory, "ProvaVida.API.xml");
            if (File.Exists(arquivoXml))
            {
                opcoes.IncludeXmlComments(arquivoXml);
            }
        });

        return servicos;
    }
}
