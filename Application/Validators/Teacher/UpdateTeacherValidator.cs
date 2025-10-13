using Application.Common;
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

            });
        }
    }
}
