using Application.Common;
using Application.DTO.Students;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Tests.TestDoubles;

namespace Tests.Services;

public sealed class StudentServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesStudentAndUser_WhenRequestIsValid()
    {
        var uow = new UnitOfWorkMock();
        var service = new StudentService(uow.Object);
        var request = ValidCreateRequest();
        Student? addedStudent = null;
        User? addedUser = null;

        uow.People
            .Setup(x => x.ExistsByJmbgAsync(request.JMBG, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        uow.Students
            .Setup(x => x.ExistsByIndexAsync(request.IndexNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        uow.Users
            .Setup(x => x.ExistsByUsernameAsync("aa20241234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        uow.Students
            .Setup(x => x.Add(It.IsAny<Student>()))
            .Callback<Student>(student =>
            {
                student.ID = 42;
                addedStudent = student;
            });
        uow.Users
            .Setup(x => x.Add(It.IsAny<User>()))
            .Callback<User>(user => addedUser = user);
        uow.SetupCommit();
        uow.Students
            .Setup(x => x.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => addedStudent);

        var response = await service.CreateAsync(request);

        response.ID.Should().Be(42);
        response.FirstName.Should().Be(request.FirstName);
        response.LastName.Should().Be(request.LastName);
        response.IndexNumber.Should().Be(request.IndexNumber);
        response.DateOfBirth.Should().Be(new DateOnly(1995, 1, 2));

        addedStudent.Should().NotBeNull();
        addedStudent!.User.Should().BeSameAs(addedUser);
        addedUser.Should().NotBeNull();
        addedUser!.Role.Should().Be(UserRole.Student);
        addedUser.Username.Should().Be("aa20241234");
        addedUser.PasswordHash.Should().NotBe("TEMP");
        addedUser.MustChangePassword.Should().BeTrue();

        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenJmbgAlreadyExists()
    {
        var uow = new UnitOfWorkMock();
        var service = new StudentService(uow.Object);
        var request = ValidCreateRequest();

        uow.People
            .Setup(x => x.ExistsByJmbgAsync(request.JMBG, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => service.CreateAsync(request);

        await act.Should().ThrowAsync<AppException>()
            .Where(ex => ex.Code == AppErrorCode.Conflict)
            .WithMessage("Person with this JMBG already exists.");

        uow.Students.Verify(x => x.Add(It.IsAny<Student>()), Times.Never);
        uow.Users.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ChangesIndex_WhenNewIndexIsAvailable()
    {
        var uow = new UnitOfWorkMock();
        var service = new StudentService(uow.Object);
        var student = Student(id: 7, indexNumber: "2023/0001");
        var request = new UpdateStudentRequest
        {
            FirstName = "Mila",
            IndexNumber = "2024/0002"
        };

        uow.Students
            .Setup(x => x.GetByIdAsync(student.ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);
        uow.Students
            .Setup(x => x.ExistsByIndexAsync(request.IndexNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        uow.SetupCommit();

        await service.UpdateAsync(student.ID, request);

        student.FirstName.Should().Be("Mila");
        student.LastName.Should().Be("Anic");
        student.IndexNumber.Should().Be("2024/0002");
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_MarksStudentDeletedAndDeactivatesUser()
    {
        var uow = new UnitOfWorkMock();
        var service = new StudentService(uow.Object);
        var student = Student(id: 8, indexNumber: "2024/0008");
        student.User = new User(UserRole.Student, "student", "hash");

        uow.Students
            .Setup(x => x.GetByIdWithUserAsync(student.ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);
        uow.SetupCommit();

        await service.SoftDeleteAsync(student.ID);

        student.IsDeleted.Should().BeTrue();
        student.DeletedAt.Should().NotBeNull();
        student.User.isActive.Should().BeFalse();
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListAsync_NormalizesPagingAndMapsStats()
    {
        var uow = new UnitOfWorkMock();
        var service = new StudentService(uow.Object);
        var students = new List<Student>
        {
            Student(id: 1, indexNumber: "2024/0001"),
            Student(id: 2, indexNumber: "2024/0002")
        };
        var stats = new List<StudentStats>
        {
            new() { StudentID = 1, GPA = 8.75m, ECTSCount = 60 }
        };

        uow.Students
            .Setup(x => x.CountAsync("ana", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        uow.Students
            .Setup(x => x.ListPagedAsync(0, 100, "ana", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        uow.StudentStats
            .Setup(x => x.ListByStudentIdsAsync(It.Is<List<int>>(ids => ids.SequenceEqual(new[] { 1, 2 })), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var response = await service.ListAsync(-5, 200, "ana", false, CancellationToken.None);

        response.Total.Should().Be(2);
        response.Items.Should().HaveCount(2);
        response.Items[0].GPA.Should().Be(8.75);
        response.Items[0].ECTSCount.Should().Be(60);
        response.Items[1].GPA.Should().BeNull();
    }

    private static CreateStudentRequest ValidCreateRequest() => new()
    {
        JMBG = "0201995701231",
        FirstName = "Ana",
        LastName = "Anic",
        IndexNumber = "2024/1234"
    };

    private static Student Student(int id, string indexNumber) => new()
    {
        ID = id,
        JMBG = "0201995701231",
        FirstName = "Ana",
        LastName = "Anic",
        DateOfBirth = new DateOnly(1995, 1, 2),
        IndexNumber = indexNumber
    };
}
