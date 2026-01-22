using Application.DTO.Exams;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Exam
{
    public class CreateExamValidator : AbstractValidator<CreateExamRequest>
    {
        public CreateExamValidator()
        {
            RuleSet("Create", () =>
            {
               RuleFor(x => x.Grade)
                   .Must(g => g is null || (g >= 5 && g <= 10))
                   .WithMessage("Grade must be between 5 and 10 or null.");

                RuleFor(x => x.Date)
                   .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                    .WithMessage("Date cannot be in the future.");

                RuleFor(x => x.Note)
                   .MaximumLength(500)
                   .WithMessage("Note is too long");
            });
        }
    }
}
