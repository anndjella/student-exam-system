using System.Security.Claims;
using Application.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace StudentExam.Functions.Common;

public enum AuthOutcome
{
    NoToken,
    InvalidToken,
    Authenticated
}

public sealed record AuthResult(AuthOutcome Outcome, ClaimsPrincipal? Principal = null);

// Does what JwtBearer did in the Api: reads "Authorization: Bearer ..." and validates it
// with the same parameters (Application/Auth/JwtValidation.cs).
public sealed class JwtAuthenticator
{
    private readonly TokenValidationParameters _parameters;

    // MapInboundClaims = true matches JwtBearer's default, so "role"/"nameid" come back
    // as ClaimTypes.Role/ClaimTypes.NameIdentifier just like in the controllers
    private readonly JsonWebTokenHandler _handler = new() { MapInboundClaims = true };

    public JwtAuthenticator(IConfiguration configuration)
    {
        _parameters = JwtValidation.CreateParameters(configuration);
    }

    public async Task<AuthResult> AuthenticateAsync(HttpRequest req)
    {
        string? header = req.Headers.Authorization;
        if (string.IsNullOrWhiteSpace(header) ||
            !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return new(AuthOutcome.NoToken);

        var token = header["Bearer ".Length..].Trim();
        if (token.Length == 0)
            return new(AuthOutcome.NoToken);

        var result = await _handler.ValidateTokenAsync(token, _parameters);
        return result.IsValid
            ? new(AuthOutcome.Authenticated, new ClaimsPrincipal(result.ClaimsIdentity))
            : new(AuthOutcome.InvalidToken);
    }
}
