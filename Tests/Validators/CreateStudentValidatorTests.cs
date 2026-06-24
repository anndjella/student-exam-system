using Application.DTO.Students;
using Application.Validators.Student;
using FluentAssertions;
using FluentValidation;

namespace Tests.Validators;

public sealed class CreateStudentValidatorTests
{
    private readonly CreateStudentValidator _validator = new();

    [Fact]
    public void Validate_AcceptsValidStudentRequest()
    {
        var request = ValidRequest();

        var result = _validator.Validate(request, options => options.IncludeRuleSets("Create"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("2024-1234")]
    [InlineData("1899/1234")]
    [InlineData("3024/1234")]
    public void Validate_RejectsInvalidIndexNumber(string indexNumber)
    {
        var request = ValidRequest();
        request.IndexNumber = indexNumber;

        var result = _validator.Validate(request, options => options.IncludeRuleSets("Create"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateStudentRequest.IndexNumber));
    }

    private static CreateStudentRequest ValidRequest() => new()
    {
        JMBG = "0201995701231",
        FirstName = "Ana",
        LastName = "Anic",
        IndexNumber = "2024/1234"
    };
}
