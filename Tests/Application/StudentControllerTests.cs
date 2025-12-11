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
using Application.Common;

namespace Tests.Application
{
    public class StudentControllerTests
    {
        private readonly Mock<IStudentService> _svc;
        private readonly StudentController _ctrl;
        public StudentControllerTests()
        {
            _svc = new Mock<IStudentService>(MockBehavior.Strict);
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
        public async Task Create_WhenServiceThrowsInvalidOperation_Throws()
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
                .ThrowsAsync(new AppException(AppErrorCode.Conflict, "Index already exists."));

            // act 
            Func<Task> act = () => _ctrl.Create(req, CancellationToken.None);
            
            //assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code==AppErrorCode.Conflict)
                .WithMessage("Index already exists.");

            _svc.Verify(s => s.CreateAsync(req, It.IsAny<CancellationToken>()), Times.Once);
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
            };
            _svc.Setup(s => s.GetAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resp);

            var result = await _ctrl.GetOne(7, CancellationToken.None);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);
            ok.Value.Should().Be(resp);
        }
        [Fact]
        public async Task GetOne_WhenServiceReturnsNull_Throws()
        {
            _svc.Setup(s => s.GetAsync(26, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StudentResponse?)null);

            var result = await _ctrl.GetOne(26, CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }
        [Fact]
        public async Task Update_WhenServiceCompletes_Returns204()
        {
            UpdateStudentRequest req = new UpdateStudentRequest
            {
                FirstName = "Ana"
            };
            _svc.Setup(s => s.UpdateAsync(26, req, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _ctrl.Update(26, req, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }
        [Fact]
        public async Task Update_WhenServiceThrowsNotFound_Throws()
        {
            UpdateStudentRequest req = new UpdateStudentRequest
            {
                FirstName = "Ana"
            };
            _svc.Setup(s => s.UpdateAsync(26, req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AppException(AppErrorCode.NotFound, "Student with id 26 not found."));

            Func<Task> act = () => _ctrl.Update(26, req, CancellationToken.None);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code==AppErrorCode.NotFound)
                .WithMessage("Student with id 26 not found.");

            _svc.Verify(s => s.UpdateAsync(26, req, It.IsAny<CancellationToken>()), Times.Once);
        }  
        [Fact]
        public async Task Delete_Always_Returns204()
        {
            _svc.Setup(s => s.DeleteAsync(26, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _ctrl.Delete(26, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }
        [Fact]
        public async Task Delete_WhenServiceThrows_Throws()
        {
            _svc.Setup(s => s.DeleteAsync(26, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("invalid"));

            var act = async () => await _ctrl.Delete(26, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("invalid");
        }
    }
}
