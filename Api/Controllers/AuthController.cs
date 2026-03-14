using Application.Auth;
using Application.DTO.Auth;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly AuthService _auth; 

        public AuthController(AuthService auth) => _auth = auth;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login(LoginRequest req, CancellationToken ct)
        {
            var token = await _auth.LoginAsync(req.Username, req.Password, ct);
            return Ok(token);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody, CustomizeValidator(RuleSet = "ChangePassword")] ChangePasswordRequest req, CancellationToken ct)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _auth.ChangePasswordAsync(userId, req.CurrentPassword, req.NewPassword, ct);
            return NoContent();
        }
    }
}
