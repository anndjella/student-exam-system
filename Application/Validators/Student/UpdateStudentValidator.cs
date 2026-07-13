using Application.Validators.Person;
using Application.DTO.Students;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Student
{
    public class UpdateStudentValidator : AbstractValidator<UpdateStudentRequest>
    {
        public UpdateStudentValidator()
        {
            RuleSet("Update", () =>
            {
                Include(new PersonCommonUpdate<UpdateStudentRequest>());          

                When(x => x.IndexNumber != null, () =>
                    RuleFor(x => x.IndexNumber!)
                            .ValidSchoolNumber(
                                "Index",
                                "Format of an Index Number must be YYYY/Number, e.g., 2024/1234."));
            });
        }

    }
}
