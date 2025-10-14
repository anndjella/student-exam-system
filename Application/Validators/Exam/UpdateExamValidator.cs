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
                When(x => x.Grade is not null, () =>
                    RuleFor(x => x.Grade!.Value)
                        .InclusiveBetween((byte)5, (byte)10)
                        .WithMessage("Grade must be between 5 and 10."));


                When(x => x.Grade != null, () =>
                     RuleFor(x => x.Note)
                    .NotEmpty().WithMessage("If you want to change the grade, you must enter a note")
                    .MaximumLength(500).WithMessage("Note is too long"));
            });
        }
    }
}

