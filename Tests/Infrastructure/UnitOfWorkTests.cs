using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Tests.TestDoubles;

namespace Tests.Infrastructure;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task RepositoryProperties_ReturnExpectedRepositoryTypes()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using var _ = connection;
        await using var __ = db;
        var unitOfWork = new UnitOfWork(db);

        unitOfWork.Students.Should().BeOfType<StudentRepository>();
        unitOfWork.Teachers.Should().BeOfType<TeacherRepository>();
        unitOfWork.People.Should().BeOfType<PersonRepository>();
        unitOfWork.Users.Should().BeOfType<UserRepository>();
        unitOfWork.Subjects.Should().BeOfType<SubjectRepository>();
        unitOfWork.Enrollments.Should().BeOfType<EnrollmentRepository>();
        unitOfWork.TeachingAssignments.Should().BeOfType<TeachingAssignmentRepository>();
        unitOfWork.Terms.Should().BeOfType<TermRepository>();
        unitOfWork.Registrations.Should().BeOfType<RegistrationRepository>();
        unitOfWork.Exams.Should().BeOfType<ExamRepository>();
        unitOfWork.StudentStats.Should().BeOfType<StudentStatsRepository>();
    }
}
