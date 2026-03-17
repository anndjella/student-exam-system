using Application.Common;
using Application.DTO.Exams;
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

        public async Task<TeacherExamItemResponse> CreateAsync(int subjectId,int termId,int studentId,CreateExamRequest req, int teacherId, CancellationToken ct = default)
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

            var term = await _uow.Terms.GetByIdAsync(termId, ct);
            if (term is null)
                throw new AppException(AppErrorCode.NotFound, "Term not found.");

            await EnsurePreviousTermFinalizedAsync(subjectId, termId, ct);

            if (!term.IsInTermWindow(req.Date))
                throw new AppException(
                    AppErrorCode.Validation,
                    $"Exam date must be between {term.StartDate:yyyy-MM-dd} and {term.EndDate:yyyy-MM-dd}."
                );

            if (term.IsInRegistrationWindow(req.Date))
                throw new AppException(
                    AppErrorCode.Conflict,
                    "Exam cannot be created while the registration period is still open."
                );

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

            return Mapper.ExamToTeacherResponse(exam);
        }

        public async Task<int> LockAsync(LockExamsRequest req, int teacherId, CancellationToken ct = default)
        {
            await EnsureTeacherCanGradeAsync(teacherId, req.SubjectID, ct);

            var term = await _uow.Terms.GetByIdAsync(req.TermID, ct);
            if (term is null)
                throw new AppException(AppErrorCode.NotFound, "Term not found.");

            await EnsurePreviousTermFinalizedAsync(req.SubjectID, req.TermID, ct);

            if (term.IsInRegistrationWindow(_clock.Today))
                throw new AppException(
                    AppErrorCode.Conflict,
                    "Cannot lock exams while the registration period is still open."
                );

            var activeRegs = await _uow.Registrations
                .ListActiveBySubjectAndTermWithExamAsync(req.SubjectID, req.TermID, ct);

            if (activeRegs.Count == 0) return 0;

            await EnsureAllActiveRegistrationsHaveExamAsync(activeRegs, req.SubjectID, req.TermID, ct);

            var unsignedExams = await _uow.Exams
                .ListUnsignedBySubjectTermWithRegistrationAsync(req.SubjectID, req.TermID, ct);

            if (unsignedExams.Count == 0) return 0;

            var enrollmentByStudent = await LoadEnrollmentsByStudentAsync(unsignedExams, req.SubjectID, ct);

            ApplyLock(unsignedExams, enrollmentByStudent, teacherId, _clock.UtcNow);

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
        public async Task<TeacherExamsResponse> ListBySubjectTermAsync(int subjectId, int termId, int teacherId, CancellationToken ct = default)
        {
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct);
            if (ta is null)
                throw new AppException(AppErrorCode.Forbidden, "Teacher cannot view this subject.");

            var exams = await _uow.Exams.ListAllBySubjectTermForTeacherAsync(subjectId, termId,teacherId, ct);

            var mine = new List<TeacherExamItemResponse>();
            var others = new List<TeacherExamItemResponse>();

            foreach (var exam in exams)
            {
                var dto = Mapper.ExamToTeacherResponse(exam);

                if (exam.TeacherID == teacherId)
                    mine.Add(dto);
                else
                    others.Add(dto);
            }

            return new TeacherExamsResponse
            {
                Mine = mine,
                Others = others
            };
        }

        public async Task<TeacherExamItemResponse> UpdateAsync(int examId,UpdateExamRequest req, int teacherId, CancellationToken ct = default)
        {
            var exam= await _uow.Exams.GetByIdAsync(examId, ct);
            if(exam is null)
                throw new AppException(AppErrorCode.NotFound, "Exam not found.");

            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, exam.SubjectID, ct);
            if (ta is null || !ta.CanGrade)
                throw new AppException(AppErrorCode.Forbidden, "Teacher cannot grade this subject.");

            await EnsurePreviousTermFinalizedAsync(exam.SubjectID, exam.TermID, ct);

            if (exam.SignedAt is not null)
                throw new AppException(AppErrorCode.Conflict, "Exam is locked and cannot be changed.");

            exam.Grade = req.Grade;
            exam.Note = req.Note;

            await _uow.CommitAsync(ct);
            return Mapper.ExamToTeacherResponse(exam);

        }

        public async Task<StudentExamsResponse> ListMySignedAsync(int studentId, CancellationToken ct = default)
        {
            var student = await _uow.Students.GetByIdAsync(studentId, ct);
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
        public async Task<StudServiceExamsResponse> ListPagedAsync(
            int subjectId,
            int termId,
            int skip,
            int take,
            string? query,
            CancellationToken ct = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            query = query?.Trim();

            var existsSubject = await _uow.Subjects.ExistsById(subjectId, ct);
            if (!existsSubject)
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {subjectId} not found.");

            var existsTerm = await _uow.Terms.ExistsById(termId, ct);
            if (!existsTerm)
                throw new AppException(AppErrorCode.NotFound, $"Term with id {termId} not found.");

            var total = await _uow.Exams.CountPagedAsync(subjectId, termId, query, ct);
            var unsignedCount = await _uow.Exams.CountUnsignedBySubjectTermAsync(subjectId, termId, ct);
            var exams = await _uow.Exams.ListPagedAsync(subjectId, termId, skip, take, query, ct);

            return new StudServiceExamsResponse
            {
                UnsignedCount = unsignedCount,
                Total = total,
                Exams = exams.Select(Mapper.StudServiceExamToResponse).ToList()
            };
        }
        private async Task EnsurePreviousTermFinalizedAsync(
            int subjectId,
            int currentTermId,
            CancellationToken ct)
        {
            var previousTerm = await _uow.Terms.GetPreviousTermAsync(currentTermId, ct);
            if (previousTerm is null)
                return;

            var prevActiveRegs = await _uow.Registrations
                .ListActiveBySubjectAndTermWithExamAsync(subjectId, previousTerm.ID, ct);

            var hasMissingExam = prevActiveRegs.Any(r => r.Exam is null);
            if (hasMissingExam)
            {
                throw new AppException(
                    AppErrorCode.Conflict,
                    $"Previous term '{previousTerm.Name}' is not finalized. " +
                    "Some active registrations do not have exams yet."
                );
            }

            var unsignedPrevExams = await _uow.Exams
                .ListUnsignedBySubjectTermWithRegistrationAsync(subjectId, previousTerm.ID, ct);

            if (unsignedPrevExams.Count > 0)
            {
                throw new AppException(
                    AppErrorCode.Conflict,
                    $"Previous term '{previousTerm.Name}' is not finalized. " +
                    "Some exams are still not locked."
                );
            }
        }
    }
}

