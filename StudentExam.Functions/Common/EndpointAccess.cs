namespace StudentExam.Functions.Common;

// Replaces the [Authorize]/[AllowAnonymous] attributes the controllers had.
// Roles are "any of", same as [Authorize(Roles = "A,B")].
public sealed record EndpointAccess(
    bool AllowAnonymous,
    bool RequirePasswordChanged,
    IReadOnlyList<string> Roles)
{
    // [AllowAnonymous]
    public static EndpointAccess Anonymous { get; } = new(true, false, []);

    // [Authorize] / [Authorize(Roles = ...)]
    public static EndpointAccess Authenticated(params string[] roles) => new(false, false, roles);

    // [Authorize(Policy = "PasswordChanged")] (+ optional roles)
    public static EndpointAccess PasswordChanged(params string[] roles) => new(false, true, roles);
}
