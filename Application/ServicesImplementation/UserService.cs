using Application.Common;
using Application.Services;
using Domain.Common;
using Domain.Entity;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    //public class UserService : IUserService
    //{
    //    private readonly IUserRepository _userRepo;
    //    private readonly IPasswordHasher _hasher;

    //    public UserService(IUserRepository userRepo, IPasswordHasher hasher)
    //    {
    //        _userRepo = userRepo;
    //        _hasher = hasher;
    //    }

    //    public async Task<int> CreateForStudentAsync(Student student, CancellationToken ct = default)
    //    {
    //        var username = CredentialsGenerator.StudentUsername(student.FirstName, student.LastName, student.IndexNumber);

    //        if (await _userRepo.ExistsByUsernameAsync(username, ct))
    //            throw new AppException(AppErrorCode.Conflict, "Generated username already exists.");

    //        var initialPlain = CredentialsGenerator.InitialPasswordPlain(student.JMBG);
    //        var hash = _hasher.Hash(initialPlain);

    //        var user = new User(student.ID, username, hash);
    //        return await _userRepo.CreateAsync(user, ct);
    //    }

    //    public async Task<int> CreateForTeacherAsync(Teacher teacher, CancellationToken ct = default)
    //    {
    //        var username = CredentialsGenerator.TeacherUsername(teacher.FirstName, teacher.LastName, teacher.EmployeeNumber);

    //        if (await _userRepo.ExistsByUsernameAsync(username, ct))
    //            throw new AppException(AppErrorCode.Conflict, "Generated username already exists.");

    //        var initialPlain = CredentialsGenerator.InitialPasswordPlain(teacher.JMBG);
    //        var hash = _hasher.Hash(initialPlain);

    //        var user = new User(teacher.ID, username, hash);
    //        return await _userRepo.CreateAsync(user, ct);
    //    }

    //    public async Task DeactivateByPersonIdAsync(int personId, CancellationToken ct = default)
    //    {
    //        var user = await _userRepo.GetByPersonIdAsync(personId, ct);
    //        if (user == null) return;

    //        user.Deactivate();
    //        await _userRepo.UpdateAsync(user, ct);
    //    }

    //    public async Task ChangePasswordAsync(int userId, string newPasswordPlain, CancellationToken ct = default)
    //    {
    //        var user = await _userRepo.GetByIdAsync(userId, ct)
    //            ?? throw new AppException(AppErrorCode.NotFound, $"User with id {userId} not found.");

    //        user.SetPasswordHash(_hasher.Hash(newPasswordPlain));
    //        user.MarkPasswordChanged();

    //        await _userRepo.UpdateAsync(user, ct);
    //    }
    //}

    //public class UserService : IUserService
    //{
    //    private readonly IUnitOfWork _uow;

    //    public UserService(IUnitOfWork uow)
    //    {
    //        _uow = uow;
    //    }

    //    public async Task<User> CreateForStudentAsync(Student student, CancellationToken ct = default)
    //    {
    //        var username = CredentialsGenerator.StudentUsername(student.FirstName, student.LastName, student.IndexNumber);

    //        if (await _uow.Users.ExistsByUsernameAsync(username, ct))
    //            throw new AppException(AppErrorCode.Conflict, "Generated username already exists.");

    //        var initialPlain = CredentialsGenerator.InitialPasswordPlain(student.JMBG);
    //        var hash = _hasher.Hash(initialPlain);

    //        var user = new User(student.ID, username, hash);

    //        _uow.Users.Add(user);
    //        return user;
    //    }

    //    public async Task<User> CreateForTeacherAsync(Teacher teacher, CancellationToken ct = default)
    //    {
    //        var username = CredentialsGenerator.TeacherUsername(teacher.FirstName, teacher.LastName, teacher.EmployeeNumber);

    //        if (await _uow.Users.ExistsByUsernameAsync(username, ct))
    //            throw new AppException(AppErrorCode.Conflict, "Generated username already exists.");

    //        var initialPlain = CredentialsGenerator.InitialPasswordPlain(teacher.JMBG);
    //        var hash = _hasher.Hash(initialPlain);

    //        var user = new User(teacher.ID, username, hash);

    //        _uow.Users.Add(user);
    //        return user;
    //    }

    //    public async Task DeactivateByPersonIdAsync(int personId, CancellationToken ct = default)
    //    {
    //        var user = await _uow.Users.GetByPersonIdAsync(personId, ct);
    //        if (user is null) return;

    //        user.Deactivate();
    //        _uow.Users.Update(user);
    //        // bez commit ovde
    //    }

    //    public async Task ChangePasswordAsync(int userId, string newPasswordPlain, CancellationToken ct = default)
    //    {
    //        var user = await _uow.Users.GetByIdAsync(userId, ct)
    //            ?? throw new AppException(AppErrorCode.NotFound, $"User with id {userId} not found.");

    //        user.SetPasswordHash(_hasher.Hash(newPasswordPlain));
    //        user.MarkPasswordChanged();

    //        _uow.Users.Update(user);
    //        // bez commit ovde
    //    }


    //}

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
            // commit radi caller (npr. StudentService/TeacherService)
        }

        public async Task ChangePasswordAsync(int userId, string newPasswordPlain, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct)
                ?? throw new AppException(AppErrorCode.NotFound, $"User with id {userId} not found.");

            user.SetPasswordHash(PasswordHasher.Hash(newPasswordPlain)); // static
            user.MarkPasswordChanged();

            _uow.Users.Update(user);
            // commit radi caller
        }
    }
}
