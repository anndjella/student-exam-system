using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
                return sql.Number switch
                {
                    2627 or 2601 => (409, "Conflict", "Unique constraint violated.",
                                     TypeUri(AppErrorCode.Conflict), nameof(AppErrorCode.Conflict), null, LogLevel.Warning),
                    547 => (409, "Conflict", "Resource is referenced by other data (foreign key).",
                                     TypeUri(AppErrorCode.Conflict), nameof(AppErrorCode.Conflict), null, LogLevel.Warning),
                    _ => (400, "Bad request", "A database error occurred.",
                                     TypeUri(AppErrorCode.BadRequest), nameof(AppErrorCode.BadRequest), null, LogLevel.Warning)
                };
            }

            return (500, "Unexpected error", "An unexpected error occurred. Please try again.",
                    TypeUri(AppErrorCode.Unexpected), nameof(AppErrorCode.Unexpected), null, LogLevel.Error);
        }

        private static string TypeUri(AppErrorCode code) => $"https://errors.yourdomain.com/{code}";

    }
}

