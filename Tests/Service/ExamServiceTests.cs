using System;
using System.Threading.Tasks;
using Application.DTO.Exams;
using Application.ServicesImplementation;
using Domain.Entity;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

public class ExamServiceTests
{
    [Fact]
    public async Task Create_WhenAlreadyPassed_Throws()
    {
        var repo = new Mock<IExamRepository>();
        var students = new Mock<IStudentRepository>();
        var teachers = new Mock<ITeacherRepository>();
        var subjects = new Mock<ISubjectRepository>();

        students.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(new Student { ID = 1 });
        subjects.Setup(x => x.GetByIdAsync(2, default)).ReturnsAsync(new Subject { ID = 2, ESPB = 8, Name = "Math" });
        teachers.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync(new Teacher { ID = 3, Title = Title.FullProfessor });
        repo.Setup(x => x.HasPassedAsync(1, 2, default)).ReturnsAsync(true);

        var sut = new ExamService(repo.Object, students.Object, teachers.Object, subjects.Object);

        var req = new CreateExamRequest
        {
            StudentID = 1,
            SubjectID = 2,
            ExaminerID = 3,
            SupervisorID = null,
            Grade = 7,
            Date = new DateOnly(2024, 1, 10),
            Note = null
        };

        await FluentActions.Invoking(() => sut.CreateAsync(req))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Student already passed this subject.");
    }
}
