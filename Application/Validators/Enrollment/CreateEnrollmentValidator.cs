using Application.Validators.Person;
using Application.DTO.Enrollments;
using FluentValidation;

namespace Application.Validators.Enrollment
{
    public sealed class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentRequest>
    {
        public CreateEnrollmentValidator()
        {
            RuleSet("Create", () =>
            {
                RuleFor(x => x.StudentIndex)
                    .ValidSchoolNumber(
                        "Index",
                        "Format of an Index Number must be YYYY/Number, e.g., 2024/1234.");

                RuleFor(x => x.SubjectCode)
                    .NotEmpty().WithMessage("Subject code is required.")
                    .MaximumLength(6);
            });
        }
    }
}
