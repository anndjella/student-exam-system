using Application.Common.Abstractions;
using Application.Common.Errors;
using Application.Common.Mapping;
using Application.Common.Pagination;
using Application.DTO.Exams;
using Application.DTO.Students;
using Application.Services.Interfaces;
using Application.Services.Implementations;
using Domain.Entity;
using Domain.Interfaces;
using Domain.Common;
using Domain.Enums;
using Application.Auth;
using Application.DTO.Common;

namespace Application.Services.Implementations;

public sealed class StudentService : IStudentService
{
    private readonly IUnitOfWork _uow;
    public StudentService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task<StudentResponse> CreateAsync(CreateStudentRequest req, CancellationToken ct = default)
    {
        if (!JmbgParser.TryGetDateOfBirth(req.JMBG, out var dob, out var dobError))
            throw new AppException(AppErrorCode.Validation, dobError);

        if (await _uow.People.ExistsByJmbgAsync(req.JMBG, ct))
            throw new AppException(AppErrorCode.Conflict, "Person with this JMBG already exists.");

        if (await _uow.Students.ExistsByIndexAsync(req.IndexNumber, ct))
            throw new AppException(AppErrorCode.Conflict, "Index already exists.");

        var student = new Student
        {
            JMBG = req.JMBG,
            FirstName = req.FirstName,
            LastName = req.LastName,
            DateOfBirth = dob,
            IndexNumber = req.IndexNumber
        };

        _uow.Students.Add(student);

        var username = CredentialsGenerator.StudentUsername(student.FirstName, student.LastName, student.IndexNumber);

        if (await _uow.Users.ExistsByUsernameAsync(username, ct))
            throw new AppException(AppErrorCode.Conflict, "Generated username already exists.");

        var user = InitialUserFactory.Create(UserRole.Student, username, student.JMBG);
        student.User = user;

        _uow.Users.Add(user);

        await _uow.CommitAsync(ct);

        var created = await _uow.Students.GetByIdAsync(student.ID, ct)
            ?? throw new AppException(AppErrorCode.Unexpected, "Unexpected error in creating.");

        return StudentMapper.ToResponse(created);
    }

    public async Task<StudentResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var s = await _uow.Students.GetByIdAsync(id, ct);
        if (s is null)
            throw new AppException(AppErrorCode.NotFound, $"Student with id {id} not found.");

        return StudentMapper.ToResponse(s);
    }
    public async Task<StudentResponse?> GetByIndexAsync(string index, CancellationToken ct = default)
    {
        var s=await _uow.Students.GetByIndexAsync(index, ct);
        if (s is null)
            throw new AppException(AppErrorCode.NotFound, $"Student with index {index} not found.");
        var stats = await _uow.StudentStats.GetByStudentIdAsync(s.ID, ct);
        return StudentMapper.ToResponseWithStats(s,stats);
    }

    public async Task UpdateAsync(int id, UpdateStudentRequest req, CancellationToken ct = default)
    {
        var s = await _uow.Students.GetByIdAsync(id, ct)
            ?? throw new AppException(AppErrorCode.NotFound, $"Student with id {id} not found.");

        if (req.FirstName is not null) s.FirstName = req.FirstName;
        if (req.LastName is not null) s.LastName = req.LastName;

        if (req.IndexNumber is not null)
        {
            if (s.IndexNumber != req.IndexNumber && await _uow.Students.ExistsByIndexAsync(req.IndexNumber, ct))
                throw new AppException(AppErrorCode.Conflict, "Index already exists.");

            s.IndexNumber = req.IndexNumber;
        }

        //_uow.Students.Update(s);
        await _uow.CommitAsync(ct);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
           var s = await _uow.Students.GetByIdWithUserAsync(id, ct)
            ?? throw new AppException(AppErrorCode.NotFound, $"Student with id {id} not found.");

        s.MarkDeleted();
        s.User?.Deactivate();

        //_uow.Students.Update(s);
        await _uow.CommitAsync(ct);
    }

    public async Task<PagedResponse<StudentResponse>> ListAsync(int skip, int take,string? query,bool onlyDeleted,CancellationToken ct)
    {
        (skip, take) = Paging.Normalize(skip, take);

        var total = await _uow.Students.CountAsync(query, onlyDeleted, ct);
        var items = await _uow.Students.ListPagedAsync(skip, take, query, onlyDeleted, ct);

        var ids = items.Select(s => s.ID).ToList();

        var statsList = await _uow.StudentStats.ListByStudentIdsAsync(ids, ct);

        var statsByStudentId = statsList.ToDictionary(x => x.StudentID);

        var respItems = items.Select(s =>
        {
            statsByStudentId.TryGetValue(s.ID, out var st);
            return StudentMapper.ToResponseWithStats(s, st);
        }).ToList();

        return new PagedResponse<StudentResponse>
        {
            Items = respItems,
            Total = total
        };
    }
}
