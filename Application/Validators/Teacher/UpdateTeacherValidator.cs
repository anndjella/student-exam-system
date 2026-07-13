using Application.Validators.Person;
using Application.DTO.Teachers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Teacher
{
    public class UpdateTeacherValidator : AbstractValidator<UpdateTeacherRequest>
    {
        public UpdateTeacherValidator()
        {
            RuleSet("Update", () =>
            {
                Include(new PersonCommonUpdate<UpdateTeacherRequest>());

                When(x => x.Title != null, () =>
                    RuleFor(x => x.Title)
                            .IsInEnum()
                            .WithMessage("Title is required and must be a valid value."));

                When(x => x.EmployeeNumber != null, () =>
                    RuleFor(x => x.EmployeeNumber!)
                            .ValidSchoolNumber(
                                "Employee number",
                                "Format of an Employee Number must be YYYY/Number, e.g., 2024/1234."));

            });
        }
    }
}
