using System.Data;
using FluentAssertions;
using Moq;
using ProvaVida.Mobile.Infrastructure.Data;

namespace ProvaVida.Mobile.Tests.Data;

/// <summary>
/// Testes unitários para <see cref="DateTimeOffsetTypeHandler"/>.
/// Cobre <c>SetValue</c> e todos os ramos do switch em <c>Parse</c>:
/// <c>DateTimeOffset</c>, <c>DateTime</c>, <c>string</c> e fallback.
/// </summary>
public class DateTimeOffsetTypeHandlerTests
{
    private readonly DateTimeOffsetTypeHandler _handler = new();

    // ── SetValue ──────────────────────────────────────────────────────────

    [Fact]
    public void SetValue_DeveDefinirDbTypeString_EFormatoISO8601()
    {
        var param = CriarParametro();
        var dto = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero);

        _handler.SetValue(param.Object, dto);

        param.VerifySet(p => p.DbType = DbType.String);
        // O formato "O" produz representação round-trip ISO 8601
        param.VerifySet(p => p.Value = dto.ToString("O"));
    }

    // ── Parse — ramo DateTimeOffset ───────────────────────────────────────

    [Fact]
    public void Parse_DeveRetornarDateTimeOffset_QuandoValorJaEDateTimeOffset()
    {
        var dto = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero);

        var result = _handler.Parse(dto);

        result.Should().Be(dto);
    }

    // ── Parse — ramo DateTime ─────────────────────────────────────────────

    [Fact]
    public void Parse_DeveConverterDateTime_ParaDateTimeOffsetComOffsetZero()
    {
        var dt = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        var result = _handler.Parse(dt);

        result.Should().Be(new DateTimeOffset(dt, TimeSpan.Zero));
        result.Offset.Should().Be(TimeSpan.Zero);
    }

    // ── Parse — ramo string ───────────────────────────────────────────────

    [Fact]
    public void Parse_DeveConverterString_ParaDateTimeOffset()
    {
        var dto = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero);
        var iso = dto.ToString("O");

        var result = _handler.Parse(iso);

        result.Should().Be(dto);
    }

    // ── Parse — ramo fallback (Convert.ToString) ──────────────────────────

    [Fact]
    public void Parse_DeveFallbackParaConvertToString_QuandoValorNaoEConhecido()
    {
        var dto = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero);
        var wrapped = new DateTimeOffsetWrapper(dto);

        var result = _handler.Parse(wrapped);

        // O fallback usa Convert.ToString → DateTimeOffset.Parse, preservando o valor
        result.Should().BeCloseTo(dto, TimeSpan.FromSeconds(1));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Mock<IDbDataParameter> CriarParametro()
    {
        var mock = new Mock<IDbDataParameter>();
        mock.SetupAllProperties();
        return mock;
    }

    /// <summary>Wrapper cujo <see cref="ToString"/> retorna um DateTimeOffset para exercitar o ramo fallback.</summary>
    private sealed class DateTimeOffsetWrapper(DateTimeOffset value)
    {
        public override string ToString() => value.ToString("O");
    }
}
