using Application.DTO.Students;
using Application.Services;
using Domain.Entity;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services;

public sealed class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;
    public StudentService(IStudentRepository repo) => _repo = repo;

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest req, CancellationToken ct = default)
    {
        if (await _repo.ExistsByJmbgAsync(req.JMBG, ct))
            throw new InvalidOperationException("Student with this JMBG already exists.");

        if (await _repo.ExistsByIndexAsync(req.IndexNumber, ct))
            throw new InvalidOperationException("Index already exists.");

        var student = new Student
        {
            JMBG = req.JMBG,
            FirstName = req.FirstName,
            LastName = req.LastName,
            DateOfBirth = req.DateOfBirth,
            IndexNumber = req.IndexNumber
        };

        var id = await _repo.CreateAsync(student, ct);
        var created = await _repo.GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Unexpected error in creating.");

        return Map(created);
    }

    public async Task<StudentResponse?> GetAsync(int id, CancellationToken ct = default)
    {
        var s = await _repo.GetByIdAsync(id, ct);
        return s is null ? null : Map(s);
    }

    public async Task<IReadOnlyList<StudentResponse>> ListAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task UpdateAsync(int id, UpdateStudentRequest req, CancellationToken ct = default)
    {
        var s = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Student does not exist.");

        if (req.FirstName is not null) s.FirstName = req.FirstName;
        if (req.LastName is not null) s.LastName = req.LastName;
        if (req.IndexNumber is not null)
        {
            if (s.IndexNumber != req.IndexNumber && await _repo.ExistsByIndexAsync(req.IndexNumber, ct))
                throw new InvalidOperationException("Index already exists.");
            s.IndexNumber = req.IndexNumber;
        }

        await _repo.UpdateAsync(s, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var s = await _repo.GetByIdAsync(id, ct);
        if (s is null) return;
        await _repo.DeleteAsync(s, ct);
    }

    private static StudentResponse Map(Student s) => new()
    {
        Id = s.ID,
        FirstName = s.FirstName,
        LastName = s.LastName,
        Age = s.Age,
        Gpa = s.GPA,
        IndexNumber = s.IndexNumber
    };
}
