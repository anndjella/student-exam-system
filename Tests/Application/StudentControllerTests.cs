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
        private readonly CreateStudentRequest _req;
        private readonly StudentResponse _resp;
        private readonly int _id;
        private readonly int _age;
        private readonly double _gpa;
        public StudentControllerTests()
        {
            _svc = new Mock<IStudentService>();
            _ctrl = new StudentController(_svc.Object);
            _req = new CreateStudentRequest
            {
                JMBG = "0101990123456",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1990, 1, 1),
                IndexNumber = "2024/5"
            };
            _id = 26;
            _age = 35;
            _gpa = 7.8;
            _resp = Mapper.CreateToStudentResponse(_req, _id, _age, _gpa);
        }
        [Fact]
        public async Task Create_WhenServiceReturnsResponse_Returns201()
        {
            // arrange

            _svc.Setup(s => s.CreateAsync(_req, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_resp);

            // act
            var result = await _ctrl.Create(_req, CancellationToken.None);

            // assert
            var created = result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(StudentController.GetOne));
            created.RouteValues!["id"].Should().Be(_id);
            created.Value.Should().Be(_resp);
            created.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsInvalidOperation_Throws()
        {
            // arrange
            _svc.Setup(s => s.CreateAsync(_req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AppException(AppErrorCode.Conflict, "Index already exists."));

            // act 
            Func<Task> act = () => _ctrl.Create(_req, CancellationToken.None);
            
            //assert
            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code==AppErrorCode.Conflict)
                .WithMessage("Index already exists.");

            _svc.Verify(s => s.CreateAsync(_req, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetOne_WhenServiceReturnsStudent_Returns200WithBody()
        {
            _svc.Setup(s => s.GetAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_resp);

            var result = await _ctrl.GetOne(7, CancellationToken.None);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);
            ok.Value.Should().Be(_resp);
        }
        [Fact]
        public async Task GetOne_WhenServiceReturnsNull_Throws()
        {
            _svc.Setup(s => s.GetAsync(_id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StudentResponse?)null);

            var result = await _ctrl.GetOne(_id, CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }
        [Fact]
        public async Task Update_WhenServiceCompletes_Returns204()
        {
            UpdateStudentRequest req = new UpdateStudentRequest
            {
                FirstName = "Ana"
            };
            _svc.Setup(s => s.UpdateAsync(_id, req, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _ctrl.Update(_id, req, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }
        [Fact]
        public async Task Update_WhenServiceThrowsNotFound_Throws()
        {
            UpdateStudentRequest req = new UpdateStudentRequest
            {
                FirstName = "Ana"
            };
            _svc.Setup(s => s.UpdateAsync(_id, req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AppException(AppErrorCode.NotFound, "Student with id 26 not found."));

            Func<Task> act = () => _ctrl.Update(_id, req, CancellationToken.None);

            await act.Should().ThrowAsync<AppException>()
                .Where(ex=>ex.Code==AppErrorCode.NotFound)
                .WithMessage("Student with id 26 not found.");

            _svc.Verify(s => s.UpdateAsync(_id, req, It.IsAny<CancellationToken>()), Times.Once);
        }  
        [Fact]
        public async Task Delete_Always_Returns204()
        {
            _svc.Setup(s => s.DeleteAsync(_id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _ctrl.Delete(_id, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }
        [Fact]
        public async Task Delete_WhenServiceThrows_Throws()
        {
            _svc.Setup(s => s.DeleteAsync(_id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("invalid"));

            var act = async () => await _ctrl.Delete(_id, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("invalid");
        }
    }
}
