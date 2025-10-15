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
        public void GetAsync_ReturnsNull_WhenStudentDoesNotExist()
        {
            // arrange
            int id = 26;
            _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Student?)null); //student with id=26 does not exists

            // act
            StudentResponse? resp = _svc.GetAsync(id).Result;

            // assert (state)
            Assert.Null(resp);

            // assert (behavior)
            _repo.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public void CreateAsync_ReturnsStudentResponse_WhenCreateIsOK()
        {
            //arrange
            CreateStudentRequest student = new CreateStudentRequest
            {
                JMBG = "0211995701236",
                FirstName = "Milica",
                LastName = "Tosic",
                DateOfBirth = new DateOnly(1995, 11, 2),
                IndexNumber = "2020/1234"
            };
            _repo.Setup(r => r.CreateAsync(It.IsAny<Student>(),It.IsAny<CancellationToken>()))
                .ReturnsAsync(13);
            _repo.Setup(r => r.GetByIdAsync(13, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Student {});
            //act
            var resp = _svc.CreateAsync(student);

            //assert 
            resp.Should().NotBeNull();
            resp.Id.Should().Be(123);
            _repo.Verify(r => r.CreateAsync(
                         It.Is<Student>(s =>
                             s.FirstName == "Milica" &&
                             s.LastName == "Tosic" &&
                             s.JMBG == "0211995701236" &&
                             s.DateOfBirth == new DateOnly(1995, 11, 2) &&
                             s.IndexNumber == "2020/1234"
                         ),
                        It.IsAny<CancellationToken>()),
                        Times.Once);
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
            await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.CreateAsync(req));

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

            //act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.UpdateAsync(11, req));

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
            await _svc.DeleteAsync(999);

            //assert
            _repo.Verify(r => r.DeleteAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
        }

    }
}
