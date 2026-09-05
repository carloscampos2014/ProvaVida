using System.Data;
using FluentAssertions;
using Moq;
using ProvaVida.Mobile.Infrastructure.Data;

namespace ProvaVida.Mobile.Tests.Data;

/// <summary>
/// Testes unitários para <see cref="DateOnlyTypeHandler"/>.
/// Cobre <c>SetValue</c> e todos os ramos do switch em <c>Parse</c>:
/// <c>DateOnly</c>, <c>DateTime</c>, <c>string</c> e fallback.
/// </summary>
public class DateOnlyTypeHandlerTests
{
    private readonly DateOnlyTypeHandler _handler = new();

    // ── SetValue ──────────────────────────────────────────────────────────

    [Fact]
    public void SetValue_DeveDefinirDbTypeString_EFormatoYyyyMMdd()
    {
        var param = CriarParametro();
        var date = new DateOnly(2025, 6, 15);

        _handler.SetValue(param.Object, date);

        param.VerifySet(p => p.DbType = DbType.String);
        param.VerifySet(p => p.Value = "2025-06-15");
    }

    // ── Parse — ramo DateOnly ─────────────────────────────────────────────

    [Fact]
    public void Parse_DeveRetornarDateOnly_QuandoValorJaEDateOnly()
    {
        var date = new DateOnly(2025, 6, 15);

        var result = _handler.Parse(date);

        result.Should().Be(date);
    }

    // ── Parse — ramo DateTime ─────────────────────────────────────────────

    [Fact]
    public void Parse_DeveConverterDateTime_ParaDateOnly()
    {
        var dt = new DateTime(2025, 6, 15, 10, 30, 0);

        var result = _handler.Parse(dt);

        result.Should().Be(new DateOnly(2025, 6, 15));
    }

    // ── Parse — ramo string ───────────────────────────────────────────────

    [Fact]
    public void Parse_DeveConverterString_ParaDateOnly()
    {
        var result = _handler.Parse("2025-06-15");

        result.Should().Be(new DateOnly(2025, 6, 15));
    }

    // ── Parse — ramo fallback (Convert.ToDateTime) ────────────────────────

    [Fact]
    public void Parse_DeveFallbackParaConvertToDateTime_QuandoValorNaoEConhecido()
    {
        // Convert.ToDateTime(object) exige IConvertible; DateWrapper implementa a interface
        // retornando o DateTime subjacente, ativando o ramo "_" do switch.
        var dt = new DateTime(2025, 6, 15);
        var wrapped = new DateWrapper(dt);

        var result = _handler.Parse(wrapped);

        result.Should().Be(new DateOnly(2025, 6, 15));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Mock<IDbDataParameter> CriarParametro()
    {
        var mock = new Mock<IDbDataParameter>();
        mock.SetupAllProperties();
        return mock;
    }

    /// <summary>
    /// Wrapper que implementa <see cref="IConvertible"/> delegando para um <see cref="DateTime"/>
    /// interno, para exercitar o ramo fallback de <c>Convert.ToDateTime(value)</c>.
    /// </summary>
    private sealed class DateWrapper(DateTime value) : IConvertible
    {
        public TypeCode GetTypeCode() => TypeCode.Object;
        public DateTime ToDateTime(IFormatProvider? provider) => value;
        public string ToString(IFormatProvider? provider) => value.ToString("yyyy-MM-dd");
        public override string ToString() => value.ToString("yyyy-MM-dd");

        // Membros obrigatórios de IConvertible — não usados neste contexto
        public bool ToBoolean(IFormatProvider? provider) => throw new InvalidCastException();
        public byte ToByte(IFormatProvider? provider) => throw new InvalidCastException();
        public char ToChar(IFormatProvider? provider) => throw new InvalidCastException();
        public decimal ToDecimal(IFormatProvider? provider) => throw new InvalidCastException();
        public double ToDouble(IFormatProvider? provider) => throw new InvalidCastException();
        public short ToInt16(IFormatProvider? provider) => throw new InvalidCastException();
        public int ToInt32(IFormatProvider? provider) => throw new InvalidCastException();
        public long ToInt64(IFormatProvider? provider) => throw new InvalidCastException();
        public sbyte ToSByte(IFormatProvider? provider) => throw new InvalidCastException();
        public float ToSingle(IFormatProvider? provider) => throw new InvalidCastException();
        public object ToType(Type conversionType, IFormatProvider? provider) => throw new InvalidCastException();
        public ushort ToUInt16(IFormatProvider? provider) => throw new InvalidCastException();
        public uint ToUInt32(IFormatProvider? provider) => throw new InvalidCastException();
        public ulong ToUInt64(IFormatProvider? provider) => throw new InvalidCastException();
    }
}
