using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.API.Configuracao;
using ProvaVida.API.Middleware;
using ProvaVida.API.Validadores;
using ProvaVida.Aplicacao.Configuracao;
using ProvaVida.Infraestrutura;
using ProvaVida.Infraestrutura.Configuracao;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProvaVida.API;

/// <summary>
/// Ponto de entrada da aplicação ASP.NET Core.
/// Centraliza toda a configuração de DI, middlewares e Swagger.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 📋 Adicionar serviços ao container
        ConfigurarServicos(builder.Services, builder.Configuration);

        var app = builder.Build();

        // 🔌 Configurar pipeline de requisição HTTP
        ConfigurarPipeline(app);

        app.Run();
    }

    /// <summary>
    /// Registra todos os serviços necessários (DI).
    /// ORDEM IMPORTA: Camadas de fora para dentro.
    /// </summary>
    private static void ConfigurarServicos(IServiceCollection servicos, IConfiguration configuracao)
    {
        // 🛠️ Controllers
        servicos.AddControllers();

        // 📚 Swagger/OpenAPI
        //servicos.AddOpenApi();
        servicos.AddSwagger();

        // 🔐 CORS (se necessário para frontend)
        servicos.AddCors(opcoes =>
        {
            opcoes.AddPolicy("PermitirTodos", builder =>
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());
        });

        // ✅ Validação Fluent
        servicos.AddFluentValidationAutoValidation();
        servicos.AddValidatorsFromAssemblyContaining<UsuarioRegistroDtoValidator>();

        // 📦 Injeção de Dependência das Camadas
        // ORDEM: Infraestrutura → Aplicação → API
        
        // 1️⃣ Infraestrutura (BD, Repositórios, Serviços de Hash)
        var configDb = new ConfiguracaoBancoDados
        {
            Tipo = TipoProviderBancoDados.SQLite,  // Dev: SQLite
            StringConexao = configuracao.GetConnectionString("DefaultConnection") 
                ?? "Data Source=provavida.db"
        };
        servicos.AdicionarInfraestrutura(configDb);

        // 2️⃣ Aplicação (Services, Mapeadores)
        servicos.AdicionarAplicacao();

        // 3️⃣ API (Controllers já estão acima)

        // ⚙️ Configurações
        servicos.ConfigureHttpJsonOptions(opcoes =>
        {
            // Serializar enums como string
            opcoes.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            // CamelCase para JSON
            opcoes.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    }

    /// <summary>
    /// Configura o pipeline de requisição HTTP (middlewares).
    /// ORDEM IMPORTA: Do exterior para interior.
    /// </summary>
    private static void ConfigurarPipeline(WebApplication app)
    {
        // 🌍 Ambiente de desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProvaVida API v1");
                c.RoutePrefix = string.Empty;  // Swagger na raiz: http://localhost:5000/
            });

            // 🔍 Exceção de desenvolvedor
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // 🔒 Produção: Não expõe detalhes
            app.UseExceptionHandler("/error");
        }

        // 🛡️ HTTPS redirection
        //app.UseHttpsRedirection();

        // 🔓 CORS
        app.UseCors("PermitirTodos");

        // ⚠️ MIDDLEWARE GLOBAL DE EXCEÇÃO (ORDEM IMPORTA: ANTES de routing)
        app.UseGlobalExceptionMiddleware();

        // 🚦 Roteamento
        app.UseRouting();

        // 🔗 Endpoints
        app.MapControllers();

        // 🔐 Autenticação/Autorização (futura - JWT)
        // app.UseAuthentication();
        // app.UseAuthorization();

        // 📊 Logging simples
        app.Logger.LogInformation("🚀 ProvaVida API iniciada em {Ambiente}", app.Environment.EnvironmentName);
    }
}
