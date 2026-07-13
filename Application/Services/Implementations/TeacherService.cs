using Application.Common.Abstractions;
using Application.Common.Errors;
using Application.Common.Mapping;
using Application.Common.Pagination;
using Application.Auth;
using Application.DTO.Common;
using Application.DTO.Exams;
using Application.DTO.Students;
using Application.DTO.Teachers;
using Application.Services.Interfaces;
using Domain.Common;
using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementations
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
            if (!JmbgParser.TryGetDateOfBirth(req.JMBG, out var dob, out var dobError))
                throw new AppException(AppErrorCode.Validation, dobError);

            if (await _uow.People.ExistsByJmbgAsync(req.JMBG, ct))
                throw new AppException(AppErrorCode.Conflict,"Person with this JMBG already exists.");
            if (await _uow.Teachers.ExistsByEmployeeNumAsync(req.EmployeeNumber, ct))
                throw new AppException(AppErrorCode.Conflict, "Employee number already exists.");

            Teacher teacher = new Teacher
            {
                JMBG = req.JMBG,
                FirstName = req.FirstName,
                LastName = req.LastName,
                DateOfBirth = dob,
                EmployeeNumber = req.EmployeeNumber,
                Title=req.Title
            };

            _uow.Teachers.Add(teacher);

            var username = CredentialsGenerator.TeacherUsername(teacher.FirstName, teacher.LastName, teacher.EmployeeNumber);

            if (await _uow.Users.ExistsByUsernameAsync(username, ct))
                throw new AppException(AppErrorCode.Conflict, "Generated username already exists.");

            var user = InitialUserFactory.Create(UserRole.Teacher, username, teacher.JMBG);
            teacher.User = user;

            _uow.Users.Add(user);

            await _uow.CommitAsync(ct);

            var created = await _uow.Teachers.GetByIdAsync(teacher.ID, ct)
                ?? throw new AppException(AppErrorCode.Unexpected, "Unexpected error in creating.");

            return TeacherMapper.ToResponse(created);
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
                : TeacherMapper.ToResponse(t);
        }
        public async Task<TeacherResponse?> GetByNumAsync(string employeeNum, CancellationToken ct = default)
        {
            var t = await _uow.Teachers.GetByEmployeeNumAsync(employeeNum, ct);
            return t is null ?
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {employeeNum} not found.")
                : TeacherMapper.ToResponse(t);
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
                if (t.EmployeeNumber != req.EmployeeNumber && await _uow.Teachers.ExistsByEmployeeNumAsync(req.EmployeeNumber, ct))
                    throw new AppException(AppErrorCode.Conflict, "Employee number already exists.");

                t.EmployeeNumber = req.EmployeeNumber;
            }

            //_uow.Teachers.Update(t);
            await _uow.CommitAsync(ct);
        }

        public async Task<PagedResponse<TeacherResponse>> ListAsync(int skip,int take,string? query, bool onlyDeleted, CancellationToken ct)
        {
            (skip, take) = Paging.Normalize(skip, take);

            var total = await _uow.Teachers.CountAsync(query, onlyDeleted, ct);
            var items = await _uow.Teachers.ListPagedAsync(skip, take, query, onlyDeleted, ct);

            var respItems = items.Select(TeacherMapper.ToResponse).ToList();

            return new PagedResponse<TeacherResponse>
            {
                Items = respItems,
                Total = total
            };
        }
    }
}
