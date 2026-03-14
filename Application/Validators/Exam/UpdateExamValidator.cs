using Application.DTO.Exams;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Exam
{
    public class UpdateExamValidator : AbstractValidator<UpdateExamRequest>
    {
        public UpdateExamValidator()
        {
            RuleSet("Update", () =>
            {
                RuleFor(x => x.Note)
                 .MaximumLength(500).WithMessage("Note is too long");

                When(x => x.Grade.HasValue, () =>
                {
                    RuleFor(x => x.Grade!.Value)
                        .InclusiveBetween((byte)5, (byte)10)
                        .WithMessage("Grade must be between 5 and 10.");

                    RuleFor(x => x.Note)
                        .NotEmpty().WithMessage("If you change the grade, you must enter a note.");
                });
            });
        }
    }
}

