using Application.Common;
using Application.DTO.Students;
using Application.DTO.Teachers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Teacher
{
    public class CreateTeacherValidator :AbstractValidator<CreateTeacherRequest>
    {
        public CreateTeacherValidator()
        {
            RuleSet("Create", () =>
            {
                Include(new PersonCommonCreate<CreateTeacherRequest>());

                RuleFor(x => x.Title)
                            .IsInEnum()
                            .WithMessage("Title is required and must be a valid value.");
                RuleFor(x => x.EmployeeNumber)
                            .NotEmpty()
                            .MaximumLength(9)
                            .Matches(@"^[0-9]{4}/[0-9]{4}$")
                            .WithMessage("Format of an Employee Number must be YYYY/Number, e.g., 2024/1234.")
                            .Must(index =>
                            {
                                var year = int.Parse(index.Substring(0, 4));
                                return year >= 1900;
                            })
                            .WithMessage("Employee number year cannot be earlier than 1900.")
                            .Must(index =>
                            {
                                var year = int.Parse(index.Substring(0, 4));
                                return year <= DateTime.UtcNow.Year;
                            })
                            .WithMessage("Employee number year cannot be in the future.");

            });
        }
    }
}
