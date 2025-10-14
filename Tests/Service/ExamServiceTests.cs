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
    private readonly Mock<IExamRepository> _repo;
    private readonly Mock<IStudentRepository> _students;
    private readonly Mock<ITeacherRepository> _teachers;
    private readonly Mock<ISubjectRepository> _subjects;
    public ExamServiceTests()
    {
        _repo = new Mock<IExamRepository>();
        _students = new Mock<IStudentRepository>();
        _teachers = new Mock<ITeacherRepository>();
        _subjects = new Mock<ISubjectRepository>();
    }
    [Fact]
    public async Task Create_WhenAlreadyPassed_Throws()
    {

        _students.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(new Student { ID = 1 });
        _subjects.Setup(x => x.GetByIdAsync(2, default)).ReturnsAsync(new Subject { ID = 2, ESPB = 8, Name = "Math" });
        _teachers.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync(new Teacher { ID = 3, Title = Title.FullProfessor });
        _repo.Setup(x => x.HasPassedAsync(1, 2, default)).ReturnsAsync(true);

        ExamService sut = new ExamService(_repo.Object, _students.Object, _teachers.Object, _subjects.Object);

        CreateExamRequest req = new CreateExamRequest
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
