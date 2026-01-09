using Application.DTO.Enrollments;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public sealed class CreateEnrollmentsValidator : AbstractValidator<BulkEnrollByIndexYearRequest>
    {
        public CreateEnrollmentsValidator()
        {
            RuleSet("Create", () =>
            {
                RuleFor(e => e.IndexStartYear).
                        NotNull().WithMessage("Index start year can not be null").
                        GreaterThan(1980).WithMessage("Index start year must be grater than 1980").
                        LessThan(DateTime.Now.Year).WithMessage("Future enrollments can not be made.");

                RuleFor(e => e.SchoolYearId)
                     .GreaterThan(0)
                     .WithMessage("School year is required.");

                RuleFor(e => e.SubjectIds)
                    .NotNull()
                    .WithMessage("SubjectId is required.")
                    .NotEmpty()
                    .WithMessage("At least one subject must be selected.");
            });
        }
    }
}
