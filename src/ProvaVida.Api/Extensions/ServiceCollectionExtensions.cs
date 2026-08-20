using System.Text;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.AlterarConta;using ProvaVida.Application.UseCases.AlterarSenha;
using ProvaVida.Application.UseCases.CadastrarUsuario;
using ProvaVida.Application.UseCases.ExcluirConta;
using ProvaVida.Application.UseCases.Login;
using ProvaVida.Application.UseCases.Logoff;
using ProvaVida.Application.UseCases.ObterHistoricoCheckIn;
using ProvaVida.Application.UseCases.ObterMetricasAdmin;
using ProvaVida.Application.UseCases.TestarNotificacao;
using ProvaVida.Application.UseCases.RefreshToken;
using ProvaVida.Application.UseCases.RegistrarCheckIn;
using ProvaVida.Application.UseCases.RegistrarHeartbeat;
using ProvaVida.Application.UseCases.VerificarInatividade;
using ProvaVida.Infrastructure.Jobs;
using ProvaVida.Infrastructure.Notifications;
using ProvaVida.Infrastructure.Persistence;
using ProvaVida.Infrastructure.Persistence.Repositories;
using ProvaVida.Infrastructure.Security;

namespace ProvaVida.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        services.AddSingleton(_ => new DbConnectionFactory(cs));
        services.AddSingleton(_ => new DatabaseMigrator(cs));

        return services;
    }

    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(cs)));

        services.AddHangfireServer(opts =>
        {
            opts.WorkerCount = 2;
            opts.Queues = ["default"];
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey não configurada.");
        var issuer   = configuration["Jwt:Issuer"]   ?? "ProvaVida";
        var audience = configuration["Jwt:Audience"] ?? "ProvaVida";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = issuer,
                    ValidAudience            = audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew                = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositórios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISessaoLoginRepository, SessaoLoginRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<IHeartbeatRepository, HeartbeatRepository>();
        services.AddScoped<INotificacaoEmergenciaRepository, NotificacaoEmergenciaRepository>();
        services.AddScoped<IAdminMetricasRepository, AdminMetricasRepository>();

        // Segurança
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();

        // Notificações
        services.AddHttpClient<IWhatsAppService, WhatsAppService>();
        services.AddScoped<IWhatsAppService, TwilioWhatsAppService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, TwilioSmsService>();
        services.AddScoped<IVoiceService, TwilioVoiceService>();

        // Validadores
        services.AddScoped<IValidator<CadastrarUsuarioInput>, CadastrarUsuarioValidator>();
        services.AddScoped<IValidator<AlterarContaInput>, AlterarContaValidator>();

        // Casos de uso
        services.AddScoped<CadastrarUsuarioUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<LogoffUseCase>();
        services.AddScoped<RefreshTokenUseCase>();
        services.AddScoped<AlterarContaUseCase>();
        services.AddScoped<AlterarSenhaUseCase>();
        services.AddScoped<ExcluirContaUseCase>();
        services.AddScoped<RegistrarCheckInUseCase>();
        services.AddScoped<RegistrarHeartbeatUseCase>();
        services.AddScoped<ObterHistoricoCheckInUseCase>();
        services.AddScoped<VerificarInatividadeUseCase>();

        // Jobs
        services.AddScoped<VerificacaoInatividadeJob>();
        services.AddScoped<DispararAlertaJob>();

        // Admin
        services.AddScoped<ObterMetricasAdminUseCase>();
        services.AddScoped<TestarNotificacaoUseCase>();

        return services;
    }
}
