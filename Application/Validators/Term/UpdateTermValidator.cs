using Application.DTO.Exams;
using Application.DTO.Term;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Term
{
    public class UpdateTermValidator : AbstractValidator<UpdateTermRequest>
    {
        public UpdateTermValidator()
        {
            RuleSet("Update", () =>
            {
                RuleFor(x => x.Name)
                    .Must(name => !string.IsNullOrWhiteSpace(name))
                    .When(x => x.Name is not null)
                     .WithMessage("Term name must not be empty or whitespace.");

                RuleFor(x => x.RegistrationEndDate)
                 .GreaterThan(x => x.RegistrationStartDate)
                 .When(x =>
                     x.RegistrationEndDate.HasValue &&
                     x.RegistrationStartDate.HasValue)
                 .WithMessage("Registration end date must be after registration start date.");

                RuleFor(x => x.StartDate)
                    .GreaterThan(x => x.RegistrationStartDate!.Value)
                    .When(x =>
                        x.StartDate.HasValue &&
                        x.RegistrationStartDate.HasValue)
                    .WithMessage("Term start date must be after registration start date.");

                RuleFor(x => x.StartDate)
                    .GreaterThan(x => x.RegistrationEndDate!.Value)
                    .When(x =>
                        x.StartDate.HasValue &&
                        x.RegistrationEndDate.HasValue)
                    .WithMessage("Term start date must be after registration end date.");

                RuleFor(x => x.EndDate)
                    .GreaterThan(x => x.StartDate!.Value)
                    .When(x =>
                        x.EndDate.HasValue &&
                        x.StartDate.HasValue)
                    .WithMessage("Term end date must be after term start date.");

                RuleFor(x => x.EndDate)
                    .GreaterThan(x => x.RegistrationStartDate!.Value)
                    .When(x =>
                        x.EndDate.HasValue &&
                        x.RegistrationStartDate.HasValue)
                    .WithMessage("Term end date must be after registration start date.");

                RuleFor(x => x.EndDate)
                    .GreaterThan(x => x.RegistrationEndDate!.Value)
                    .When(x =>
                        x.EndDate.HasValue &&
                        x.RegistrationEndDate.HasValue)
                    .WithMessage("Term end date must be after registration end date.");
            });
        }
    }
}
