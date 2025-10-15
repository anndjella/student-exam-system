using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Api.Filter
{
    public sealed class ApiExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ApiExceptionFilter> _log;

        public ApiExceptionFilter(IWebHostEnvironment env, ILogger<ApiExceptionFilter> log)
        {
            _env = env;
            _log = log;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            var (status, title, detail, code) = Map(ex);

            _log.LogError(ex, "API error: {Code}", code);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{status}",
            };
            problem.Extensions["code"] = code;

            context.Result = new ObjectResult(problem) { StatusCode = status };
            context.ExceptionHandled = true;
        }

        private (int status, string title, string detail, string code) Map(Exception ex)
            => ex switch
            {
                KeyNotFoundException => (404, "Not Found", ex.Message, "not_found"),
                ArgumentException => (400, "Bad Request", ex.Message, "bad_request"),

                ValidationException vex => (400, "Validation Error", JoinValidation(vex), "validation_error"),

                InvalidOperationException => (409, "Conflict", ex.Message, "conflict"),

                DbUpdateException dbu => MapDb(dbu),

                _ => (500, "Internal Server Error", SafeDetail(ex), "internal_error"),
            };

        private static string JoinValidation(ValidationException vex)
        {
            var sb = new StringBuilder("Validation failed: ");
            foreach (var g in vex.Errors.GroupBy(e => e.PropertyName))
                sb.Append($"{g.Key} [{string.Join("; ", g.Select(e => e.ErrorMessage))}]; ");
            return sb.ToString().Trim();
        }

        private (int status, string title, string detail, string code) MapDb(DbUpdateException dbu)
        {
            var raw = dbu.InnerException?.Message ?? dbu.Message;

            if (ContainsAny(raw, "UNIQUE", "unique", "duplicate"))
                return (409, "Conflict", "Unique constraint violated.", "unique_violation");

            if (ContainsAny(raw, "FOREIGN KEY", "foreign key", "reference"))
                return (409, "Constraint failed", "Operation not allowed due to referential integrity.", "fk_violation");

            if (ContainsAny(raw, "CHECK constraint", "check constraint"))
                return (400, "Constraint failed", "A check constraint was violated.", "check_violation");

            return (400, "Database Error", SafeDetail(dbu), "db_error");
        }

        private string SafeDetail(Exception ex)
            => _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.";

        private static bool ContainsAny(string? haystack, params string[] needles)
            => !string.IsNullOrEmpty(haystack) && needles.Any(n =>
                   haystack.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
