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
        private readonly CreateStudentRequest _req;
        private readonly Student _student;
        private readonly int _id;
        public StudentServiceTests()
        {
            _repo = new Mock<IStudentRepository>(MockBehavior.Strict);
            _svc= new StudentService(_repo.Object);
            _req = new CreateStudentRequest
            {
                JMBG = "0211995701231",
                FirstName = "Ana",
                LastName = "Anic",
                DateOfBirth = new DateOnly(1995, 11, 2),
                IndexNumber = "2020/1234"
            };
            _id = 26;
            _student = Mapper.CreateToStudent(_req, _id);
        }

        [Fact]
        public void GetAsync_ReturnsStudentResponse_WhenStudentExists()
        {
            //arrange
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_student); //student exists

            // act
            StudentResponse? resp = _svc.GetAsync(_id).Result;

            // assert (state)
            resp.Should().NotBeNull();
            resp.Id.Should().Be(_id);
            resp.FirstName.Should().Be(_student.FirstName);
            resp.LastName.Should().Be(_student.LastName);

            // assert (behavior)
            _repo.Verify(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAsync_ThrowsAppExc_WhenStudentDoesNotExist()
        {
            // arrange
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Student?)null); //student with id=26 does not exists

            // act
            Func<Task> act = () => _svc.GetAsync(_id, CancellationToken.None);

            // assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code==AppErrorCode.NotFound)
                .WithMessage("Student with id 26 not found.");

            // assert (behavior)
            _repo.Verify(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }
        [Fact]
        public async Task CreateAsync_ReturnsStudentResponse_WhenCreateIsOK()
        {
            // arrange
            _repo.Setup(r => r.ExistsByJmbgAsync(It.IsAny<string>(), default))
                .ReturnsAsync(false);
            _repo.Setup(r => r.ExistsByIndexAsync(It.IsAny<string>(), default))
             .ReturnsAsync(false);

            _repo.Setup(r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_id);

            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_student);

            // act
            StudentResponse resp = await _svc.CreateAsync(_req);

            // assert (state)
            resp.Should().NotBeNull();
            resp.Id.Should().Be(_id);
            resp.FirstName.Should().Be(_req.FirstName);
            resp.LastName.Should().Be(_req.LastName);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenJmbgAlreadyExists()
        {
            //arrange

            _repo.Setup(r => r.ExistsByJmbgAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);
            _repo.Setup(r => r.ExistsByIndexAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

            //act + Assert
            Func<Task> act = () => _svc.CreateAsync(_req);

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
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_student);

            Student? captured = null;
            _repo.Setup(r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
                 .Callback<Student, CancellationToken>((s, _) => captured = s)
                 .Returns(Task.CompletedTask);

            var req = new UpdateStudentRequest { LastName = "Anić" };

            // Act
            await _svc.UpdateAsync(_id, req);

            // Assert
            _repo.Verify(r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.ExistsByIndexAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            captured.Should().NotBeNull();
            captured!.ID.Should().Be(_id);
            captured.FirstName.Should().Be(_student.FirstName);
            captured.JMBG.Should().Be(_req.JMBG);
            captured.LastName.Should().Be(req.LastName);
            captured.IndexNumber.Should().Be(_student.IndexNumber);
            captured.DateOfBirth.Should().Be(_student.DateOfBirth);
        }
        [Fact]
        public async Task UpdateAsync_Throws_WhenNewIndexAlreadyExists()
        {
            //arrange 
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_student);

            UpdateStudentRequest UpdateReq = new UpdateStudentRequest
            {
                IndexNumber = "2020/1233"
            };

            _repo.Setup(r => r.ExistsByIndexAsync(UpdateReq.IndexNumber, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);


            //act
            Func<Task> act = () => _svc.UpdateAsync(_id, UpdateReq);

            // assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.Conflict)
                .WithMessage("Index already exists.");

            _repo.Verify(r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Deletes_WhenFound()
        {
            //arrange
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_student);

            Student? captured = null;
            _repo.Setup(r => r.DeleteAsync(It.IsAny<Student>(),default))
                 .Callback<Student, CancellationToken>((s, _) => captured = s)
                 .Returns(Task.CompletedTask);

            //act
            await _svc.DeleteAsync(_id);

            //assert
            captured.Should().NotBeNull();
            captured.ID.Should().Be(_id);
            
            _repo.Verify(r=>r.DeleteAsync(It.IsAny<Student>(),default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenNotFound()
        {
            //arrange
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
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
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Student?)null);

            // act
            Func<Task> act = () => _svc.GetExamsAsync(_id, CancellationToken.None);

            // assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.NotFound)
                .WithMessage("Student with id 26 not found.");

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetExamsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetExamsAsync_WhenStudentExists_ReturnsMappedList()
        {
            // arrange
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_student);

            var exams = new[]
            {
            new Exam { StudentID = 1, SubjectID = 2, ExaminerID = 3, SupervisorID = null, Grade = 8, Date = new DateOnly(2025, 10, 5), Note = "ispravka" },
            new Exam { StudentID = 1, SubjectID = 5, ExaminerID = 6, SupervisorID = 7, Grade = 9, Date = new DateOnly(2025, 9, 20),  Note = null }
            };
            _repo.Setup(r => r.GetExamsAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(exams);

            // act
            var result = await _svc.GetExamsAsync(_id, CancellationToken.None);

            // assert
            result.Should().NotBeNull().And.HaveCount(2);

            var expected = exams.Select(e => new { e.Grade }).ToArray();
            var actual = result.Select(r => new { r.Grade }).ToArray();

            actual.Should().BeEquivalentTo(expected, opts => opts.WithoutStrictOrdering());

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetExamsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetExamsAsync_WhenNoExams_ReturnsEmptyList()
        {
            // arrange
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_student);

            _repo.Setup(r => r.GetExamsAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(Array.Empty<Exam>());

            // act
            var result = await _svc.GetExamsAsync(_id, CancellationToken.None);

            // assert
            result.Should().NotBeNull().And.BeEmpty();

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetExamsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

    }
}
