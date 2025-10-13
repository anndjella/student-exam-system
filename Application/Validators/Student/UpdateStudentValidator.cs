using Application.Common;
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
                            .NotEmpty()
                            .MaximumLength(20)
                            .Matches(@"^[0-9]{4}/[0-9]{1,6}$")
                            .WithMessage("Format of an Index Number must be YYYY/Number, e.g., 2024/1234.")); ;
            });
        }

    }
}
