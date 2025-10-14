using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text;
using FluentValidation;
//using System.ComponentModel.DataAnnotations;

namespace Api.Filter
{
    public sealed class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var (status, title, detail, type, code) = Map(context.Exception);

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

        private static (int status, string title, string detail, string type, string code) Map(Exception ex)
        {
            // 404
            if (ex is KeyNotFoundException)
                return (404, "Not Found", ex.Message, "https://httpstatuses.com/404", "not_found");

            // 400 – application-level bad args
            if (ex is ArgumentException)
                return (400, "Bad Request", ex.Message, "https://httpstatuses.com/400", "bad_request");

            // 400 – DTO validation (FluentValidation)
            if (ex is ValidationException vex)
            {
                var sb = new StringBuilder("Validation failed: ");
                foreach (var g in vex.Errors.GroupBy(e => e.PropertyName))
                    sb.Append($"{g.Key} [{string.Join("; ", g.Select(e => e.ErrorMessage))}]; ");
                return (400, "Validation Error", sb.ToString().Trim(), "https://httpstatuses.com/400", "validation_error");
            }

            // 409 – business conflicts you might throw from services
            if (ex is InvalidOperationException)
                return (409, "Conflict", ex.Message, "https://httpstatuses.com/409", "conflict");

            // DB exceptions (provider-agnostic)
            if (ex is DbUpdateException dbu)
            {
                var raw = dbu.InnerException?.Message ?? dbu.Message;
                var name = ExtractConstraintName(raw);

                // ---- UNIQUE / INDEX conflicts ----
                if (ContainsAny(raw, "UNIQUE", "unique", "duplicate", "is already", "violates unique") ||
                    ContainsAny(name, "UX_", "IX_") ||
                    ContainsAny(raw, "UQ_", "uq_"))
                {
                    // your named uniques
                    if (Contains(name, "UX_Exam_PassOnce"))
                        return Problem(409, "Conflict", "The exam is already passed; passing is allowed only once.", "unique_violation", "exam_pass_once");

                    if (Contains(name, "UX_Exam_Student_Subject_Date"))
                        return Problem(409, "Conflict", "An exam for the same student, subject and date already exists.", "unique_violation", "exam_duplicate_same_day");

                    // column-based uniques
                    if (Contains(raw, "Person") && Contains(raw, "JMBG") || Contains(name, "JMBG"))
                        return Problem(409, "Conflict", "JMBG already exists.", "unique_violation", "person_jmbg_unique");

                    if (Contains(raw, "Student") && Contains(raw, "IndexNumber") || Contains(name, "IndexNumber"))
                        return Problem(409, "Conflict", "Index number already exists.", "unique_violation", "student_index_unique");

                    return Problem(409, "Conflict", "Unique constraint violated.", "unique_violation", "unique_violation");
                }

                // ---- CHECK / semantic constraint violations ----
                if (ContainsAny(raw, "CHECK constraint", "check constraint", "violates check") ||
                    ContainsAny(name, "CK_"))
                {
                    if (Contains(name, "CK_Subject_Espb"))
                        return Problem(400, "Invalid ESPB", "ESPB must be between 1 and 60.", "check_violation", "subject_espb_range");

                    if (Contains(name, "CK_Exam_Grade"))
                        return Problem(400, "Invalid grade", "Grade must be between 5 and 10.", "check_violation", "exam_grade_range");

                    if (Contains(name, "CK_Person_DateOfBirth_Range"))
                        return Problem(400, "Invalid date of birth", "Date of birth must be between 1900-01-01 and 2008-12-31.", "check_violation", "dob_range");

                    if (Contains(name, "CK_Person_JMBG_13Digits"))
                        return Problem(400, "Invalid JMBG format", "JMBG must have exactly 13 digits and contain only digits.", "check_violation", "jmbg_format");

                    if (Contains(name, "CK_Person_JMBG_DateOfBirth"))
                        return Problem(422, "JMBG/DateOfBirth mismatch", "JMBG does not match the date of birth.", "check_violation", "jmbg_dob_mismatch");

                    if (Contains(name, "CK_Student_IndexNumber_Format"))
                        return Problem(400, "Invalid index format", "Index number must be in the form YYYY/NNN… and contain only digits and '/'.", "check_violation", "index_format");

                    return Problem(400, "Constraint failed", "A database constraint was violated.", "check_violation", "constraint_failed");
                }

                // ---- FK / reference conflicts (delete/update restrictions) ----
                if (ContainsAny(raw, "FOREIGN KEY constraint", "reference constraint", "REFERENCE constraint", "FK constraint failed"))
                {
                    // heuristics by keywords
                    if (Contains(raw, "Subject"))
                        return Problem(409, "Cannot delete subject", "The subject cannot be deleted because there are related exams.", "fk_violation", "subject_in_use");

                    if (Contains(raw, "Teacher"))
                        return Problem(409, "Cannot delete teacher", "The teacher cannot be deleted because they are assigned to exams.", "fk_violation", "teacher_in_use");

                    if (Contains(raw, "Student"))
                        return Problem(409, "Cannot delete student", "The student cannot be deleted because there are related exam records.", "fk_violation", "student_in_use");

                    return Problem(409, "Constraint failed", "Operation is not allowed due to referential integrity.", "fk_violation", "fk_violation");
                }

                // generic DB failure
                return Problem(400, "Database Error", "Database update failed.", "db_error", "db_error");
            }

            // 500 fallback
            return (500, "Internal Server Error", "An unexpected error occurred.", "https://httpstatuses.com/500", "internal_error");
        }

        private static (int status, string title, string detail, string type, string code)
            Problem(int status, string title, string detail, string typeCode, string code)
            => (status, title, detail, $"https://httpstatuses.com/{status}", typeCode);

        // ---- helpers ----
        private static string? ExtractConstraintName(string message)
        {
            if (string.IsNullOrEmpty(message)) return null;

            // common pattern: constraint/index 'NAME'
            var s = message.IndexOf('\'');
            if (s >= 0)
            {
                var e = message.IndexOf('\'', s + 1);
                if (e > s) return message.Substring(s + 1, e - s - 1);
            }

            // try to locate well-known prefixes (CK_/UX_/IX_/UQ_)
            foreach (var token in new[] { "CK_", "UX_", "IX_", "UQ_", "uq_", "ck_", "ix_" })
            {
                var i = message.IndexOf(token, StringComparison.OrdinalIgnoreCase);
                if (i >= 0)
                {
                    var j = i;
                    while (j < message.Length && !char.IsWhiteSpace(message[j]) && message[j] != '.' && message[j] != ':' && message[j] != ',' && message[j] != ')')
                        j++;
                    return message.Substring(i, j - i);
                }
            }

            return null;
        }

        private static bool Contains(string? haystack, string needle)
            => haystack?.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool ContainsAny(string? haystack, params string[] needles)
            => needles.Any(n => Contains(haystack, n));
    }

}
