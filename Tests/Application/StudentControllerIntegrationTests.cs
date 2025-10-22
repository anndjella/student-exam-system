using Application.DTO.Students;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Application
{
    public class StudentControllerIntegrationTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _fx;
        private readonly HttpClient _client;
        public StudentControllerIntegrationTests(ApiFactory fx)
        {
            _fx = fx;
            _client = fx.CreateClient();
        }
        [Fact]
        public async Task Create_WhenValid_Returns201_WithLocation()
        {
            var req = new CreateStudentRequest
            {
                JMBG = "0101000700011",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(2000, 1, 1),
                IndexNumber = "2024/5"
            };
            var resp = new StudentResponse { Id = 123, FirstName = "Ana", LastName = "Anić", IndexNumber = "2024/5" };

            _fx.StudentSvcMock
                .Setup(s => s.CreateAsync(It.IsAny<CreateStudentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resp);

            var http = await _client.PostAsJsonAsync("/api/students", req);

            http.StatusCode.Should().Be(HttpStatusCode.Created);
            http.Headers.Location.Should().NotBeNull();
            http.Headers.Location!.ToString().Should().Contain("/api/students/123");
            var body = await http.Content.ReadFromJsonAsync<StudentResponse>();
            body!.Id.Should().Be(123);
        }
        [Fact]
        public async Task Create_WhenConflict_Returns409_ProblemDetails()
        {
            var req = new CreateStudentRequest
            {
                JMBG = "0101000700011",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(2000, 1, 1),
                IndexNumber = "2024/5"
            };

            _fx.StudentSvcMock
                .Setup(s => s.CreateAsync(It.IsAny<CreateStudentRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AppException(AppErrorCode.Conflict, "Index already exists."));

            var http = await _client.PostAsJsonAsync("/api/students", req);

            http.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var problem = await http.Content.ReadFromJsonAsync<ProblemDetails>();
            problem!.Title.Should().Be("Conflict");
            problem.Detail.Should().Be("Index already exists.");
            problem.Status.Should().Be(409);
        }
        [Fact]
        public async Task Update_WhenNotFound_Returns404_ProblemDetails()
        {
            var req = new UpdateStudentRequest { FirstName = "Ana"};

            _fx.StudentSvcMock
                .Setup(s => s.UpdateAsync(99, It.IsAny<UpdateStudentRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AppException(AppErrorCode.NotFound, "Student with id 999 not found."));

            var http = await _client.PutAsJsonAsync("/api/students/999", req);

            http.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var problem = await http.Content.ReadFromJsonAsync<ProblemDetails>();
            problem?.Title.Should().Be("Resource not found");
            problem?.Detail.Should().Be("Student with id 999 not found.");
            problem?.Status.Should().Be(404);
            _fx.StudentSvcMock.Verify(s => s.UpdateAsync(999, It.IsAny<UpdateStudentRequest>(), It.IsAny<CancellationToken>()), Times.Once);

        }
        [Fact]
        public async Task Delete_WhenOk_Returns204()
        {
            _fx.StudentSvcMock
                .Setup(s => s.DeleteAsync(7, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var http = await _client.DeleteAsync("/api/students/7");

            http.StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await http.Content.ReadAsStringAsync()).Should().BeEmpty();
        }
    }
}

