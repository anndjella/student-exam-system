using Application.Common.Abstractions;
using Application.Common.Errors;
using Application.Auth;
using Application.Services.Interfaces;
using Domain.Common;
using Domain.Entity;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public sealed class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;

        public UserService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task DeactivateByPersonIdAsync(int personId, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByPersonIdAsync(personId, ct);
            if (user is null) return;

            user.Deactivate();
            _uow.Users.Update(user);
        }

        public async Task ChangePasswordAsync(int userId, string newPasswordPlain, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct)
                ?? throw new AppException(AppErrorCode.NotFound, $"User with id {userId} not found.");

            user.SetPasswordHash(PasswordService.Hash(user, newPasswordPlain));
            user.MarkPasswordChanged();

            _uow.Users.Update(user);
        }
    }
}
