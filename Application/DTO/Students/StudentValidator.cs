using Application.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Students
{
    public sealed class StudentValidator : AbstractValidator<CreateStudentRequest>
    {
        public StudentValidator()
        {
            RuleSet("Create", () =>
            {
                Include(new PersonCommonValidator<CreateStudentRequest>());

                RuleFor(x => x.IndexNumber)
                            .NotEmpty()
                            .MaximumLength(20)
                            .Matches(@"^[0-9]{4}/[0-9]{1,6}$")
                            .WithMessage("Format must be YYYY/Number, e.g., 2024/1234.");

            });

            RuleSet("Update", () =>
            {
                When(x => x.FirstName != null, () =>
                    RuleFor(x => x.FirstName!).NotEmpty().MaximumLength(100));

                When(x => x.LastName != null, () =>
                    RuleFor(x => x.LastName!).NotEmpty().MaximumLength(100));

                When(x => x.DateOfBirth != null, () =>
                    RuleFor(x => x.DateOfBirth!)
                        .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today)));

                When(x => x.IndexNumber != null, () =>
                    RuleFor(x => x.IndexNumber!)
                            .NotEmpty()
                            .MaximumLength(20)
                            .Matches(@"^[0-9]{4}/[0-9]{1,6}$")
                            .WithMessage("Format must be YYYY/Number, e.g., 2024/1234.")); ;
            });
        }

    }
}
