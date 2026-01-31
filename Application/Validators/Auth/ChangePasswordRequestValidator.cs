using Application.DTO.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Auth
{
    public sealed class ChangePasswordRequestValidator
     : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {

            RuleSet("ChangePassword", () =>
            {
                RuleFor(x => x.CurrentPassword)
                    .NotEmpty()
                    .WithMessage("Current password is required.");

                RuleFor(x => x.NewPassword)
                    .NotEmpty()
                    .WithMessage("New password is required.")
                    .MinimumLength(8)
                    .WithMessage("New password must be at least 8 characters long.")
                    .Matches(@"\d")
                    .WithMessage("New password must contain at least one number.");
            });

        }
    }
}
