using Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Api.Auth
{
    public static class JwtAuthExtensions
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration cfg)
        {
            // same parameters as the Functions host (Application/Auth/JwtValidation.cs)
            var parameters = JwtValidation.CreateParameters(cfg);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = parameters;

                    options.Events = new JwtBearerEvents
                    {
                        OnForbidden = async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/problem+json";

                            var mustChange = context.HttpContext.User.FindFirstValue("mcp") == "true";

                            var payload = new
                            {
                                type = "https://errors.yourdomain.com/forbidden",
                                title = "Forbidden",
                                status = 403,
                                detail = JwtValidation.ForbiddenDetail(mustChange)
                            };

                            await context.Response.WriteAsJsonAsync(payload);
                        }
                    };
                });

            return services;
        }
    }
}
