using System.Security.Claims;

namespace Api.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var sub = user.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("Missing 'sub' claim (user id).");

            return int.Parse(sub);
        }

        public static int GetPersonId(this ClaimsPrincipal user)
        {
            var pid = user.FindFirstValue("pid");
            if (string.IsNullOrWhiteSpace(pid))
                throw new InvalidOperationException("Missing 'pid' claim (person id).");

            return int.Parse(pid);
        }

        public static string GetRole(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Role) ?? "";

        public static bool MustChangePassword(this ClaimsPrincipal user)
        {
            var mcp = user.FindFirstValue("mcp");
            if (string.IsNullOrWhiteSpace(mcp)) return false;

            return string.Equals(mcp, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
