using Api.Controllers;
using Application.DTO.Students;
using Application.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Application
{
    public class StudentControllerTests
    {
        private readonly Mock<IStudentService> _svc;
        private readonly StudentController _ctrl;
        public StudentControllerTests()
        {
            _svc = new Mock<IStudentService>();
            _ctrl = new StudentController(_svc.Object);

        }
        [Fact]
        public async Task Create_WhenServiceReturnsResponse_Returns201()
        {
            // arrange
            CreateStudentRequest req = new CreateStudentRequest
            {
                JMBG = "0101990123456",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1990, 1, 1),
                IndexNumber = "2024/5"
            };
            StudentResponse resp = new StudentResponse { Id = 123, FirstName = "Ana", LastName = "Anić", IndexNumber = "2024/5" };

            _svc.Setup(s => s.CreateAsync(req, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resp);

            // act
            var result = await _ctrl.Create(req, CancellationToken.None);

            // assert
            var created = result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(StudentController.GetOne));
            created.RouteValues!["id"].Should().Be(123);
            created.Value.Should().Be(resp);
            created.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsInvalidOperation_Returns409()
        {
            // arrange
            CreateStudentRequest req = new CreateStudentRequest
            {
                JMBG = "0101990123456",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1990, 1, 1),
                IndexNumber = "2024/5"
            };

            _svc.Setup(s => s.CreateAsync(req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Index already exists."));

            // act
            var result = await _ctrl.Create(req, CancellationToken.None);

            // assert
            var conflict = result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.StatusCode.Should().Be(409);
            conflict.Value.Should().Be("Index already exists.");
        }
        [Fact]
        public async Task Create_WhenServiceThrowsArgumentException_Returns400()
        {
            // arrange
            CreateStudentRequest req = new CreateStudentRequest
            {
                JMBG = "bad",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1990, 1, 1),
                IndexNumber = "2024/5"
            };

            _svc.Setup(s => s.CreateAsync(req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Invalid JMBG."));

            // act
            var result = await _ctrl.Create(req, CancellationToken.None);

            // assert
            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.StatusCode.Should().Be(400);
            bad.Value.Should().Be("Invalid JMBG.");
        }

        [Fact]
        public async Task GetOne_WhenServiceReturnsStudent_Returns200WithBody()
        {
            StudentResponse resp = new StudentResponse
            {
                Id = 7,
                FirstName = "Ana",
                LastName = "Anić",
                IndexNumber = "2024/5"
            }
            ;
            _svc.Setup(s => s.GetAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resp);

            var result = await _ctrl.GetOne(7, CancellationToken.None);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);
            ok.Value.Should().Be(resp);
        }
        [Fact]
        public async Task GetOne_WhenServiceReturnsNull_Returns404()
        {
            _svc.Setup(s => s.GetAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StudentResponse?)null);

            var result = await _ctrl.GetOne(99, CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }
        [Fact]
        public async Task Update_WhenServiceCompletes_Returns204()
        {
            UpdateStudentRequest req = new UpdateStudentRequest 
            { 
                FirstName = "Ana" 
            };
            _svc.Setup(s => s.UpdateAsync(5, req, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _ctrl.Update(5, req, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }
        [Fact]
        public async Task Update_WhenServiceThrowsNotFound_Returns404()
        {
            UpdateStudentRequest req = new UpdateStudentRequest
            {
                FirstName = "Ana" 
            };
            _svc.Setup(s => s.UpdateAsync(999, req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _ctrl.Update(999, req, CancellationToken.None);

            var notFound = result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be(404);
            notFound.Value.Should().Be($"Person with id 999 does not exist.");
        }
        [Fact]
        public async Task Update_WhenServiceThrowsConflict_Returns409()
        {
            UpdateStudentRequest req = new UpdateStudentRequest
            { 
                IndexNumber = "2024/5"
            };
            _svc.Setup(s => s.UpdateAsync(5, req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Index already exists."));

            var result = await _ctrl.Update(5, req, CancellationToken.None);

            var conflict = result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.StatusCode.Should().Be(409);
            conflict.Value.Should().Be("Index already exists.");
        }
        [Fact]
        public async Task Delete_Always_Returns204()
        {
            _svc.Setup(s => s.DeleteAsync(7, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _ctrl.Delete(7, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_WhenServiceThrows_ExceptionBubbles()
        {
            _svc.Setup(s => s.DeleteAsync(7, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var act = async () => await _ctrl.Delete(7, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("boom");
        }

    }
}
