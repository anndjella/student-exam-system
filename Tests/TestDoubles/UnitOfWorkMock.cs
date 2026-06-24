using Application.Common;
using Domain.Interfaces;
using Moq;

namespace Tests.TestDoubles;

internal sealed class UnitOfWorkMock
{
    public Mock<IUnitOfWork> Mock { get; } = new(MockBehavior.Strict);
    public IUnitOfWork Object => Mock.Object;

    public Mock<IStudentRepository> Students { get; } = new(MockBehavior.Strict);
    public Mock<ITeacherRepository> Teachers { get; } = new(MockBehavior.Strict);
    public Mock<IPersonRepository> People { get; } = new(MockBehavior.Strict);
    public Mock<IUserRepository> Users { get; } = new(MockBehavior.Strict);
    public Mock<ISubjectRepository> Subjects { get; } = new(MockBehavior.Strict);
    public Mock<IEnrollmentRepository> Enrollments { get; } = new(MockBehavior.Strict);
    public Mock<ITeachingAssignmentRepository> TeachingAssignments { get; } = new(MockBehavior.Strict);
    public Mock<ITermRepository> Terms { get; } = new(MockBehavior.Strict);
    public Mock<IRegistrationRepository> Registrations { get; } = new(MockBehavior.Strict);
    public Mock<IExamRepository> Exams { get; } = new(MockBehavior.Strict);
    public Mock<IStudentStatsRepository> StudentStats { get; } = new(MockBehavior.Strict);

    public UnitOfWorkMock()
    {
        Mock.SetupGet(x => x.Students).Returns(Students.Object);
        Mock.SetupGet(x => x.Teachers).Returns(Teachers.Object);
        Mock.SetupGet(x => x.People).Returns(People.Object);
        Mock.SetupGet(x => x.Users).Returns(Users.Object);
        Mock.SetupGet(x => x.Subjects).Returns(Subjects.Object);
        Mock.SetupGet(x => x.Enrollments).Returns(Enrollments.Object);
        Mock.SetupGet(x => x.TeachingAssignments).Returns(TeachingAssignments.Object);
        Mock.SetupGet(x => x.Terms).Returns(Terms.Object);
        Mock.SetupGet(x => x.Registrations).Returns(Registrations.Object);
        Mock.SetupGet(x => x.Exams).Returns(Exams.Object);
        Mock.SetupGet(x => x.StudentStats).Returns(StudentStats.Object);
    }

    public void SetupCommit(int result = 1)
        => Mock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);

    public void VerifyNoUnexpectedCalls()
    {
        Students.VerifyNoOtherCalls();
        Teachers.VerifyNoOtherCalls();
        People.VerifyNoOtherCalls();
        Users.VerifyNoOtherCalls();
        Subjects.VerifyNoOtherCalls();
        Enrollments.VerifyNoOtherCalls();
        TeachingAssignments.VerifyNoOtherCalls();
        Terms.VerifyNoOtherCalls();
        Registrations.VerifyNoOtherCalls();
        Exams.VerifyNoOtherCalls();
        StudentStats.VerifyNoOtherCalls();
        Mock.VerifyNoOtherCalls();
    }
}
