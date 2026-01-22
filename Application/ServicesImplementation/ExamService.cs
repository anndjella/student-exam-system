using Application.Common;
using Application.DTO.Exams;
using Application.DTO.Me.Student;
using Application.DTO.Me.StudService;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using System.Linq.Expressions;

namespace Application.ServicesImplementation
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        public ExamService(IUnitOfWork uow, IClock clock) {_uow = uow; _clock = clock; }

        public async Task<ExamResponse> CreateAsync(int subjectId,int termId,int studentId,CreateExamRequest req, int teacherId, CancellationToken ct = default)
        {
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct);
            if (ta is null || !ta.CanGrade)
                throw new AppException(AppErrorCode.Forbidden, "Teacher cannot grade this subject.");

            var hasReg = await _uow.Registrations.ExistsActiveAsync(studentId, subjectId, termId, ct);
            if (!hasReg)
                throw new AppException(AppErrorCode.Conflict, "Student does not have an active registration for this term.");

            var existing = await _uow.Exams.GetByKeyAsync(studentId, subjectId, termId, ct);
            if (existing is not null)
                throw new AppException(AppErrorCode.Conflict, "Exam already exists.");

            var exam = new Exam
            {
                StudentID = studentId,
                SubjectID = subjectId,
                TermID = termId,
                Grade = req.Grade,
                Date = req.Date,
                Note = req.Note,
                SignedAt = null,
                TeacherID = teacherId
            };

            _uow.Exams.Add(exam);
            await _uow.CommitAsync(ct);

            return Mapper.ExamToResponse(exam);
        }

        public async Task<int> LockAsync(LockExamsRequest req, int teacherId, CancellationToken ct = default)
        {
            await EnsureTeacherCanGradeAsync(teacherId, req.SubjectID, ct);

            var now = _clock.UtcNow;

            var activeRegs = await _uow.Registrations
                .ListActiveBySubjectAndTermAsync(req.SubjectID, req.TermID, ct);

            if (activeRegs.Count == 0) return 0;

            await EnsureAllActiveRegistrationsHaveExamAsync(activeRegs, req.SubjectID, req.TermID, ct);

            var unsignedExams = await _uow.Exams
                .ListUnsignedBySubjectTermWithRegistrationAsync(req.SubjectID, req.TermID, ct);

            if (unsignedExams.Count == 0) return 0;

            var enrollmentByStudent = await LoadEnrollmentsByStudentAsync(unsignedExams, req.SubjectID, ct);

            ApplyLock(unsignedExams, enrollmentByStudent, teacherId, now);

            await _uow.CommitAsync(ct);
            return unsignedExams.Count;
        }

        private async Task EnsureTeacherCanGradeAsync(int teacherId, int subjectId, CancellationToken ct)
        {
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct);
            if (ta is null)
                throw new AppException(AppErrorCode.Forbidden, "Teacher is not assigned to this subject.");
            if (!ta.CanGrade)
                throw new AppException(AppErrorCode.Forbidden, "Teacher can not grade (CanGrade=false).");
        }

        private async Task EnsureAllActiveRegistrationsHaveExamAsync(
            List<Registration> activeRegs,
            int subjectId,
            int termId,
            CancellationToken ct)
        {
            var allExams = await _uow.Exams.ListBySubjectTermAsync(subjectId, termId, ct);

            var examByStudent = allExams.ToDictionary(e => e.StudentID);

            var missingExam = new List<int>();

            foreach (var reg in activeRegs)
            {
                if (!examByStudent.ContainsKey(reg.StudentID))
                    missingExam.Add(reg.StudentID);
            }

            if (missingExam.Count > 0)
                throw new AppException(
                    AppErrorCode.Conflict,
                    "Cannot lock exams because some active registrations do not have exams.");
        }


        private async Task<Dictionary<int, Enrollment>> LoadEnrollmentsByStudentAsync(
            List<Exam> unsignedExams,
            int subjectId,
            CancellationToken ct)
        {
            var studentIds = unsignedExams.Select(e => e.StudentID).Distinct().ToList();
            var enrollments = await _uow.Enrollments.ListByStudentsAndSubjectAsync(studentIds, subjectId, ct);
            return enrollments.ToDictionary(e => e.StudentID);
        }

        private static void ApplyLock(
            List<Exam> unsignedExams,
            Dictionary<int, Enrollment> enrollmentByStudent,
            int teacherId,
            DateTime now)
        {
            foreach (var exam in unsignedExams)
            {
                exam.SignedAt = now;
                exam.TeacherID = teacherId;

                if (exam.Registration is not null)
                    exam.Registration.IsActive = false;

                if (exam.Grade is >= 6)
                {
                    if (enrollmentByStudent.TryGetValue(exam.StudentID, out var enr))
                    {
                        enr.IsPassed = true;
                        enr.PassedAt = now;
                    }
                }
            }
        }
        public async Task<List<ExamResponse>> ListBySubjectTermAsync(int subjectId, int termId, int teacherId, CancellationToken ct = default)
        {
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct);
            if (ta is null)
                throw new AppException(AppErrorCode.Forbidden, "Teacher is not assigned to this subject.");

            var list = await _uow.Exams.ListBySubjectTermAsync(subjectId, termId, ct);
            return list.Select(Mapper.ExamToResponse).ToList();
        }

        public async Task<ExamResponse> UpdateAsync(int subjectId, int termId, int studentId,UpdateExamRequest req, int teacherId, CancellationToken ct = default)
        {
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct);
            if (ta is null || !ta.CanGrade)
                throw new AppException(AppErrorCode.Forbidden, "Teacher cannot grade this subject.");

            var exam = await _uow.Exams.GetByKeyAsync(studentId, subjectId, termId, ct);
            if (exam is null)
                throw new AppException(AppErrorCode.NotFound, "Exam not found.");

            if (exam.SignedAt is not null)
                throw new AppException(AppErrorCode.Conflict, "Exam is locked and cannot be changed.");

            exam.Grade = req.Grade;
            exam.Note = req.Note;

            await _uow.CommitAsync(ct);
            return Mapper.ExamToResponse(exam);

        }

        public async Task<StudentExamsResponse> ListMySignedAsync(int personId, CancellationToken ct = default)
        {
            var student = await _uow.Students.GetByIdAsync(personId, ct);
            if (student is null)
                throw new AppException(AppErrorCode.NotFound, "Student not found.");

            var exams = await _uow.Exams.ListSignedByStudentIdAsync(student.ID, ct);

            var resp = new StudentExamsResponse();

            foreach (var e in exams)
            {
                var dto = Mapper.StudentExamToResponse(e);

                if (dto.Grade.HasValue && dto.Grade.Value > 5)
                    resp.Passed.Add(dto);
                else
                    resp.NotPassed.Add(dto);
            }

            return resp;
        }

        public async Task<StudentServiceExamsResponse> ListAllBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default)
        {
            var exams = await _uow.Exams.ListAllBySubjectTermAsync(termId, subjectId, ct);

            return new StudentServiceExamsResponse
            {
                UnsignedCount = exams.Count(e => e.SignedAt == null),
                Exams = exams.Select(Mapper.StudServiceExamToResponse).ToList()
            };
        }
    }
}

