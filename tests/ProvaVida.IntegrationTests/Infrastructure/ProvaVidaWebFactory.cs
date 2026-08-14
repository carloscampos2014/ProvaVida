using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ProvaVida.Application.Interfaces;
using ProvaVida.Infrastructure.Persistence;

namespace ProvaVida.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory que usa o banco local provavida_dev.
/// Substitui DbConnectionFactory, DatabaseMigrator, e-mail e WhatsApp por versões de teste.
/// </summary>
public class ProvaVidaWebFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=provavida_dev;Username=postgres;Password=12345678";

    public Mock<IEmailService> EmailServiceMock { get; } = new();
    public Mock<IWhatsAppService> WhatsAppServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        // UseSetting é processado ANTES do ConfigureServices — garante que as keys estejam disponíveis
        builder.UseSetting("Jwt:SecretKey",        "dev-secret-key-provavida-2026-trocar-em-producao-32chars!");
        builder.UseSetting("Jwt:Issuer",           "ProvaVida");
        builder.UseSetting("Jwt:Audience",         "ProvaVida");
        builder.UseSetting("Jwt:ExpirationHours",  "1");

        builder.ConfigureServices(services =>
        {
            // Sobrescreve DbConnectionFactory e DatabaseMigrator com a string de teste
            services.RemoveAll<DbConnectionFactory>();
            services.RemoveAll<DatabaseMigrator>();
            services.AddSingleton(_ => new DbConnectionFactory(TestConnectionString));
            services.AddSingleton(_ => new DatabaseMigrator(TestConnectionString));

            // Mocks de notificação
            services.RemoveAll<IEmailService>();
            services.RemoveAll<IWhatsAppService>();
            services.AddSingleton(EmailServiceMock.Object);
            services.AddSingleton(WhatsAppServiceMock.Object);

            // Hangfire: substitui client e manager por mocks (desativa jobs)
            services.RemoveAll<IBackgroundJobClient>();
            services.RemoveAll<IRecurringJobManager>();
            services.AddSingleton(new Mock<IBackgroundJobClient>().Object);
            services.AddSingleton(new Mock<IRecurringJobManager>().Object);
        });

        // Configura JWT para os testes — removido, já feito via UseSetting acima
    }
}
