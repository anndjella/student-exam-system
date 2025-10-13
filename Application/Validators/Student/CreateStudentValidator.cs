using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTO.Students;
using FluentValidation;


namespace Application.Validators.Student
{
    
    public class CreateStudentValidator : AbstractValidator<CreateStudentRequest>
    {
        public CreateStudentValidator()
        {
            RuleSet("Create", () =>
            {
                Include(new PersonCommonCreate<CreateStudentRequest>());

                RuleFor(x => x.IndexNumber)
                            .NotEmpty()
                            .MaximumLength(20)
                            .Matches(@"^[0-9]{4}/[0-9]{1,6}$")
                            .WithMessage("Format of an Index Number must be YYYY/Number, e.g., 2024/1234.");

            });
        }
    }
}
