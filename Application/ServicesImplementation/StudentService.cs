using Application.Common;
using Application.DTO.Exams;
using Application.DTO.Students;
using Application.Services;
using Application.ServicesImplementation;
using Domain.Entity;
using Domain.Interfaces;
using Domain.Common;
using Domain.Enums;
using Application.Auth;
using Application.DTO.Common;

namespace Application.Services;

public sealed class StudentService : IStudentService
{
    private readonly IUnitOfWork _uow;
    public StudentService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task<StudentResponse> CreateAsync(CreateStudentRequest req, CancellationToken ct = default)
    {
        if (await _uow.People.ExistsByJmbgAsync(req.JMBG, ct))
            throw new AppException(AppErrorCode.Conflict, "Person with this JMBG already exists.");

        if (await _uow.Students.ExistsByIndexAsync(req.IndexNumber, ct))
            throw new AppException(AppErrorCode.Conflict, "Index already exists.");

        var student = new Student
        {
            JMBG = req.JMBG,
            FirstName = req.FirstName,
            LastName = req.LastName,
            DateOfBirth = JmbgParser.GetDateOfBirth(req.JMBG),
            IndexNumber = req.IndexNumber
        };

        _uow.Students.Add(student);

        var username = CredentialsGenerator.StudentUsername(student.FirstName, student.LastName, student.IndexNumber);

        if (await _uow.Users.ExistsByUsernameAsync(username, ct))
            throw new AppException(AppErrorCode.Conflict, "Generated username already exists.");

        var initialPlain = CredentialsGenerator.InitialPasswordPlain(student.JMBG);
        var user = new User(UserRole.Student, username, passwordHash: "TEMP");

        var hash = PasswordService.Hash(user, initialPlain);
        user.SetPasswordHash(hash);

        student.User = user;

        _uow.Users.Add(user);

        await _uow.CommitAsync(ct);

        var created = await _uow.Students.GetByIdAsync(student.ID, ct)
            ?? throw new AppException(AppErrorCode.Unexpected, "Unexpected error in creating.");

        return Mapper.StudentToResponse(created);
    }

    public async Task<StudentResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var s = await _uow.Students.GetByIdAsync(id, ct);
        if (s is null)
            throw new AppException(AppErrorCode.NotFound, $"Student with id {id} not found.");

        return Mapper.StudentToResponse(s);
    }
    public async Task<StudentResponse?> GetByIndexAsync(string index, CancellationToken ct = default)
    {
        var s=await _uow.Students.GetByIndexAsync(index, ct);
        if (s is null)
            throw new AppException(AppErrorCode.NotFound, $"Student with index {index} not found.");
        var stats = await _uow.StudentStats.GetByStudentIdAsync(s.ID, ct);
        return Mapper.StudentToResponseWithStats(s,stats);
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

    public async Task<PagedResponse<StudentResponse>> ListAsync(int skip, int take,string? query,CancellationToken ct)
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 20;
        if (take > 100) take = 100;

        var total = await _uow.Students.CountAsync(query, ct);
        var items = await _uow.Students.ListPagedAsync(skip, take, query, ct);


        var respItems = items.Select(Mapper.StudentToResponse).ToList();

        return new PagedResponse<StudentResponse>
        {
            Items = respItems,
            Total = total
        };
    }
}