using Application.DTO.Students;
using Application.Services;
using Domain.Entity;
using Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Application.Common;
using FluentAssertions.Specialized;
using static System.Net.Mime.MediaTypeNames;


namespace Tests.Service
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _repo;
        private readonly StudentService _svc;
        public StudentServiceTests()
        {
            _repo = new Mock<IStudentRepository>();
            _svc= new StudentService(_repo.Object);
        }

        [Fact]
        public void GetAsync_ReturnsStudentResponse_WhenStudentExists()
        {
            //arrange
            int id = 26;
            Student student = new Student
            {
                ID=id,
                FirstName = "Nikola",
                LastName = "Nikolic",
                JMBG = "0211995701236",
                DateOfBirth = new DateOnly(1995, 2, 11),
                IndexNumber="2023/123"
            };
            _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(student); //student exists

            // act
            StudentResponse? resp = _svc.GetAsync(id).Result;

            // assert (state)
            Assert.NotNull(resp);
            Assert.Equal(id, resp!.Id);
            Assert.Equal("Nikola", resp.FirstName);
            Assert.Equal("Nikolic", resp.LastName);
            Assert.Equal("2023/123", resp.IndexNumber);

            // assert (behavior)
            _repo.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAsync_ThrowsAppExc_WhenStudentDoesNotExist()
        {
            // arrange
            int id = 26;
            _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Student?)null); //student with id=26 does not exists

            // act
            Func<Task> act = () => _svc.GetAsync(id, CancellationToken.None);

            // assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code==AppErrorCode.NotFound)
                .WithMessage("Student with id 26 not found.");

            // assert (behavior)
            _repo.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateAsync_ReturnsStudentResponse_WhenCreateIsOK()
        {
            // arrange
            CreateStudentRequest req = new CreateStudentRequest
            {
                JMBG = "0211995701236",
                FirstName = "Milica",
                LastName = "Tosic",
                DateOfBirth = new DateOnly(1995, 11, 2),
                IndexNumber = "2020/1234"
            };

            _repo.Setup(r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(13);

            Student entity = new Student
            {
                ID = 13,
                JMBG = req.JMBG,
                FirstName = req.FirstName,
                LastName = req.LastName,
                DateOfBirth = req.DateOfBirth,
                IndexNumber = req.IndexNumber
            };

            _repo.Setup(r => r.GetByIdAsync(13, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

            // act
            StudentResponse resp = await _svc.CreateAsync(req);

            // assert (state)
            resp.Should().NotBeNull();
            resp.Id.Should().Be(13);
            resp.FirstName.Should().Be("Milica");
            resp.LastName.Should().Be("Tosic");

            // assert (behavior)
            _repo.Verify(r => r.CreateAsync(
                It.Is<Student>(s =>
                    s.FirstName == "Milica" &&
                    s.LastName == "Tosic" &&
                    s.JMBG == "0211995701236" &&
                    s.DateOfBirth == new DateOnly(1995, 11, 2) &&
                    s.IndexNumber == "2020/1234"
                ),
                It.IsAny<CancellationToken>()), Times.Once);

            _repo.Verify(r => r.GetByIdAsync(13, It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task CreateAsync_Throws_WhenJmbgAlreadyExists()
        {
            //arrange
            CreateStudentRequest req = new CreateStudentRequest
            {
                JMBG = "0211995701236",
                FirstName = "Milica",
                LastName = "Tosic",
                DateOfBirth = new DateOnly(1995, 11, 2),
                IndexNumber = "2020/1234"
            };

            _repo.Setup(r => r.ExistsByJmbgAsync("0211995701236", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);
            _repo.Setup(r => r.ExistsByIndexAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

            //act + Assert
            Func<Task> act = () => _svc.CreateAsync(req);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code==AppErrorCode.Conflict)
                .WithMessage("Student with this JMBG already exists.");

            _repo.Verify(r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task UpdateAsync_ChangesOnlyLastName_WhenOk()
        {
            // Arrange
            Student existing = new Student
            {
                ID=10,
                FirstName = "Ana",
                LastName = "Anic",
                DateOfBirth = new DateOnly(2000, 1, 1),
                IndexNumber = "2020/001"
            };

            _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            UpdateStudentRequest req = new UpdateStudentRequest
            {
                LastName = "Anić"
            };

            // Act
            await _svc.UpdateAsync(10, req);

            // Assert
            _repo.Verify(r => r.UpdateAsync(
                It.Is<Student>(s =>
                    s.ID == 10 &&
                    s.FirstName == "Ana" &&
                    s.LastName == "Anić" &&
                    s.DateOfBirth == new DateOnly(2000, 1, 1) &&
                    s.IndexNumber == "2020/001"
                ),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _repo.Verify(r => r.ExistsByIndexAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenNewIndexAlreadyExists()
        {
            //arrange
            Student existing = new Student
            {
                FirstName = "Mila",
                LastName = "Milić",
                DateOfBirth = new DateOnly(1999, 5, 5),
                IndexNumber = "2019/123"
            };

            _repo.Setup(r => r.GetByIdAsync(11, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            _repo.Setup(r => r.ExistsByIndexAsync("2019/999", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            UpdateStudentRequest req = new UpdateStudentRequest
            {
                IndexNumber = "2019/999"
            };

            //act
            Func<Task> act = () => _svc.UpdateAsync(11, req);

            // assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code==AppErrorCode.Conflict)
                .WithMessage("Index already exists.");

            _repo.Verify(r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Deletes_WhenFound()
        {
            //arrange
            Student existing = new Student { ID = 15 };
            _repo.Setup(r => r.GetByIdAsync(15, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            //act
            await _svc.DeleteAsync(15);

            //assert
            _repo.Verify(r => r.DeleteAsync(
                It.Is<Student>(s => s.ID == 15),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenNotFound()
        {
            //arrange
            _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Student?)null);

            //act
            Func<Task> act = () => _svc.DeleteAsync(999, CancellationToken.None);

            //assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.NotFound)
                .WithMessage("Student with id 999 not found.");

            _repo.Verify(r => r.DeleteAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetExamsAsync_WhenStudentNotFound_ThrowsNotFound()
        {
            // arrange
            _repo.Setup(r => r.GetByIdAsync(26, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Student?)null);

            // act
            Func<Task> act = () => _svc.GetExamsAsync(26, CancellationToken.None);

            // assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code== AppErrorCode.NotFound)
                .WithMessage("Student with id 26 not found.");

            _repo.Verify(r => r.GetByIdAsync(26, It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetExamsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetExamsAsync_WhenStudentExists_ReturnsMappedList()
        {
            // arrange
            _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Student { ID = 1 });

            var exams = new[]
            {
            new Exam { ID = 10, StudentID = 1, SubjectID = 2, ExaminerID = 3, SupervisorID = null, Grade = 8, Date = new DateOnly(2025, 10, 5), Note = "ispravka" },
            new Exam { ID = 11, StudentID = 1, SubjectID = 5, ExaminerID = 6, SupervisorID = 7, Grade = 9, Date = new DateOnly(2025, 9, 20),  Note = null }
            };
            _repo.Setup(r => r.GetExamsAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(exams);

            // act
            var result = await _svc.GetExamsAsync(1, CancellationToken.None);

            // assert
            result.Should().NotBeNull().And.HaveCount(2);

            result[0].ID.Should().Be(10);
            result[0].Grade.Should().Be(8);

            result[1].ID.Should().Be(11);
            result[1].Grade.Should().Be(9);

            _repo.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetExamsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetExamsAsync_WhenNoExams_ReturnsEmptyList()
        {
            // arrange
            _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Student { ID = 5 });

            _repo.Setup(r => r.GetExamsAsync(5, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(Array.Empty<Exam>());

            // act
            var result = await _svc.GetExamsAsync(5, CancellationToken.None);

            // assert
            result.Should().NotBeNull().And.BeEmpty();

            _repo.Verify(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetExamsAsync(5, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

    }
}
