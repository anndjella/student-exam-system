using Application.Common;
using Application.DTO.Exams;
using Application.DTO.Teachers;
using Application.Services;
using Application.ServicesImplementation;
using Domain.Entity;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Text.RegularExpressions;

namespace Tests.Service
{
    public class TeacherServiceTests
    {
        private readonly Mock<ITeacherRepository> _repo;
        private readonly TeacherService _svc;
        private readonly CreateTeacherRequest _req;
        private readonly Teacher _teacher;
        private readonly int _id;

        public TeacherServiceTests()
        {
            _repo = new Mock<ITeacherRepository>(MockBehavior.Strict);
            _svc = new TeacherService(_repo.Object);
            _req = new CreateTeacherRequest
            {
                JMBG = "0409974732292",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1974, 9, 4),
                Title = Title.FullProfessor
            };
            _id = 26;
            _teacher = Mapper.CreateToTeacher(_req, _id);
        }

        [Fact]
        public async Task GetAsync_ReturnsTeacherResponse_WhenExists()
        {
            // arrange
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_teacher);

            // act
            var resp = await _svc.GetAsync(_id);

            // assert (state)
            resp.Should().NotBeNull();
            resp!.Id.Should().Be(_id);
            resp.FirstName.Should().Be(_teacher.FirstName);
            resp.LastName.Should().Be(_teacher.LastName);
            resp.Title.Should().Be(_teacher.Title);

            // assert (behavior)
            _repo.Verify(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAsync_ThrowsNotFound_WhenMissing()
        {
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Teacher?)null);

            var act = () => _svc.GetAsync(_id, CancellationToken.None);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.NotFound)
                .WithMessage("Teacher with id 26 not found.");

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateAsync_ReturnsResponse_WhenOk()
        {
            // arrange
            _repo.Setup(r => r.ExistsByJmbgAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

            _repo.Setup(r => r.CreateAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_id);

            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_teacher);

            // act
            TeacherResponse resp = await _svc.CreateAsync(_req, It.IsAny<CancellationToken>());

            // assert (state)
            resp.Should().NotBeNull();
            resp.Id.Should().Be(_id);
            resp.FirstName.Should().Be(_teacher.FirstName);
            resp.LastName.Should().Be(_teacher.LastName);
            resp.Title.Should().Be(_teacher.Title);

            // assert (behavior)
            _repo.Verify(r => r.CreateAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ThrowsConflict_WhenJmbgExists()
        {
            _repo.Setup(r => r.ExistsByJmbgAsync(_req.JMBG, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            var act = () => _svc.CreateAsync(_req);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.Conflict)
                .WithMessage("Teacher with this JMBG already exists.");

            _repo.Verify(r => r.CreateAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ThrowsUnexpected_WhenNotFoundAfterCreate()
        {
            _repo.Setup(r => r.ExistsByJmbgAsync(_req.JMBG, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
            _repo.Setup(r => r.CreateAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_id);
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Teacher?)null);

            var act = () => _svc.CreateAsync(_req);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.Unexpected)
                .WithMessage("Unexpected error in creating.");

            _repo.Verify(r => r.CreateAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ChangesProvidedFields_WhenOk()
        {
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_teacher);

            Teacher? captured = null;
            _repo.Setup(r => r.UpdateAsync(It.IsAny<Teacher>(), CancellationToken.None))
                   .Callback<Teacher, CancellationToken>((t, _) => captured = t)
                   .Returns(Task.CompletedTask);

            UpdateTeacherRequest req = new UpdateTeacherRequest
            {
                Title = Title.ProfessorEmeritus
            };

            await _svc.UpdateAsync(_id, req);

            captured.Should().NotBeNull();
            captured.ID.Should().Be(_teacher.ID);
            captured.FirstName.Should().Be(_teacher.FirstName);
            captured.LastName.Should().Be(_teacher.LastName);
            captured.JMBG.Should().Be(_teacher.JMBG);
            captured.Title.Should().Be(Title.ProfessorEmeritus);

            _repo.Verify(r=>r.UpdateAsync(It.IsAny<Teacher>(),default),Times.Once);
        }    

        [Fact]
        public async Task UpdateAsync_ThrowsNotFound_WhenMissing()
        {
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Teacher?)null);

            var act = () => _svc.UpdateAsync(_id, new UpdateTeacherRequest(), CancellationToken.None);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.NotFound)
                .WithMessage("Teacher with id 26 not found.");

            _repo.Verify(r => r.UpdateAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Deletes_WhenFound()
        {
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_teacher);
            _repo.Setup(r => r.DeleteAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _svc.DeleteAsync(_id);

            _repo.Verify(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()), Times.Once);

            _repo.Verify(r => r.DeleteAsync(
                It.Is<Teacher>(t => t.ID == _id),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsNotFound_WhenMissing()
        {
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Teacher?)null);

            var act = () => _svc.DeleteAsync(_id, CancellationToken.None);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.NotFound)
                .WithMessage("Teacher with id 26 not found.");

            _repo.Verify(r => r.DeleteAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ListAsync_ReturnsMappedList()
        {
            var list = new[]
            {
                new Teacher { ID = 1, FirstName = "A", LastName = "B", Title = Title.AssistantProfessor },
                new Teacher { ID = 2, FirstName = "C", LastName = "D", Title = Title.AssociateProfessor }
            };
            _repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list);

            var resp = await _svc.ListAsync();

            var expected = list.Select(e => new { e.FirstName, e.LastName ,e.Title}).ToArray();
            var actual = resp.Select(r => new { r.FirstName, r.LastName, r.Title }).ToArray();

            expected.Should().BeEquivalentTo(actual,opts=>opts.WithoutStrictOrdering());   

            _repo.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ListExamsAsExaminerAsync_ThrowsNotFound_WhenTeacherMissing()
        {
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Teacher?)null);

            var act = () => _svc.ListExamsAsExaminerAsync(_id, CancellationToken.None);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == AppErrorCode.NotFound)
                .WithMessage("Teacher with id 26 not found.");

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.ListExamsAsExaminerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ListExamsAsExaminerAsync_ReturnsMappedList()
        {
            _repo.Setup(r => r.GetByIdAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_teacher);

            var exams = new[]
            {
                new Exam { StudentID = 1, SubjectID = 2, ExaminerID = 3, Grade = 8, Date = new DateOnly(2025, 10, 5) },
                new Exam { StudentID = 4, SubjectID = 5, ExaminerID = 3, Grade = 9, Date = new DateOnly(2025, 9, 20) }
            };
            _repo.Setup(r => r.ListExamsAsExaminerAsync(_id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(exams);

            var result = await _svc.ListExamsAsExaminerAsync(_id, CancellationToken.None);

            var expected = exams.Select(e => new { e.Grade }).ToArray();
            var actual = result.Select(r => new { r.Grade }).ToArray();

            expected.Should().BeEquivalentTo(actual,op=>op.WithoutStrictOrdering());   

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.ListExamsAsExaminerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }
    }
}
