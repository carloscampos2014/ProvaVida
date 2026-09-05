using System.Data;
using FluentAssertions;
using Moq;
using ProvaVida.Mobile.Infrastructure.Data;

namespace ProvaVida.Mobile.Tests.Data;

/// <summary>
/// Testes unitários para <see cref="GuidTypeHandler"/>.
/// Cobre <c>SetValue</c> e todos os ramos do switch em <c>Parse</c>.
/// </summary>
public class GuidTypeHandlerTests
{
    private readonly GuidTypeHandler _handler = new();

    // ── SetValue ──────────────────────────────────────────────────────────

    [Fact]
    public void SetValue_DeveDefinirDbTypeString_EValorComoTextoDoGuid()
    {
        var param = CriarParametro();
        var guid = Guid.NewGuid();

        _handler.SetValue(param.Object, guid);

        param.VerifySet(p => p.DbType = DbType.String);
        param.VerifySet(p => p.Value = guid.ToString());
    }

    // ── Parse — ramo Guid ─────────────────────────────────────────────────

    [Fact]
    public void Parse_DeveRetornarGuid_QuandoValorJaEGuid()
    {
        var guid = Guid.NewGuid();

        var result = _handler.Parse(guid);

        result.Should().Be(guid);
    }

    // ── Parse — ramo string ───────────────────────────────────────────────

    [Fact]
    public void Parse_DeveConverterString_ParaGuid()
    {
        var guid = Guid.NewGuid();

        var result = _handler.Parse(guid.ToString());

        result.Should().Be(guid);
    }

    // ── Parse — ramo fallback (Convert.ToString) ──────────────────────────

    [Fact]
    public void Parse_DeveFallbackParaConvertToString_QuandoValorNaoEStringNemGuid()
    {
        // Qualquer tipo boxeado diferente de Guid e string cai no ramo "_".
        // GuidWrapper.ToString() retorna a representação do guid, ativando o caminho fallback.
        var guid = Guid.NewGuid();
        var wrapped = new GuidWrapper(guid);

        var result = _handler.Parse(wrapped);

        result.Should().Be(guid);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Mock<IDbDataParameter> CriarParametro()
    {
        var mock = new Mock<IDbDataParameter>();
        mock.SetupAllProperties();
        return mock;
    }

    /// <summary>Wrapper cujo <see cref="ToString"/> retorna o guid para exercitar o ramo fallback.</summary>
    private sealed class GuidWrapper(Guid value)
    {
        public override string ToString() => value.ToString();
    }
}
