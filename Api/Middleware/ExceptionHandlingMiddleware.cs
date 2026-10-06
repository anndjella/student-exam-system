using Infrastructure.Errors;
using Microsoft.AspNetCore.Mvc;
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
                // mapping is shared with the Functions host (Infrastructure/Errors/ExceptionMapper.cs)
                var error = ExceptionMapper.Map(ex);

                var problem = new ProblemDetails
                {
                    Status = error.Status,
                    Title = error.Title,
                    Detail = error.Detail,
                    Type = error.Type
                };
                if (error.Extensions is not null)
                    foreach (var kv in error.Extensions) problem.Extensions[kv.Key] = kv.Value!;

                // Log
                _log.Log(error.Level, ex, "{Title} ({Code})", error.Title, error.Code);

                // Response
                context.Response.Clear();
                context.Response.StatusCode = error.Status;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }
}
