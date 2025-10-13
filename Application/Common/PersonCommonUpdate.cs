using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public class PersonCommonUpdate<T> :  AbstractValidator<T> where T : IPersonUpdate
    {
        public PersonCommonUpdate()
        {
            RuleSet("Update", () =>
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .WithMessage("First name is required.")
                    .MaximumLength(100);

                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .WithMessage("Last name is required.")
                    .MaximumLength(100);
            });
        }

    }
}
