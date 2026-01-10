using Application.Auth;
using Application.Common;
using Application.DTO.Exams;
using Application.DTO.Students;
using Application.DTO.Teachers;
using Application.Services;
using Domain.Common;
using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _uow;
        public TeacherService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest req, CancellationToken ct = default)
        {
            if (await _uow.People.ExistsByJmbgAsync(req.JMBG, ct))
                throw new AppException(AppErrorCode.Conflict,"Person with this JMBG already exists.");
            if (await _uow.Teachers.ExistsByEmployeeNumAsync(req.EmployeeNumber, ct))
                throw new AppException(AppErrorCode.Conflict, "Employee number already exists.");

            Teacher teacher = new Teacher
            {
                JMBG = req.JMBG,
                FirstName = req.FirstName,
                LastName = req.LastName,
                DateOfBirth = JmbgParser.GetDateOfBirth(req.JMBG),
                EmployeeNumber = req.EmployeeNumber,
                Title=req.Title
            };

            _uow.Teachers.Add(teacher);

            var username = CredentialsGenerator.StudentUsername(teacher.FirstName, teacher.LastName, teacher.EmployeeNumber);

            if (await _uow.Users.ExistsByUsernameAsync(username, ct))
                throw new AppException(AppErrorCode.Conflict, "Generated username already exists.");

            var initialPlain = CredentialsGenerator.InitialPasswordPlain(teacher.JMBG);
            var user = new User(UserRole.Teacher, username, passwordHash: "TEMP");

            var hash = PasswordService.Hash(user, initialPlain);
            user.SetPasswordHash(hash);

            teacher.User = user;

            _uow.Users.Add(user);

            await _uow.CommitAsync(ct);

            var created = await _uow.Teachers.GetByIdAsync(teacher.ID, ct)
                ?? throw new AppException(AppErrorCode.Unexpected, "Unexpected error in creating.");

            return Mapper.TeacherToResponse(created);
        }

        public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var t = await _uow.Teachers.GetByIdWithUserAsync(id, ct)
            ?? throw new AppException(AppErrorCode.NotFound, $"Teacher with id {id} not found.");

            t.MarkDeleted();
            t.User?.Deactivate();

            //_uow.Teachers.Update(t);
            await _uow.CommitAsync(ct);
        }

        public async Task<TeacherResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var t = await _uow.Teachers.GetByIdAsync(id, ct);
            return t is null ? 
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {id} not found.")
                : Mapper.TeacherToResponse(t);
        }
        public async Task<TeacherResponse?> GetByNumAsync(string employeeNum, CancellationToken ct = default)
        {
            var t = await _uow.Teachers.GetByEmployeeNumAsync(employeeNum, ct);
            return t is null ?
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {employeeNum} not found.")
                : Mapper.TeacherToResponse(t);
        }
        public async Task UpdateAsync(int id, UpdateTeacherRequest req, CancellationToken ct = default)
        {
            var t = await _uow.Teachers.GetByIdAsync(id, ct) ??
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {id} not found.");

            if (req.FirstName is not null) t.FirstName = req.FirstName;
            if (req.LastName is not null) t.LastName = req.LastName;
            if (req.Title is not null) t.Title = req.Title.Value;

            if (req.EmployeeNumber is not null)
            {
                if (t.EmployeeNumber != req.EmployeeNumber && await _uow.Students.ExistsByIndexAsync(req.EmployeeNumber, ct))
                    throw new AppException(AppErrorCode.Conflict, "Employee number already exists.");

                t.EmployeeNumber = req.EmployeeNumber;
            }

            //_uow.Teachers.Update(t);
            await _uow.CommitAsync(ct);
        }
    }
}
