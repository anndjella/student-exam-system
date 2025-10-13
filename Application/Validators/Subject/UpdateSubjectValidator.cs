using Application.DTO.Subjects;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Subject
{
    public class UpdateSubjectValidator : AbstractValidator<UpdateSubjectRequest>
    {
        public UpdateSubjectValidator()
        {
            RuleSet("Update", () =>
            {
                When(x => x.Name is not null, () =>
                    RuleFor(x => x.Name!)
                        .NotEmpty().WithMessage("Name cannot be empty.")
                        .MaximumLength(100));

                When(x => x.ESPB is not null, () =>
                    RuleFor(x => x.ESPB!.Value)
                       .GreaterThan(0).WithMessage("ESPB must be > 0.")
                    .LessThanOrEqualTo(60).WithMessage("ESPB must be <= 60."));
            });
        }
    }
}
