using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auth
{

    public sealed class AuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly TokenService _tokens;

        public AuthService(IUnitOfWork uow, TokenService tokens)
        {
            _uow = uow;
            _tokens = tokens;
        }

        public async Task<string> LoginAsync(string username, string password, CancellationToken ct)
        {
            var user = await _uow.Users.GetByUsernameAsync(username, ct);

            if (user is null)
                throw new AppException(AppErrorCode.Unauthorized, "Invalid credentials.");

            if (!user.isActive)
                throw new AppException(AppErrorCode.Forbidden, "Account is inactive.");

            if (!PasswordService.Verify(user, password))
                throw new AppException(AppErrorCode.Unauthorized, "Invalid credentials.");

            try
            {
                return _tokens.CreateToken(user);
            }
            catch (ArgumentNullException ex)
            {
                throw new AppException(
                    AppErrorCode.Unexpected,
                    "Token configuration is missing (Jwt key).",
                    payload: new { ex.ParamName }
                );
            }
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is null)
                throw new AppException(AppErrorCode.NotFound, "User not found.");

            if (!user.isActive)
                throw new AppException(AppErrorCode.Forbidden, "Account is inactive.");

            if (!PasswordService.Verify(user, currentPassword))
                throw new AppException(AppErrorCode.Unauthorized, "Invalid current password.");

            user.SetPasswordHash(PasswordService.Hash(user, newPassword));
            user.MarkPasswordChanged();

            await _uow.CommitAsync(ct);
        }
    }
}
