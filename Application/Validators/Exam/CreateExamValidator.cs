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
                RuleFor(x => x.StudentID)
                  .GreaterThan(0)
                  .WithMessage("You must specify student.");

                RuleFor(x => x.SubjectID)
                  .GreaterThan(0)
                  .WithMessage("You must specify subject.");

                //RuleFor(x => x.ExaminerID)
                //  .GreaterThan(0)
                //  .WithMessage("You must specify examiner.");

                //RuleFor(x => x.SupervisorID)
                //    .Must((req, sup) => sup is null || sup > 0)
                //    .WithMessage("Supervisor ID must be greater than 0.");

                RuleFor(x => x.Grade)
                    .InclusiveBetween((byte)5, (byte)10)
                    .WithMessage("Grade must be between 5 and 10.");

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
