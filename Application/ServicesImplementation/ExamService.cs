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
        public ExamService(IUnitOfWork uow) => _uow = uow;

        public async Task<ExamResponse> UpsertAsync(UpsertExamRequest req, int teacherId, CancellationToken ct = default)
        {
            // 1) teacher assignment + can grade
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, req.SubjectID, ct);
            if (ta is null)
                throw new AppException(AppErrorCode.Forbidden, "Teacher is not assigned to this subject.");

            if (!ta.CanGrade)
                throw new AppException(AppErrorCode.Forbidden, "Teacher can not grade (CanGrade=false).");

            // 2) student must have active registration for this term+subject
            var hasReg = await _uow.Registrations.ExistsActiveAsync(req.StudentID, req.SubjectID, req.TermID, ct);
            if (!hasReg)
                throw new AppException(AppErrorCode.Conflict, "Student does not have an active registration for this term.");

            // 3) upsert exam
            var exam = await _uow.Exams.GetByKeyAsync(req.StudentID, req.SubjectID, req.TermID, ct);

            if (exam is null)
            {
                exam = new Exam
                {
                    StudentID = req.StudentID,
                    SubjectID = req.SubjectID,
                    TermID= req.TermID,
                    Grade = req.Grade,
                    Date = req.Date,
                    Note = req.Note,
                    SignedAt = null
                };

                _uow.Exams.Add(exam);
                await _uow.CommitAsync(ct);
                return Mapper.ExamToResponse(exam);
            }

            if (exam.SignedAt is not null)
                throw new AppException(AppErrorCode.Conflict, "Exam is locked (SignedAt is set).");

            exam.Grade = req.Grade;
            exam.Date = req.Date;
            exam.Note = req.Note;

            await _uow.CommitAsync(ct);
            return Mapper.ExamToResponse(exam);
        }

        public async Task<int> LockAsync(LockExamsRequest req, int teacherId, CancellationToken ct = default)
        {
            // teacher assignment + can grade
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, req.SubjectID, ct);
            if (ta is null)
                throw new AppException(AppErrorCode.Forbidden, "Teacher is not assigned to this subject.");

            if (!ta.CanGrade)
                throw new AppException(AppErrorCode.Forbidden, "Teacher can not grade (CanGrade=false).");

            var now = DateTime.UtcNow;

            // all students who have active registrations for subject in this term
            var studentIds = await _uow.Registrations.ListActiveStudentIdsAsync(req.SubjectID, req.TermID, ct);

            // 3) osiguraj da svaki ima exam record (grade null = nije izasao)
            foreach (var studentId in studentIds)
            {
                var exam = await _uow.Exams.GetByKeyAsync(studentId, req.SubjectID, req.TermID, ct);

                if (exam is null)
                {
                    exam = new Exam
                    {
                        StudentID = studentId,
                        SubjectID = req.SubjectID,
                        TermID = req.TermID,
                        Grade = null,
                        Date = DateOnly.FromDateTime(now),
                        Note = null,
                        SignedAt = null
                    };

                    _uow.Exams.Add(exam);
                }
                else
                {
                    if (exam.SignedAt is not null)
                        continue;
                }
            }

            // 4) zakljucaj sve unsigned za ovaj subject+term
            var unsigned = await _uow.Exams.ListUnsignedBySubjectTermAsync(req.SubjectID, req.TermID, ct);

            foreach (var exam in unsigned)
            {
                exam.SignedAt = now;
                exam.TeacherID = teacherId;

                var r = exam.Registration;

                var enrollment = await _uow.Enrollments.GetAsync(r.StudentID, r.SubjectID, ct);

                if (enrollment is null)
                    continue;

                if (exam.Grade >= 6)
                    enrollment.IsPassed = true;
            }

            await _uow.CommitAsync(ct);
            return unsigned.Count;
        }

        public async Task<List<ExamResponse>> ListBySubjectTermAsync(int subjectId, int termId, int teacherId, CancellationToken ct = default)
        {
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct);
            if (ta is null)
                throw new AppException(AppErrorCode.Forbidden, "Teacher is not assigned to this subject.");

            var list = await _uow.Exams.ListBySubjectTermAsync(subjectId, termId, ct);
            return list.Select(Mapper.ExamToResponse).ToList();
        }
        
    }
}

