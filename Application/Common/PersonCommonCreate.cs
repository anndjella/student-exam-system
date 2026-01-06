using Domain.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
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


                RuleFor(x => x.JMBG)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage("JMBG is required.")
                    .Length(13).WithMessage("JMBG must be 13 digits.")
                    .Must(JmbgValidation.IsAllDigits).WithMessage("JMBG must contain only digits.");
                    //.DependentRules(() =>
                    //{
                    //    RuleFor(x => x.JMBG)
                    //        .Must(JmbgValidation.RegionLooksSerbian).WithMessage("JMBG region is not for Serbia.")
                    //        .Must(JmbgValidation.ChecksumValid).WithMessage("Check digit of JMBG is not valid.");
                    //});
            });
            
        }
    }
}
