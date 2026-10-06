using Application.Common.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Application.Auth
{
    // One source of truth for how tokens are validated, so the Api (JwtBearer)
    // and the Functions host accept exactly the same tokens.
    public static class JwtValidation
    {
        public static TokenValidationParameters CreateParameters(IConfiguration cfg)
        {
            var key = cfg["Jwt:Key"] ?? throw new AppException(AppErrorCode.Conflict, "Missing Jwt:Key");
            var issuer = cfg["Jwt:Issuer"];
            var audience = cfg["Jwt:Audience"];

            return new TokenValidationParameters
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
        }

        public static string ForbiddenDetail(bool mustChangePassword)
            => mustChangePassword
                ? "You must change your password before accessing this resource."
                : "You do not have permission to access this resource.";
    }
}
