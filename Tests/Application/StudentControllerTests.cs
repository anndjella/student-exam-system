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
        [Fact]
        public async Task Create_WhenServiceReturnsResponse_Returns201WithLocation()
        {
            // arrange
            var svc = new Mock<IStudentService>();
            var req = new CreateStudentRequest
            {
                JMBG = "0101990123456",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1990, 1, 1),
                IndexNumber = "2024/5"
            };
            var resp = new StudentResponse { Id = 123, FirstName = "Ana", LastName = "Anić", IndexNumber = "2024/5" };

            svc.Setup(s => s.CreateAsync(req, It.IsAny<CancellationToken>())).ReturnsAsync(resp);

            var ctrl = new StudentsController(svc.Object);

            // act
            var result = await ctrl.Create(req, CancellationToken.None);

            // assert
            var created = result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(StudentsController.GetOne));
            created.RouteValues!["id"].Should().Be(123);
            created.Value.Should().Be(resp);
            created.StatusCode.Should().Be(201);
        }
    }
}
