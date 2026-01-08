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

                When(x => x.ECTS is not null, () =>
                    RuleFor(x => x.ECTS!.Value)
                       .GreaterThan((byte)0).WithMessage("ESPB must be > 0.")
                    .LessThanOrEqualTo((byte)15).WithMessage("ESPB must be <= 15."));
            });
        }
    }
}
