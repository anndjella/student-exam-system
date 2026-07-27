using Domain.Common;
using FluentValidation;

namespace Application.Validators.Person
{
    public class PersonCommonCreate<T> : AbstractValidator<T> where T : IPersonCreate
    {
        public PersonCommonCreate()
        {
            RuleSet("Create", () =>
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .WithMessage("First name is required.")
                    .MaximumLength(50);

                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .WithMessage("Last name is required.")
                    .MaximumLength(50);

                RuleFor(x => x.Email)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("Email is required.")
                    .MaximumLength(254)
                    .EmailAddress()
                    .WithMessage("Email is not valid.");

                RuleFor(x => x.JMBG)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage("JMBG is required.")
                    .Length(13).WithMessage("JMBG must be 13 digits.")
                    .Must(JmbgValidation.IsAllDigits).WithMessage("JMBG must contain only digits.")
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.JMBG)
                            .Must(JmbgValidation.RegionLooksSerbian).WithMessage("JMBG region is not for Serbia.");
                    });
            });
        }
    }
}
