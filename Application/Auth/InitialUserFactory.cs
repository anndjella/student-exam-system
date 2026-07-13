using Domain.Common;
using Domain.Entity;
using Domain.Enums;

namespace Application.Auth
{
    public static class InitialUserFactory
    {
        public static User Create(UserRole role, string username, string jmbg)
        {
            var initialPlain = CredentialsGenerator.InitialPasswordPlain(jmbg);
            var user = new User(role, username, passwordHash: "TEMP");
            user.SetPasswordHash(PasswordService.Hash(user, initialPlain));

            return user;
        }
    }
}
