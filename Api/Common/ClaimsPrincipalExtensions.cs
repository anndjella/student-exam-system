using System.Security.Claims;

namespace Api.Common
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetPid(this ClaimsPrincipal user, out int pid)
            => int.TryParse(user.FindFirstValue("pid"), out pid);
    }
}
