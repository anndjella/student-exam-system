using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Api.Filter
{
    public sealed class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var (status, title, detail, type) = Map(context.Exception);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Type = type
            };

            context.Result = new ObjectResult(problem) { StatusCode = status };
            context.ExceptionHandled = true;
        }

        private static (int status, string title, string detail, string type) Map(Exception ex)
        {
            if (ex is KeyNotFoundException)
                return (404, "Not Found", ex.Message, "https://httpstatuses.com/404");

            if (ex is ArgumentException)
                return (400, "Bad Request", ex.Message, "https://httpstatuses.com/400");

            if (ex is InvalidOperationException)
                return (409, "Conflict", ex.Message, "https://httpstatuses.com/409");

            if (ex is DbUpdateException dbu)
            {
                var sqlEx = dbu.GetBaseException() as SqlException;
                var rawMsg = dbu.InnerException?.Message ?? dbu.Message;

                if (sqlEx != null)
                {
                    // UNIQUE / PK
                    if (sqlEx.Number is 2627 or 2601)
                    {
                       return (409, "Conflict", "Unique constraint violated.", "https://httpstatuses.com/409");
                    }

                    // CHECK / FK
                    if (sqlEx.Number == 547)
                    {
                        if (rawMsg.Contains("CK_Student_IndexNumber_Format", StringComparison.OrdinalIgnoreCase))
                            return (400, "Invalid index format", "Index must be in the format YYYY/Number.", "https://httpstatuses.com/400");
                        if (rawMsg.Contains("CK_Person_JMBG_DateOfBirth", StringComparison.OrdinalIgnoreCase))
                            return (400, "Invalid date for JMBG", "DateOfBirth must match the date encoded in JMBG.", "https://httpstatuses.com/400");

                        return (400, "Constraint failed", "A database constraint was violated.", "https://httpstatuses.com/400");
                    }
                }

                return (400, "Database Error", "Database update failed.", "https://httpstatuses.com/400");
            }

            // Fallback
            return (500, "Internal Server Error", "An unexpected error occurred.", "https://httpstatuses.com/500");
        }
    }
}
