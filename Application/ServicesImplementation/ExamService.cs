using Application.Common;
using Application.DTO.Exams;
using Application.Services;
using Domain.Entity;
using Domain.Interfaces;
using System.Linq.Expressions;

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
        public async Task CreateAsync(CreateExamRequest req, CancellationToken ct = default)
        {
            if (await _students.GetByIdAsync(req.StudentID, ct) is null)
                throw new AppException(AppErrorCode.BadRequest, $"Student with id {req.StudentID} not found.");

            if (await _subjects.GetByIdAsync(req.SubjectID, ct) is null)
                throw new AppException(AppErrorCode.BadRequest, $"Subject with id {req.SubjectID} not found.");

            if (await _teachers.GetByIdAsync(req.ExaminerID, ct) is null)
                throw new AppException(AppErrorCode.BadRequest, $"Examiner with id {req.ExaminerID} not found.");

            if (req.SupervisorID is not null && await _teachers.GetByIdAsync(req.SupervisorID.Value, ct) is null)
                throw new AppException(AppErrorCode.BadRequest, $"Supervisor with id {req.SupervisorID} not found.");

            if (await _repo.ExistsOnDateAsync(req.StudentID, req.SubjectID, req.Date, ct))
                throw new AppException(AppErrorCode.Conflict, "A student cannot take the same subject exam more than once on the same day.");

            if (req.Grade >= 6 && await _repo.HasPassedAsync(req.StudentID, req.SubjectID, ct))
                throw new AppException(AppErrorCode.Conflict, "Student already passed this subject.");

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

            await _repo.CreateAsync(exam, ct);
        }

        public async Task DeleteAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default)
        {
            var s = await _repo.GetByKeyAsync(studentId,subjectId,date, ct);
            if (s is null) 
                throw new AppException(AppErrorCode.NotFound, $"Exam not found.");
            await _repo.DeleteAsync(studentId,subjectId,date, ct);
        }

        public async Task<ExamResponse?> GetAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default)
        {
            var e = await _repo.GetByKeyWithDetailsAsync(studentId, subjectId, date, ct);
            return e is null ?
                throw new AppException(AppErrorCode.NotFound, $"Exam not found.")
                : Mapper.ExamToResponse(e);
        }

        public async Task<IReadOnlyList<ExamResponse>> ListAsync(CancellationToken ct = default)
        {
            var list = await _repo.ListWithDetailsAsync(ct);
            return list.Select(Mapper.ExamToResponse).ToList();
        }

        public async Task UpdateAsync(int studentId, int subjectId, DateOnly date, UpdateExamRequest req, CancellationToken ct = default)
        {
            var e = await _repo.GetByKeyAsync(studentId, subjectId, date, ct) ??
                throw new AppException(AppErrorCode.NotFound, $"Exam not found.");

            if (req.Grade is byte newGrade && newGrade != e.Grade)
            {
                if (string.IsNullOrWhiteSpace(req.Note))
                    throw new AppException(AppErrorCode.BadRequest,"Note is required when changing the grade.");
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
    }
}
