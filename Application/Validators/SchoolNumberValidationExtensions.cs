using FluentValidation;
using System;

namespace Application.Validators
{
    public static class SchoolNumberValidationExtensions
    {
        private const string Pattern = @"^[0-9]{4}/[0-9]{4}$";

        public static IRuleBuilderOptions<T, string> ValidSchoolNumber<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            string fieldName,
            string formatMessage)
        {
            return ruleBuilder
                .NotEmpty()
                .MaximumLength(9)
                .Matches(Pattern)
                .WithMessage(formatMessage)
                .Must(HasYearNotEarlierThan1900)
                .WithMessage($"{fieldName} year cannot be earlier than 1900.")
                .Must(HasYearNotInFuture)
                .WithMessage($"{fieldName} year cannot be in the future.");
        }

        private static bool HasYearNotEarlierThan1900(string value)
            => TryGetYear(value, out var year) && year >= 1900;

        private static bool HasYearNotInFuture(string value)
            => TryGetYear(value, out var year) && year <= DateTime.UtcNow.Year;

        private static bool TryGetYear(string value, out int year)
        {
            year = default;

            if (string.IsNullOrWhiteSpace(value) || value.Length < 4)
                return false;

            return int.TryParse(value.Substring(0, 4), out year);
        }
    }
}
