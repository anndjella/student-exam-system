using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auth
{
    public sealed class MustChangePasswordClearedRequirement : IAuthorizationRequirement { }

    public sealed class MustChangePasswordClearedHandler
        : AuthorizationHandler<MustChangePasswordClearedRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MustChangePasswordClearedRequirement requirement)
        {
            var mcp = context.User.FindFirstValue("mcp");

            if (string.Equals(mcp, "false", StringComparison.OrdinalIgnoreCase))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
