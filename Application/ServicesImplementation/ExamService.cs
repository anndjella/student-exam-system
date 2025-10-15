using Application.DTO.Exams;
using Application.Services;
using Domain.Entity;
using Domain.Interfaces;

namespace Application.ServicesImplementation
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _repo;
        private readonly IStudentRepository _students;
        private readonly ITeacherRepository _teachers;
        private readonly ISubjectRepository _subjects;
        public ExamService(
        IExamRepository repo,
        IStudentRepository students,
        ITeacherRepository teachers,
        ISubjectRepository subjects)
        {
            _repo = repo;
            _students = students;
            _teachers = teachers;
            _subjects = subjects;
        }
        public async Task<ExamResponse> CreateAsync(CreateExamRequest req, CancellationToken ct = default)
        {
            if (await _students.GetByIdAsync(req.StudentID, ct) is null)
                throw new InvalidOperationException("Student does not exist.");

            if (await _subjects.GetByIdAsync(req.SubjectID, ct) is null)
                throw new InvalidOperationException("Subject does not exist.");

            if (await _teachers.GetByIdAsync(req.ExaminerID, ct) is null)
                throw new InvalidOperationException("Examiner does not exist.");

            if (req.SupervisorID is not null && await _teachers.GetByIdAsync(req.SupervisorID.Value, ct) is null)
                throw new InvalidOperationException("Supervisor does not exist.");

            if (await _repo.ExistsOnDateAsync(req.StudentID, req.SubjectID, req.Date, ct))
                throw new InvalidOperationException("A student cannot take the same subject exam more than once on the same day.");

            if (req.Grade >= 6 && await _repo.HasPassedAsync(req.StudentID, req.SubjectID, ct))
                throw new InvalidOperationException("Student already passed this subject.");

            Exam exam = new Exam
            {
                StudentID = req.StudentID,
                SubjectID = req.SubjectID,
                ExaminerID = req.ExaminerID,
                SupervisorID = req.SupervisorID,
                Date = req.Date,
                Note = req.Note,
                Grade = req.Grade
            };

            var id = await _repo.CreateAsync(exam, ct);
            var created = await _repo.GetByIdWithDetailsAsync(id, ct)
                          ?? throw new InvalidOperationException("Unexpected error in creating.");

            return Map(created);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct);
            if (s is null)
                throw new InvalidOperationException($"Exam with id {id} does not exist.");
            await _repo.DeleteAsync(s, ct);
        }

        public async Task<ExamResponse?> GetAsync(int id, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdWithDetailsAsync(id, ct);
            return e is null
                ? throw new InvalidOperationException($"Exam with id {id} does not exist.")
                : Map(e);
        }

        public async Task<IReadOnlyList<ExamResponse>> ListAsync(CancellationToken ct = default)
        {
            var list = await _repo.ListWithDetailsAsync(ct);
            return list.Select(Map).ToList();
        }

        public async Task UpdateAsync(int id, UpdateExamRequest req, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Exam does not exist.");

            if (req.Grade is byte newGrade && newGrade != e.Grade)
            {
                if (string.IsNullOrWhiteSpace(req.Note))
                    throw new InvalidOperationException("Note is required when changing the grade.");
                e.Grade = newGrade;
                e.Note = req.Note;
            }
            else
            {
                if (req.Grade.HasValue)
                    e.Grade = req.Grade.Value;

                if (req.Note is not null)
                    e.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
            }

            await _repo.UpdateAsync(e, ct);
        }
        private static ExamResponse Map(Exam e) => new()
        {
            ID = e.ID,
            StudentID = e.StudentID,
            SubjectID = e.SubjectID,
            ExaminerID = e.ExaminerID,
            SupervisorID = e.SupervisorID,
            Grade = e.Grade,
            Date = e.Date,
            Note = e.Note,
            StudentIndex = e.Student?.IndexNumber ?? "",
            SubjectName = e.Subject?.Name ?? ""
        };
    }
}
