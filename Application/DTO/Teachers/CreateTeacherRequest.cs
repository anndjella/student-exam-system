using Application.Common;
using Application.DTO.Students;
using Domain.Entity;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Teachers
{
    public class CreateTeacherRequest : IPersonCreate
    {
        public string JMBG { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateOnly DateOfBirth { get; set; }
        public Title Title { get; set; }
    }
    public sealed class CreateTeacherValidator : AbstractValidator<CreateTeacherRequest>
    {
        public CreateTeacherValidator()
        {
            Include(new PersonCommonValidator<CreateTeacherRequest>());
            RuleFor(x => x.Title)
                        .IsInEnum()
                        .WithMessage("Title is required and must be a valid value.");
        }
    }
}
