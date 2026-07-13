using FluentValidation;

namespace Application.Validators.Person
{
    public class PersonCommonUpdate<T> : AbstractValidator<T> where T : IPersonUpdate
    {
        public PersonCommonUpdate()
        {
            RuleSet("Update", () =>
            {
                When(x => x.FirstName != null, () =>
                    RuleFor(x => x.FirstName)
                        .NotEmpty()
                        .WithMessage("First name is required.")
                        .MaximumLength(50));

                When(x => x.LastName != null, () =>
                    RuleFor(x => x.LastName)
                        .NotEmpty()
                        .WithMessage("Last name is required.")
                        .MaximumLength(50));
            });
        }
    }
}
