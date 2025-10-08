using Domain.Validation;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public class PersonCommonValidator<T> : AbstractValidator<T> where T : IPersonCreate
    {
        public PersonCommonValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(100);

            var minYear = 1900;
            var maxYear = 2008;
            var minDate = new DateOnly(minYear, 1, 1);
            var maxDate = new DateOnly(maxYear, 12, 31);

            RuleFor(x => x.JMBG)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("JMBG is required.")
                .Length(13).WithMessage("JMBG must be 13 digits.")
                .Must(JMBGValidation.IsAllDigits).WithMessage("JMBG must contain only digits.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.JMBG)
                        .Must(JMBGValidation.RegionLooksSerbian).WithMessage("JMBG region is not for Serbia.")
                        .Must(JMBGValidation.ChecksumValid).WithMessage("Check digit of JMBG is not valid.");
                });

            RuleFor(x => x)
                .Must(x => JMBGValidation.TryParseDate(x.JMBG, out var dob) && dob == x.DateOfBirth)
                .WithMessage("DateOfBirth doesn't match the date from JMBG.")
                .When(x => !string.IsNullOrEmpty(x.JMBG)
                            && x.JMBG.Length == 13
                            && JMBGValidation.IsAllDigits(x.JMBG));
        }
    }
}
