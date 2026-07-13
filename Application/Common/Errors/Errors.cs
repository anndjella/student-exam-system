using System;

namespace Application.Common.Errors
{
    public enum AppErrorCode
    {
        NotFound,
        Validation,
        Conflict,
        Forbidden,
        Unauthorized,
        BadRequest,
        Concurrency,
        Unexpected
    }

    public sealed class AppException : Exception
    {
        public AppErrorCode Code { get; }
        public object? Payload { get; }

        public AppException(AppErrorCode code, string message, object? payload = null)
            : base(message)
        {
            Code = code;
            Payload = payload;
        }
    }
}
