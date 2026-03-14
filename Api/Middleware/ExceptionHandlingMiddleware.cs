using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Api.Middleware
{

    public sealed class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _log;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> log) => _log = log;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var (status, title, detail, type, code, extensions, level) = Map(ex);

                var problem = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = detail,
                    Type = type
                };
                if (extensions is not null)
                    foreach (var kv in extensions) problem.Extensions[kv.Key] = kv.Value!;

                // Log
                _log.Log(level, ex, "{Title} ({Code})", title, code);

                // Response
                context.Response.Clear();
                context.Response.StatusCode = status;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }

        private static (int, string, string, string, string, IDictionary<string, object>?, LogLevel) Map(Exception ex)
        {
            if (ex is AppException app)
            {
                var (status, title) = app.Code switch
                {
                    AppErrorCode.NotFound => (404, "Resource not found"),
                    AppErrorCode.Validation => (422, "Validation failed"),
                    AppErrorCode.Conflict => (409, "Conflict"),
                    AppErrorCode.Forbidden => (403, "Forbidden"),
                    AppErrorCode.Unauthorized => (401, "Unauthorized"),
                    AppErrorCode.BadRequest => (400, "Bad request"),
                    AppErrorCode.Concurrency => (412, "Precondition failed"),
                    _ => (500, "Unexpected error")
                };
                var ext = app.Payload is null ? null : new Dictionary<string, object> { ["payload"] = app.Payload };
                var level = status >= 500 ? LogLevel.Error : LogLevel.Information;
                return (status, title, app.Message, TypeUri(app.Code), app.Code.ToString(), ext, level);
            }

            if (ex is DbUpdateException dbu && dbu.InnerException is SqlException sql)
            {
                // Extract constraint name if present (helpful for debugging and client messages)
                var constraint = TryExtractConstraintName(sql.Message);

                // Always include constraint in extensions (safe and super useful)
                IDictionary<string, object>? ext = constraint is null
                    ? null
                    : new Dictionary<string, object> { ["constraint"] = constraint };

                // Unique constraint violations
                if (sql.Number is 2627 or 2601)
                {
                    return (409, "Conflict", "Unique constraint violated.",
                        TypeUri(AppErrorCode.Conflict), nameof(AppErrorCode.Conflict), ext, LogLevel.Warning);
                }

                // Constraint violation: FK or CHECK (both commonly 547)
                if (sql.Number == 547)
                {
                    var msg = sql.Message;

                    if (msg.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase))
                    {
                        return (409, "Conflict", "Foreign key constraint violated.",
                            TypeUri(AppErrorCode.Conflict), nameof(AppErrorCode.Conflict), ext, LogLevel.Warning);
                    }

                    if (msg.Contains("CHECK constraint", StringComparison.OrdinalIgnoreCase))
                    {
                        // This is usually validation (data out of allowed range)
                        return (422, "Validation failed", "Check constraint violated.",
                            TypeUri(AppErrorCode.Validation), nameof(AppErrorCode.Validation), ext, LogLevel.Warning);
                    }

                    // Unknown 547 flavor
                    return (409, "Conflict", "A database constraint was violated.",
                        TypeUri(AppErrorCode.Conflict), nameof(AppErrorCode.Conflict), ext, LogLevel.Warning);
                }

                // Not-null constraint violation (often 515)
                if (sql.Number == 515)
                {
                    return (422, "Validation failed", "A required field was missing (NULL insert).",
                        TypeUri(AppErrorCode.Validation), nameof(AppErrorCode.Validation), null, LogLevel.Warning);
                }

                // Truncation (string too long) (often 2628)
                if (sql.Number == 2628)
                {
                    return (422, "Validation failed", "A value was too long for a column.",
                        TypeUri(AppErrorCode.Validation), nameof(AppErrorCode.Validation), null, LogLevel.Warning);
                }

                return (400, "Bad request", "A database error occurred.",
                    TypeUri(AppErrorCode.BadRequest), nameof(AppErrorCode.BadRequest), null, LogLevel.Warning);
            }

            return (500, "Unexpected error", "An unexpected error occurred. Please try again.",
                TypeUri(AppErrorCode.Unexpected), nameof(AppErrorCode.Unexpected), null, LogLevel.Error);
        }
        private static string? TryExtractConstraintName(string message)
        {
            // Typical SQL Server message includes: constraint "CK_Person_DateOfBirth_Range"
            var m = Regex.Match(message, "constraint\\s+\"(?<c>[^\"]+)\"", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups["c"].Value : null;
        }
        private static string TypeUri(AppErrorCode code) => $"https://errors.yourdomain.com/{code}";

    }
}

