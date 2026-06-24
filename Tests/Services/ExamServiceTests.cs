using Application.Common;
using Application.DTO.Exams;
using Application.ServicesImplementation;
using Domain.Entity;
using FluentAssertions;
using Moq;
using Tests.TestDoubles;

namespace Tests.Services;

public sealed class ExamServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsExam_WhenTeacherCanGradeAndRegistrationIsActive()
    {
        var uow = new UnitOfWorkMock();
        var clock = new TestClock();
        var service = new ExamService(uow.Object, clock);
        var request = new CreateExamRequest
        {
            Grade = 9,
            Date = new DateOnly(2026, 2, 12),
            Note = "Kolokvijum i usmeni"
        };
        Exam? added = null;

        SetupTeacherCanGrade(uow, teacherId: 4, subjectId: 10);
        uow.Registrations
            .Setup(x => x.ExistsActiveAsync(6, 10, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        uow.Exams
            .Setup(x => x.GetByKeyAsync(6, 10, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);
        uow.Terms
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Term(2));
        uow.Terms
            .Setup(x => x.GetPreviousTermAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Term?)null);
        uow.Exams
            .Setup(x => x.Add(It.IsAny<Exam>()))
            .Callback<Exam>(exam =>
            {
                exam.ID = 15;
                added = exam;
            });
        uow.SetupCommit();

        var response = await service.CreateAsync(10, 2, 6, request, 4);

        response.ID.Should().Be(15);
        response.Grade.Should().Be(request.Grade);
        response.ExamDate.Should().Be(request.Date);
        added.Should().NotBeNull();
        added!.StudentID.Should().Be(6);
        added.SubjectID.Should().Be(10);
        added.TermID.Should().Be(2);
        added.TeacherID.Should().Be(4);
        added.SignedAt.Should().BeNull();
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsForbidden_WhenTeacherCannotGradeSubject()
    {
        var uow = new UnitOfWorkMock();
        var service = new ExamService(uow.Object, new TestClock());

        uow.TeachingAssignments
            .Setup(x => x.GetAsync(4, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TeachingAssignment { TeacherID = 4, SubjectID = 10, CanGrade = false });

        var act = () => service.CreateAsync(
            subjectId: 10,
            termId: 2,
            studentId: 6,
            req: new CreateExamRequest { Grade = 8, Date = new DateOnly(2026, 2, 12) },
            teacherId: 4);

        await act.Should().ThrowAsync<AppException>()
            .Where(ex => ex.Code == AppErrorCode.Forbidden)
            .WithMessage("Teacher cannot grade this subject.");

        uow.Exams.Verify(x => x.Add(It.IsAny<Exam>()), Times.Never);
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenPreviousTermHasUnsignedExams()
    {
        var uow = new UnitOfWorkMock();
        var service = new ExamService(uow.Object, new TestClock());
        var previousTerm = Term(1);

        SetupTeacherCanGrade(uow, teacherId: 4, subjectId: 10);
        uow.Registrations
            .Setup(x => x.ExistsActiveAsync(6, 10, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        uow.Exams
            .Setup(x => x.GetByKeyAsync(6, 10, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);
        uow.Terms
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Term(2));
        uow.Terms
            .Setup(x => x.GetPreviousTermAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousTerm);
        uow.Registrations
            .Setup(x => x.ListActiveBySubjectAndTermWithExamAsync(10, previousTerm.ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Registration>());
        uow.Exams
            .Setup(x => x.ListUnsignedBySubjectTermWithRegistrationAsync(10, previousTerm.ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Exam> { new() { StudentID = 6, SubjectID = 10, TermID = previousTerm.ID } });

        var act = () => service.CreateAsync(
            10,
            2,
            6,
            new CreateExamRequest { Grade = 8, Date = new DateOnly(2026, 2, 12) },
            4);

        await act.Should().ThrowAsync<AppException>()
            .Where(ex => ex.Code == AppErrorCode.Conflict)
            .WithMessage("Previous term 'Term 1' is not finalized. Some exams are still not locked.");

        uow.Exams.Verify(x => x.Add(It.IsAny<Exam>()), Times.Never);
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LockAsync_SignsUnsignedExams_DeactivatesRegistrations_AndMarksPassingEnrollments()
    {
        var uow = new UnitOfWorkMock();
        var clock = new TestClock
        {
            Today = new DateOnly(2026, 2, 12),
            UtcNow = new DateTime(2026, 2, 12, 14, 0, 0, DateTimeKind.Utc)
        };
        var service = new ExamService(uow.Object, clock);
        var request = new LockExamsRequest { SubjectID = 10, TermID = 2 };
        var passedRegistration = Registration(studentId: 6, subjectId: 10, termId: 2);
        var failedRegistration = Registration(studentId: 7, subjectId: 10, termId: 2);
        var passedExam = Exam(id: 21, studentId: 6, grade: 8, passedRegistration);
        var failedExam = Exam(id: 22, studentId: 7, grade: 5, failedRegistration);
        var passedEnrollment = new Enrollment { StudentID = 6, SubjectID = 10, IsPassed = false };
        var failedEnrollment = new Enrollment { StudentID = 7, SubjectID = 10, IsPassed = false };

        passedRegistration.Exam = passedExam;
        failedRegistration.Exam = failedExam;

        SetupTeacherCanGrade(uow, teacherId: 4, subjectId: request.SubjectID);
        uow.Terms
            .Setup(x => x.GetByIdAsync(request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Term(request.TermID));
        uow.Terms
            .Setup(x => x.GetPreviousTermAsync(request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Term?)null);
        uow.Registrations
            .Setup(x => x.ListActiveBySubjectAndTermWithExamAsync(request.SubjectID, request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Registration> { passedRegistration, failedRegistration });
        uow.Exams
            .Setup(x => x.ListBySubjectTermAsync(request.SubjectID, request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Exam> { passedExam, failedExam });
        uow.Exams
            .Setup(x => x.ListUnsignedBySubjectTermWithRegistrationAsync(request.SubjectID, request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Exam> { passedExam, failedExam });
        uow.Enrollments
            .Setup(x => x.ListByStudentsAndSubjectAsync(
                It.Is<List<int>>(ids => ids.OrderBy(id => id).SequenceEqual(new[] { 6, 7 })),
                request.SubjectID,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Enrollment> { passedEnrollment, failedEnrollment });
        uow.SetupCommit();

        var locked = await service.LockAsync(request, teacherId: 4);

        locked.Should().Be(2);
        passedExam.SignedAt.Should().Be(clock.UtcNow);
        failedExam.SignedAt.Should().Be(clock.UtcNow);
        passedExam.TeacherID.Should().Be(4);
        failedExam.TeacherID.Should().Be(4);
        passedRegistration.IsActive.Should().BeFalse();
        failedRegistration.IsActive.Should().BeFalse();
        passedEnrollment.IsPassed.Should().BeTrue();
        passedEnrollment.PassedAt.Should().Be(clock.UtcNow);
        failedEnrollment.IsPassed.Should().BeFalse();
        failedEnrollment.PassedAt.Should().BeNull();
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LockAsync_ThrowsConflict_WhenActiveRegistrationDoesNotHaveExam()
    {
        var uow = new UnitOfWorkMock();
        var service = new ExamService(uow.Object, new TestClock { Today = new DateOnly(2026, 2, 12) });
        var request = new LockExamsRequest { SubjectID = 10, TermID = 2 };

        SetupTeacherCanGrade(uow, teacherId: 4, subjectId: request.SubjectID);
        uow.Terms
            .Setup(x => x.GetByIdAsync(request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Term(request.TermID));
        uow.Terms
            .Setup(x => x.GetPreviousTermAsync(request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Term?)null);
        uow.Registrations
            .Setup(x => x.ListActiveBySubjectAndTermWithExamAsync(request.SubjectID, request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Registration> { Registration(studentId: 6, subjectId: 10, termId: 2) });
        uow.Exams
            .Setup(x => x.ListBySubjectTermAsync(request.SubjectID, request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Exam>());

        var act = () => service.LockAsync(request, teacherId: 4);

        await act.Should().ThrowAsync<AppException>()
            .Where(ex => ex.Code == AppErrorCode.Conflict)
            .WithMessage("Cannot lock exams because some active registrations do not have exams.");

        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static void SetupTeacherCanGrade(UnitOfWorkMock uow, int teacherId, int subjectId)
    {
        uow.TeachingAssignments
            .Setup(x => x.GetAsync(teacherId, subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TeachingAssignment { TeacherID = teacherId, SubjectID = subjectId, CanGrade = true });
    }

    private static Term Term(int id) => new()
    {
        ID = id,
        Name = $"Term {id}",
        RegistrationStartDate = new DateOnly(2026, 2, 1),
        RegistrationEndDate = new DateOnly(2026, 2, 5),
        StartDate = new DateOnly(2026, 2, 10),
        EndDate = new DateOnly(2026, 2, 20)
    };

    private static Registration Registration(int studentId, int subjectId, int termId) => new()
    {
        StudentID = studentId,
        SubjectID = subjectId,
        TermID = termId,
        IsActive = true
    };

    private static Exam Exam(int id, int studentId, byte grade, Registration registration) => new()
    {
        ID = id,
        StudentID = studentId,
        SubjectID = registration.SubjectID,
        TermID = registration.TermID,
        Grade = grade,
        Date = new DateOnly(2026, 2, 12),
        Registration = registration
    };
}
