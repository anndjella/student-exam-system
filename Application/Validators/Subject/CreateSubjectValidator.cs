using Application.DTO.Subjects;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Subject
{
    public class CreateSubjectValidator : AbstractValidator<CreateSubjectRequest>
    {
        public CreateSubjectValidator()
        {
            RuleSet("Create", () =>
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Name is required.")
                    .MaximumLength(100);

                RuleFor(x => x.ESPB)
                    .GreaterThan(0).WithMessage("ESPB must be > 0.")
                    .LessThanOrEqualTo(60).WithMessage("ESPB must be <= 60.");

            });
        }
    }
}
