using System;
using System.Threading.Tasks;
using Application.Common;
using Application.DTO.Exams;
using Application.ServicesImplementation;
using Domain.Entity;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Service
{
    public class ExamServiceTests
    {
        private readonly Mock<IExamRepository> _repo;
        private readonly Mock<IStudentRepository> _students;
        private readonly Mock<ITeacherRepository> _teachers;
        private readonly Mock<ISubjectRepository> _subjects;
        private readonly ExamService _svc;
        public ExamServiceTests()
        {
            _repo = new Mock<IExamRepository>();
            _students = new Mock<IStudentRepository>();
            _teachers = new Mock<ITeacherRepository>();
            _subjects = new Mock<ISubjectRepository>();
            _svc = new ExamService(_repo.Object, _students.Object, _teachers.Object, _subjects.Object);
        }
        [Fact]

        public async Task Create_OK()
        {
            _students.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(new Student { ID = 1 });
            _subjects.Setup(x => x.GetByIdAsync(2, default)).ReturnsAsync(new Subject { ID = 2 });
            _teachers.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync(new Teacher { ID = 3 });
            _repo.Setup(r => r.HasPassedAsync(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
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

            _repo.Setup(r => r.CreateAsync(It.IsAny<Exam>(), default));
            _repo.Setup(r => r.GetByKeyWithDetailsAsync(1,2, new DateOnly(2024, 1, 10), default))
                .ReturnsAsync(new Exam { Grade = 7 });

             await _svc.CreateAsync(req);

            _repo.Verify(r => r.CreateAsync(It.IsAny<Exam>(), default), Times.Once);
        }

        [Fact]
        public async Task Create_WhenAlreadyPassed_Throws()
        {

            _students.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(new Student { ID = 1 });
            _subjects.Setup(x => x.GetByIdAsync(2, default)).ReturnsAsync(new Subject { ID = 2 });
            _teachers.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync(new Teacher { ID = 3 });
            _repo.Setup(x => x.HasPassedAsync(1, 2, default)).ReturnsAsync(true);

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

            Func<Task> act = () => _svc.CreateAsync(req);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.Conflict)
                .WithMessage("Student already passed this subject.");
        }
        [Fact]
        public async Task Update_Ok()
        {
            _repo.Setup(r => r.GetByKeyAsync(1, 2, new DateOnly(2024, 1, 10), default))
               .ReturnsAsync(new Exam { Grade = 6 });
            UpdateExamRequest req = new UpdateExamRequest
            {
                Grade = 7,
                Note = "Greška"
            };
            await _svc.UpdateAsync(1, 2, new DateOnly(2024, 1, 10), req, default);

            _repo.Verify(r => r.UpdateAsync(It.Is<Exam>(e => e.Grade == 7 && e.Note=="Greška"), default));
            _repo.Verify(r => r.UpdateAsync(It.IsAny<Exam>(),default), Times.Once);
        }
        [Fact]
        public async Task Update_WhenNoteNull_Throws()
        {
            _repo.Setup(r => r.GetByKeyAsync(1, 2, new DateOnly(2024, 1, 10), default))
               .ReturnsAsync(new Exam {Grade = 6 });
            UpdateExamRequest req = new UpdateExamRequest
            {
                Grade = 7,
                Note = null
            };
            Func<Task> act = async () => await _svc.UpdateAsync(1, 2, new DateOnly(2024, 1, 10), req, default);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.BadRequest)
                .WithMessage("Note is required when changing the grade.");
        }
    }

}