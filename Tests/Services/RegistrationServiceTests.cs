using Application.Common;
using Application.DTO.Registrations;
using Application.ServicesImplementation;
using Domain.Entity;
using FluentAssertions;
using Moq;
using Tests.TestDoubles;

namespace Tests.Services;

public sealed class RegistrationServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsNewActiveRegistration_WhenPreconditionsPass()
    {
        var uow = new UnitOfWorkMock();
        var clock = new TestClock
        {
            Today = new DateOnly(2026, 1, 12),
            UtcNow = new DateTime(2026, 1, 12, 9, 30, 0, DateTimeKind.Utc)
        };
        var service = new RegistrationService(uow.Object, clock);
        var request = new CreateRegistrationRequest { SubjectID = 11, TermID = 3 };
        Registration? added = null;

        SetupOpenRegistrationPreconditions(uow, studentId: 5, request.SubjectID, request.TermID, clock.Today);
        uow.Registrations
            .Setup(x => x.GetAsync(5, request.SubjectID, request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration?)null);
        uow.Registrations
            .Setup(x => x.Add(It.IsAny<Registration>()))
            .Callback<Registration>(registration => added = registration);
        uow.SetupCommit();

        var response = await service.CreateAsync(5, request);

        response.SubjectID.Should().Be(request.SubjectID);
        response.TermID.Should().Be(request.TermID);
        added.Should().NotBeNull();
        added!.StudentID.Should().Be(5);
        added.IsActive.Should().BeTrue();
        added.RegisteredAt.Should().Be(clock.UtcNow);
        added.CancelledAt.Should().BeNull();
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ReactivatesCancelledRegistration_WhenItAlreadyExists()
    {
        var uow = new UnitOfWorkMock();
        var clock = new TestClock
        {
            Today = new DateOnly(2026, 1, 12),
            UtcNow = new DateTime(2026, 1, 12, 9, 30, 0, DateTimeKind.Utc)
        };
        var service = new RegistrationService(uow.Object, clock);
        var request = new CreateRegistrationRequest { SubjectID = 11, TermID = 3 };
        var existing = new Registration
        {
            StudentID = 5,
            SubjectID = request.SubjectID,
            TermID = request.TermID,
            IsActive = false,
            CancelledAt = new DateTime(2026, 1, 11, 9, 0, 0, DateTimeKind.Utc)
        };

        SetupOpenRegistrationPreconditions(uow, 5, request.SubjectID, request.TermID, clock.Today);
        uow.Registrations
            .Setup(x => x.GetAsync(5, request.SubjectID, request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        uow.SetupCommit();

        await service.CreateAsync(5, request);

        existing.IsActive.Should().BeTrue();
        existing.RegisteredAt.Should().Be(clock.UtcNow);
        existing.CancelledAt.Should().BeNull();
        uow.Registrations.Verify(x => x.Add(It.IsAny<Registration>()), Times.Never);
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenRegistrationWindowIsClosed()
    {
        var uow = new UnitOfWorkMock();
        var clock = new TestClock { Today = new DateOnly(2026, 1, 22) };
        var service = new RegistrationService(uow.Object, clock);
        var request = new CreateRegistrationRequest { SubjectID = 11, TermID = 3 };

        uow.Terms
            .Setup(x => x.GetByIdAsync(request.TermID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Term(request.TermID, new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 15)));

        var act = () => service.CreateAsync(5, request);

        await act.Should().ThrowAsync<AppException>()
            .Where(ex => ex.Code == AppErrorCode.Conflict)
            .WithMessage("Registration window is closed.");

        uow.Registrations.Verify(x => x.Add(It.IsAny<Registration>()), Times.Never);
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_DeactivatesActiveRegistration_WhenPreconditionsPass()
    {
        var uow = new UnitOfWorkMock();
        var clock = new TestClock
        {
            Today = new DateOnly(2026, 1, 12),
            UtcNow = new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc)
        };
        var service = new RegistrationService(uow.Object, clock);
        var existing = new Registration { StudentID = 5, SubjectID = 11, TermID = 3, IsActive = true };

        SetupOpenRegistrationPreconditions(uow, 5, 11, 3, clock.Today);
        uow.Registrations
            .Setup(x => x.GetAsync(5, 11, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        uow.SetupCommit();

        await service.CancelAsync(5, 11, 3);

        existing.IsActive.Should().BeFalse();
        existing.CancelledAt.Should().Be(clock.UtcNow);
        uow.Mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetupOpenRegistrationPreconditions(
        UnitOfWorkMock uow,
        int studentId,
        int subjectId,
        int termId,
        DateOnly today)
    {
        uow.Terms
            .Setup(x => x.GetByIdAsync(termId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Term(termId, today.AddDays(-1), today.AddDays(1)));
        uow.Enrollments
            .Setup(x => x.ExistsAsync(studentId, subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        uow.Enrollments
            .Setup(x => x.IsPassedAsync(studentId, subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private static Term Term(int id, DateOnly registrationStart, DateOnly registrationEnd) => new()
    {
        ID = id,
        Name = $"Term {id}",
        RegistrationStartDate = registrationStart,
        RegistrationEndDate = registrationEnd,
        StartDate = registrationStart,
        EndDate = registrationEnd.AddDays(10)
    };
}
