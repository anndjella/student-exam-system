using Application.DTO.Enrollments;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Enrollment
{
    public sealed class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentRequest>
    {
        public CreateEnrollmentValidator()
        {
            RuleSet("Create", () =>
            {
                RuleFor(x => x.StudentIndex)
                    .NotEmpty().WithMessage("Student index is required.")
                    .Matches(@"^[0-9]{4}/[0-9]{4}$")
                    .WithMessage("Format of an Index Number must be YYYY/Number, e.g., 2024/1234.");

                RuleFor(x => x.SubjectCode)
                    .NotEmpty().WithMessage("Subject code is required.")
                    .MaximumLength(6);
            });
        }
    }
}
