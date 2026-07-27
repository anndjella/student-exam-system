using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Tests.TestDoubles;

namespace Tests.Infrastructure;

public sealed class ProductionDemoSeederTests
{
    private static readonly DateOnly Today = new(2026, 7, 27);

    [Fact]
    public async Task SeedAsync_CreatesCompleteConsistentDemoDataset_AndIsIdempotent()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using (connection)
        await using (db)
        {
            var seeder = new ProductionDemoSeeder(db);

            var first = await seeder.SeedAsync(Today, "Demo-password-2026!");

            first.WasCreated.Should().BeTrue();
            first.StudentServices.Should().Be(5);
            first.Students.Should().Be(100);
            first.Teachers.Should().Be(50);
            first.Subjects.Should().Be(30);
            first.Terms.Should().Be(13);
            first.TeachingAssignments.Should().Be(90);
            first.Enrollments.Should().Be(1_500);
            first.Registrations.Should().BeGreaterThan(1_000);
            first.Exams.Should().BeGreaterThan(800);

            (await db.Users.CountAsync(user => user.Role == UserRole.StudentService))
                .Should().Be(ProductionDemoSeeder.StudentServiceCount);
            (await db.Users.CountAsync(user => user.Role == UserRole.Student))
                .Should().Be(ProductionDemoSeeder.StudentCount);
            (await db.Users.CountAsync(user => user.Role == UserRole.Teacher))
                .Should().Be(ProductionDemoSeeder.TeacherCount);
            (await db.Users.AllAsync(user => user.MustChangePassword))
                .Should().BeTrue();

            var subjectsWithoutGrader = await db.Subjects
                .Where(subject => !db.TeachingAssignments.Any(assignment =>
                    assignment.SubjectID == subject.ID && assignment.CanGrade))
                .CountAsync();
            subjectsWithoutGrader.Should().Be(0);

            var orderedTerms = await db.Terms
                .AsNoTracking()
                .OrderBy(term => term.StartDate)
                .ToListAsync();
            orderedTerms.Select(term => term.Name).Should().Equal(
                "June 24/25",
                "July 24/25",
                "September 24/25",
                "October 24/25",
                "January 25/26",
                "February 25/26",
                "March-April 25/26",
                "June 25/26",
                "July 25/26",
                "September 25/26",
                "October 25/26",
                "January 26/27",
                "February 26/27");
            orderedTerms.Select(term => term.StartDate).Should().Equal(
                new DateOnly(2025, 6, 9),
                new DateOnly(2025, 7, 7),
                new DateOnly(2025, 9, 1),
                new DateOnly(2025, 10, 1),
                new DateOnly(2026, 1, 12),
                new DateOnly(2026, 2, 9),
                new DateOnly(2026, 3, 23),
                new DateOnly(2026, 6, 8),
                new DateOnly(2026, 7, 24),
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 10, 1),
                new DateOnly(2027, 1, 11),
                new DateOnly(2027, 2, 8));
            orderedTerms.Should().OnlyContain(term =>
                term.RegistrationStartDate < term.RegistrationEndDate &&
                term.RegistrationEndDate < term.StartDate &&
                term.StartDate < term.EndDate);
            for (var index = 1; index < orderedTerms.Count; index++)
                orderedTerms[index].StartDate.Should().BeAfter(orderedTerms[index - 1].EndDate);

            var registrations = await db.Registrations
                .AsNoTracking()
                .Include(registration => registration.Term)
                .Include(registration => registration.Subject)
                .ToListAsync();
            registrations.Should().OnlyContain(registration =>
                DateOnly.FromDateTime(registration.RegisteredAt) >=
                    registration.Term.RegistrationStartDate &&
                DateOnly.FromDateTime(registration.RegisteredAt) <=
                    registration.Term.RegistrationEndDate);
            registrations
                .Where(registration => registration.IsActive)
                .Should().OnlyContain(registration => registration.Subject.IsActive);

            var exams = await db.Exams
                .AsNoTracking()
                .Include(exam => exam.Registration)
                .ThenInclude(registration => registration.Term)
                .ToListAsync();

            exams.Should().OnlyContain(exam =>
                exam.Date >= exam.Registration.Term.StartDate &&
                exam.Date <= exam.Registration.Term.EndDate);
            exams.Should().OnlyContain(exam =>
                exam.SignedAt == null
                    ? exam.Registration.IsActive
                    : !exam.Registration.IsActive);
            exams.Should().OnlyContain(exam => exam.Note == null);
            foreach (var exam in exams)
            {
                var graderIsAssigned = await db.TeachingAssignments.AnyAsync(assignment =>
                    assignment.SubjectID == exam.SubjectID &&
                    assignment.TeacherID == exam.TeacherID &&
                    assignment.CanGrade);
                graderIsAssigned.Should().BeTrue();
            }

            var passedEnrollments = await db.Enrollments
                .Where(enrollment => enrollment.IsPassed)
                .ToListAsync();
            passedEnrollments.Should().NotBeEmpty();
            passedEnrollments.Should().OnlyContain(enrollment => enrollment.PassedAt != null);

            foreach (var enrollment in passedEnrollments)
            {
                var hasSignedPassingExam = await db.Exams.AnyAsync(exam =>
                    exam.StudentID == enrollment.StudentID &&
                    exam.SubjectID == enrollment.SubjectID &&
                    exam.Grade >= 6 &&
                    exam.SignedAt != null);
                hasSignedPassingExam.Should().BeTrue();

                var hasLaterRegistration = await db.Registrations.AnyAsync(registration =>
                    registration.StudentID == enrollment.StudentID &&
                    registration.SubjectID == enrollment.SubjectID &&
                    registration.RegisteredAt > enrollment.PassedAt);
                hasLaterRegistration.Should().BeFalse();
            }

            var candidateReader = new NotificationCandidateReader(db);
            var registrationCandidates =
                await candidateReader.ListRegistrationRemindersAsync(Today.AddDays(1));
            var missingResultCandidates =
                await candidateReader.ListMissingExamResultsAsync(Today.AddDays(-30));

            registrationCandidates.Should().NotBeEmpty();
            missingResultCandidates.Should().ContainSingle(candidate =>
                candidate.MissingResultCount == 5);

            var second = await seeder.SeedAsync(Today, initialPassword: null);

            second.WasCreated.Should().BeFalse();
            second.Should().BeEquivalentTo(first, options =>
                options.Excluding(result => result.WasCreated));
        }
    }

    [Fact]
    public async Task SeedAsync_RejectsNonEmptyDatabaseWithoutDemoMarker()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using (connection)
        await using (db)
        {
            db.Subjects.Add(new()
            {
                Code = "EXISTING",
                Name = "Existing Subject",
                ECTS = 6,
                IsActive = true
            });
            await db.SaveChangesAsync();

            var action = () => new ProductionDemoSeeder(db)
                .SeedAsync(Today, "Demo-password-2026!");

            await action.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*empty academic database*");
        }
    }
}
