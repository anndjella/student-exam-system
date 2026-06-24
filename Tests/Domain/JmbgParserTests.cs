using Domain.Common;
using FluentAssertions;

namespace Tests.Domain;

public sealed class JmbgParserTests
{
    [Theory]
    [InlineData("0201995701231", 1995, 1, 2)]
    [InlineData("1503006701231", 2006, 3, 15)]
    public void TryGetDateOfBirth_ReturnsParsedDate_ForValidJmbg(
        string jmbg,
        int year,
        int month,
        int day)
    {
        var ok = JmbgParser.TryGetDateOfBirth(jmbg, out var dateOfBirth, out var error);

        ok.Should().BeTrue();
        error.Should().BeEmpty();
        dateOfBirth.Should().Be(new DateOnly(year, month, day));
    }

    [Theory]
    [InlineData("")]
    [InlineData("020199570123")]
    [InlineData("020199570123x")]
    [InlineData("3213995701231")]
    public void TryGetDateOfBirth_ReturnsError_ForInvalidJmbg(string jmbg)
    {
        var ok = JmbgParser.TryGetDateOfBirth(jmbg, out var dateOfBirth, out var error);

        ok.Should().BeFalse();
        dateOfBirth.Should().Be(default);
        error.Should().NotBeNullOrWhiteSpace();
    }
}
