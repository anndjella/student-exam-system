using FluentAssertions;
using Infrastructure.Data;
using Tests.TestDoubles;

namespace Tests.Infrastructure;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task RepositoryProperties_ReturnSameInstancesAcrossCalls()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using var _ = connection;
        await using var __ = db;
        var unitOfWork = new UnitOfWork(db);

        unitOfWork.Students.Should().BeSameAs(unitOfWork.Students);
        unitOfWork.Teachers.Should().BeSameAs(unitOfWork.Teachers);
        unitOfWork.People.Should().BeSameAs(unitOfWork.People);
        unitOfWork.Users.Should().BeSameAs(unitOfWork.Users);
        unitOfWork.Subjects.Should().BeSameAs(unitOfWork.Subjects);
        unitOfWork.Enrollments.Should().BeSameAs(unitOfWork.Enrollments);
        unitOfWork.TeachingAssignments.Should().BeSameAs(unitOfWork.TeachingAssignments);
        unitOfWork.Terms.Should().BeSameAs(unitOfWork.Terms);
        unitOfWork.Registrations.Should().BeSameAs(unitOfWork.Registrations);
        unitOfWork.Exams.Should().BeSameAs(unitOfWork.Exams);
        unitOfWork.StudentStats.Should().BeSameAs(unitOfWork.StudentStats);
    }
}
