using Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Api.Auth
{
    public static class JwtAuthExtensions
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration cfg)
        {
            var key = cfg["Jwt:Key"] ?? throw new AppException(AppErrorCode.Conflict,"Missing Jwt:Key");
            var issuer = cfg["Jwt:Issuer"];
            var audience = cfg["Jwt:Audience"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                        ValidIssuer = issuer,
                        ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };

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
                                detail = mustChange
                                    ? "You must change your password before accessing this resource."
                                    : "You do not have permission to access this resource."
                            };

                            await context.Response.WriteAsJsonAsync(payload);
                        }
                    };
                });

            return services;
        }
    }
}
