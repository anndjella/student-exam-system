using Application.Validators.Person;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                            .ValidSchoolNumber(
                                "Index",
                                "Format of an Index Number must be YYYY/Number, e.g., 2024/1234.");

            });
        }
    }
}
