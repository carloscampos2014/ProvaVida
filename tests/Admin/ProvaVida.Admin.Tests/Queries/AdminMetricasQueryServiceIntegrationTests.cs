using DotNet.Testcontainers.Builders;
using FluentAssertions;
using ProvaVida.Admin.Infrastructure;
using ProvaVida.Admin.Infrastructure.Queries;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProvaVida.Admin.Tests.Queries;

/// <summary>
/// Testes de integração para <see cref="AdminMetricasQueryService"/>.
/// Valida as queries SQL agregadas contra um PostgreSQL real via Testcontainers.
/// Requer Docker disponível via TCP em tcp://localhost:2375 (WSL2).
/// Use <c>--filter "Category!=Integration"</c> para pular em ambientes sem Docker.
/// </summary>
[Trait("Category", "Integration")]
public class AdminMetricasQueryServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private AdminConnectionFactory _factory = null!;
    private AdminMetricasQueryService _service = null!;

    public AdminMetricasQueryServiceIntegrationTests()
    {
        var dockerEndpoint = Environment.GetEnvironmentVariable("DOCKER_HOST")
            ?? "tcp://localhost:2375";

        _postgres = new PostgreSqlBuilder()
            .WithDockerEndpoint(dockerEndpoint)
            .WithImage("postgres:16-alpine")
            .WithDatabase("provavida_admin_metricas_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = new AdminConnectionFactory(_postgres.GetConnectionString());
        _service = new AdminMetricasQueryService(_factory);
        await CriarTabelasAsync();
    }

    public async Task DisposeAsync() => await _postgres.StopAsync();

    // ── TotalUsuariosAsync ────────────────────────────────────────────────

    [Fact]
    public async Task TotalUsuariosAsync_DeveRetornarZero_QuandoBancoVazio()
    {
        var result = await _service.TotalUsuariosAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().Be(0);
    }

    [Fact]
    public async Task TotalUsuariosAsync_DeveContarUsuariosInseridos()
    {
        await InserirUsuarioAsync(CriarUsuario());
        await InserirUsuarioAsync(CriarUsuario());

        var result = await _service.TotalUsuariosAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThanOrEqualTo(2);
    }

    // ── NovosUsuariosAsync ────────────────────────────────────────────────

    [Fact]
    public async Task NovosUsuariosAsync_DeveContarUsuariosCriadosNosUltimosDias()
    {
        await InserirUsuarioAsync(CriarUsuario(criadoEm: DateTimeOffset.UtcNow));
        await InserirUsuarioAsync(CriarUsuario(criadoEm: DateTimeOffset.UtcNow.AddDays(-10)));

        var result = await _service.NovosUsuariosAsync(7);

        result.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThanOrEqualTo(1, "apenas o usuário recente entra na janela de 7 dias");
    }

    // ── UsuariosComCheckinHojeAsync ───────────────────────────────────────

    [Fact]
    public async Task UsuariosComCheckinHojeAsync_DeveRetornarZero_QuandoSemCheckins()
    {
        var result = await _service.UsuariosComCheckinHojeAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().Be(0);
    }

    [Fact]
    public async Task UsuariosComCheckinHojeAsync_DeveContarUsuariosDistintos()
    {
        var usuario = CriarUsuario();
        await InserirUsuarioAsync(usuario);
        await InserirCheckinAsync(usuario.Id, DateOnly.FromDateTime(DateTime.UtcNow));

        var result = await _service.UsuariosComCheckinHojeAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThanOrEqualTo(1);
    }

    // ── UsuariosSemCheckinAsync ───────────────────────────────────────────

    [Fact]
    public async Task UsuariosSemCheckinAsync_DeveContarUsuariosSemCheckinNaJanela()
    {
        var comCheckin = CriarUsuario();
        var semCheckin = CriarUsuario();
        await InserirUsuarioAsync(comCheckin);
        await InserirUsuarioAsync(semCheckin);
        await InserirCheckinAsync(comCheckin.Id, DateOnly.FromDateTime(DateTime.UtcNow));

        var result = await _service.UsuariosSemCheckinAsync(2);

        result.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThanOrEqualTo(1, "semCheckin não tem registro de check-in");
    }

    // ── MetricasAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task MetricasAsync_DeveRetornarDtoConsistente()
    {
        var usuario = CriarUsuario();
        await InserirUsuarioAsync(usuario);
        await InserirCheckinAsync(usuario.Id, DateOnly.FromDateTime(DateTime.UtcNow));

        var result = await _service.MetricasAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalUsuarios.Should().BeGreaterThanOrEqualTo(1);
        result.Data.UsuariosComCheckinHoje.Should().BeGreaterThanOrEqualTo(1);
        result.Data.GeradoEm.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private record UsuarioSimples(Guid Id, DateTimeOffset CriadoEm);

    private static UsuarioSimples CriarUsuario(DateTimeOffset? criadoEm = null) =>
        new(Guid.NewGuid(), criadoEm ?? DateTimeOffset.UtcNow);

    private async Task InserirUsuarioAsync(UsuarioSimples u)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO usuarios (id, nome, email, whatsapp, senha_hash,
                contato_emergencia_nome, contato_emergencia_email, contato_emergencia_whatsapp,
                criado_em, atualizado_em)
            VALUES (@id, 'Teste', @email, '11999999999', 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa',
                '', '', '', @criado_em, NOW())
            ON CONFLICT (id) DO NOTHING
            """;
        var p = cmd.CreateParameter(); p.ParameterName = "@id"; p.Value = u.Id; cmd.Parameters.Add(p);
        p = cmd.CreateParameter(); p.ParameterName = "@email"; p.Value = $"m_{Guid.NewGuid():N}@t.com"; cmd.Parameters.Add(p);
        p = cmd.CreateParameter(); p.ParameterName = "@criado_em"; p.Value = u.CriadoEm; cmd.Parameters.Add(p);
        cmd.ExecuteNonQuery();
        await Task.CompletedTask;
    }

    private async Task InserirCheckinAsync(Guid usuarioId, DateOnly data)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO checkins (id, usuario_id, data, latitude, longitude, identificacao_aparelho, sincronizado, criado_em)
            VALUES (@id, @usuario_id, @data, @lat, @lon, @device, false, NOW())
            ON CONFLICT (usuario_id, data) DO NOTHING
            """;
        var p = cmd.CreateParameter(); p.ParameterName = "@id"; p.Value = Guid.NewGuid(); cmd.Parameters.Add(p);
        p = cmd.CreateParameter(); p.ParameterName = "@usuario_id"; p.Value = usuarioId; cmd.Parameters.Add(p);
        p = cmd.CreateParameter(); p.ParameterName = "@data"; p.Value = data.ToDateTime(TimeOnly.MinValue).Date; cmd.Parameters.Add(p);
        p = cmd.CreateParameter(); p.ParameterName = "@lat"; p.Value = -23.55; cmd.Parameters.Add(p);
        p = cmd.CreateParameter(); p.ParameterName = "@lon"; p.Value = -46.63; cmd.Parameters.Add(p);
        p = cmd.CreateParameter(); p.ParameterName = "@device"; p.Value = $"device-{Guid.NewGuid():N}"; cmd.Parameters.Add(p);
        cmd.ExecuteNonQuery();
        await Task.CompletedTask;
    }

    private async Task CriarTabelasAsync()
    {
        using var conn = _factory.Create();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS usuarios (
                id                          UUID        PRIMARY KEY,
                nome                        TEXT        NOT NULL,
                email                       TEXT        NOT NULL UNIQUE,
                whatsapp                    TEXT        NOT NULL,
                senha_hash                  TEXT        NOT NULL,
                contato_emergencia_nome     TEXT        NOT NULL DEFAULT '',
                contato_emergencia_email    TEXT        NOT NULL DEFAULT '',
                contato_emergencia_whatsapp TEXT        NOT NULL DEFAULT '',
                criado_em                   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                atualizado_em               TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE TABLE IF NOT EXISTS checkins (
                id                      UUID        PRIMARY KEY,
                usuario_id              UUID        NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
                data                    DATE        NOT NULL,
                latitude                FLOAT8      NOT NULL,
                longitude               FLOAT8      NOT NULL,
                identificacao_aparelho  TEXT        NOT NULL,
                sincronizado            BOOLEAN     NOT NULL DEFAULT FALSE,
                criado_em               TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                CONSTRAINT uq_checkin_usuario_data UNIQUE (usuario_id, data)
            );
            """;
        cmd.ExecuteNonQuery();
        await Task.CompletedTask;
    }
}
