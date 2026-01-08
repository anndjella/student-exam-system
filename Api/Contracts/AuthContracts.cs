namespace Api.Contracts
{
    public sealed record LoginRequest(string Username, string Password);
    public sealed record LoginResponse(string Token);
    public sealed record ChangePasswordRequest(string CurrentPassword,string NewPassword);
}
