using Application.DTO.Term;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Term
{
    public class CreateTermValidator : AbstractValidator<CreateTermRequest>
    {
       public CreateTermValidator()
        {
            RuleSet("Create", () =>
            {
                RuleFor(x => x.TermName)
                    .NotEmpty();

                RuleFor(x => x.RegistrationEndDate)
                    .GreaterThan(x => x.RegistrationStartDate)
                    .WithMessage("Registration end date must be after registration start date.");

                RuleFor(x => x.StartDate)
                    .GreaterThan(x => x.RegistrationStartDate)
                    .GreaterThan(x => x.RegistrationEndDate)
                    .WithMessage("Term start date must be after registration interval.");


                RuleFor(x => x.EndDate)
                    .GreaterThan(x => x.StartDate)
                    .GreaterThan(x => x.RegistrationStartDate)
                    .GreaterThan(x => x.RegistrationEndDate)
                    .WithMessage("Term end date must be after registration interval date and term interval.");
            });
        }
    }
}
