using FluentAssertions;
using Moq;
using ProvaVida.Admin.Application.Dtos;
using ProvaVida.Admin.Infrastructure.Queries;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Admin.Tests.Queries;

/// <summary>
/// Testes unitários para <see cref="AdminMetricasQueryService"/>.
/// Usa mock de <see cref="IDbConnectionFactory"/> para isolar do banco.
/// Para validação das queries SQL reais, ver <see cref="AdminMetricasQueryServiceIntegrationTests"/>.
/// </summary>
public class AdminMetricasQueryServiceTests
{
    // ── MetricasAsync — caminho feliz ─────────────────────────────────────

    [Fact]
    public async Task MetricasAsync_DeveRetornarDto_ComValoresCorretos()
    {
        // Arrange: service parcialmente mockado para controlar cada método
        var factoryMock = new Mock<IDbConnectionFactory>();
        var service = new AdminMetricasQueryServiceStub(
            totalUsuarios: 10,
            novos7Dias: 3,
            checkinHoje: 7,
            semCheckin2Dias: 2);

        // Act
        var result = await service.MetricasAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.TotalUsuarios.Should().Be(10);
        result.Data.NovosUltimos7Dias.Should().Be(3);
        result.Data.UsuariosComCheckinHoje.Should().Be(7);
        result.Data.UsuariosSemCheckin2Dias.Should().Be(2);
        result.Data.GeradoEm.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task MetricasAsync_DeveRetornarFail_QuandoTotalUsuariosFalha()
    {
        var service = new AdminMetricasQueryServiceStub(
            totalUsuariosErro: "Erro de conexão");

        var result = await service.MetricasAsync();

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("Erro de conexão");
    }

    [Fact]
    public async Task MetricasAsync_DeveRetornarFail_QuandoNovosUsuariosFalha()
    {
        var service = new AdminMetricasQueryServiceStub(
            totalUsuarios: 5,
            novos7DiasErro: "Timeout ao buscar novos usuários");

        var result = await service.MetricasAsync();

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("Timeout ao buscar novos usuários");
    }

    // ── Stub auxiliar ─────────────────────────────────────────────────────

    /// <summary>
    /// Stub de <see cref="AdminMetricasQueryService"/> que sobrescreve os métodos individuais,
    /// permitindo testar <see cref="AdminMetricasQueryService.MetricasAsync"/> sem banco real.
    /// </summary>
    private sealed class AdminMetricasQueryServiceStub : AdminMetricasQueryService
    {
        private readonly int _total;
        private readonly int _novos;
        private readonly int _checkinHoje;
        private readonly int _semCheckin;
        private readonly string? _totalErro;
        private readonly string? _novosErro;

        public AdminMetricasQueryServiceStub(
            int totalUsuarios = 0,
            int novos7Dias = 0,
            int checkinHoje = 0,
            int semCheckin2Dias = 0,
            string? totalUsuariosErro = null,
            string? novos7DiasErro = null)
            : base(new Mock<IDbConnectionFactory>().Object)
        {
            _total       = totalUsuarios;
            _novos       = novos7Dias;
            _checkinHoje = checkinHoje;
            _semCheckin  = semCheckin2Dias;
            _totalErro   = totalUsuariosErro;
            _novosErro   = novos7DiasErro;
        }

        public override Task<Result<int>> TotalUsuariosAsync() =>
            Task.FromResult(_totalErro is not null
                ? Result<int>.Fail(_totalErro)
                : Result<int>.Ok(_total));

        public override Task<Result<int>> NovosUsuariosAsync(int dias) =>
            Task.FromResult(_novosErro is not null
                ? Result<int>.Fail(_novosErro)
                : Result<int>.Ok(_novos));

        public override Task<Result<int>> UsuariosComCheckinHojeAsync() =>
            Task.FromResult(Result<int>.Ok(_checkinHoje));

        public override Task<Result<int>> UsuariosSemCheckinAsync(int dias) =>
            Task.FromResult(Result<int>.Ok(_semCheckin));
    }
}
