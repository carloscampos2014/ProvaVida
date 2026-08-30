using FluentAssertions;
using ProvaVida.Shared.Common;

namespace ProvaVida.Api.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Result_Ok_DeveRetornarSuccessTrue()
    {
        var result = Result.Ok();
        result.Success.Should().BeTrue();
        result.MessageErro.Should().BeEmpty();
    }

    [Fact]
    public void Result_Fail_DeveRetornarSuccessFalseComMensagem()
    {
        var result = Result.Fail("erro de teste");
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("erro de teste");
    }

    [Fact]
    public void ResultT_Ok_DeveRetornarSuccessTrueComDado()
    {
        var result = Result<int>.Ok(42);
        result.Success.Should().BeTrue();
        result.Data.Should().Be(42);
        result.MessageErro.Should().BeEmpty();
    }

    [Fact]
    public void ResultT_Fail_DeveRetornarSuccessFalseSemDado()
    {
        var result = Result<int>.Fail("falha");
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("falha");
        result.Data.Should().Be(default);
    }
}
