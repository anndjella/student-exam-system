using Application.Auth;
using Domain.Common;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using StudentExam.DbSeeder;
using Tests.TestDoubles;

namespace Tests.Seed;

public sealed class DemoDataSeederTests
{
    private static readonly DateOnly Today = new(2026, 8, 26);

    private static readonly string[] ExpectedTermNames =
    [
        "January 24/25", "February 24/25", "March-April 24/25", "June 24/25", "July 24/25",
        "September 24/25", "October 24/25", "January 25/26", "February 25/26",
        "March-April 25/26", "June 25/26", "July 25/26", "September 25/26", "October 25/26",
    ];

    [Fact]
    public async Task Seed_creates_consistent_dataset_that_is_idempotent()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using (connection)
        await using (db)
        {
            var seeder = new DemoDataSeeder(db, Today);

            var first = await seeder.RunAsync(commit: true, default);

            first.WasCreated.Should().BeTrue();
            first.StudentServices.Should().Be(10);
            first.Teachers.Should().Be(50);
            first.Students.Should().Be(200);
            first.Subjects.Should().Be(60);
            first.Terms.Should().Be(14);
            first.Enrollments.Should().BeGreaterThan(1200);
            first.Exams.Should().BeGreaterThan(500);
            first.SignedExams.Should().BeGreaterThan(0);
            first.UnsignedExams.Should().BeGreaterThan(0);
            first.CancelledRegistrations.Should().BeGreaterThan(0);
            first.PassedEnrollments.Should().BeGreaterThan(0);

            (await db.Users.CountAsync(u => u.Role == UserRole.Student)).Should().Be(200);
            (await db.Users.AllAsync(u => u.MustChangePassword)).Should().BeTrue();

            var users = await db.Users.AsNoTracking().Include(u => u.Person).ToListAsync();
            users.Should().OnlyContain(u => PasswordService.Verify(
                u, CredentialsGenerator.InitialPasswordPlain(u.Person.JMBG)));

            (await db.Subjects.CountAsync(s => !db.TeachingAssignments
                .Any(a => a.SubjectID == s.ID && a.CanGrade))).Should().Be(0);

            var terms = await db.Terms.AsNoTracking().OrderBy(t => t.StartDate).ToListAsync();
            terms.Select(t => t.Name).Should().Equal(ExpectedTermNames);
            terms.Should().OnlyContain(t =>
                t.RegistrationStartDate < t.RegistrationEndDate &&
                t.RegistrationEndDate < t.StartDate &&
                t.StartDate < t.EndDate);
            for (var i = 1; i < terms.Count; i++)
                terms[i].StartDate.Should().BeAfter(terms[i - 1].EndDate);

            var exams = await db.Exams.AsNoTracking().Include(e => e.Registration).ToListAsync();
            exams.Should().OnlyContain(e =>
                e.SignedAt == null ? e.Registration.IsActive : !e.Registration.IsActive);
            foreach (var exam in exams)
            {
                (await db.TeachingAssignments.AnyAsync(a =>
                    a.SubjectID == exam.SubjectID && a.TeacherID == exam.TeacherID && a.CanGrade))
                    .Should().BeTrue();
            }

            var passed = await db.Enrollments.Where(e => e.IsPassed).ToListAsync();
            passed.Should().OnlyContain(e => e.PassedAt != null);
            foreach (var e in passed)
                (await db.Exams.AnyAsync(x => x.StudentID == e.StudentID
                    && x.SubjectID == e.SubjectID && x.Grade >= 6 && x.SignedAt != null))
                    .Should().BeTrue();

            var second = await seeder.RunAsync(commit: true, default);
            second.WasCreated.Should().BeFalse();
            second.Students.Should().Be(first.Students);
            second.Exams.Should().Be(first.Exams);
        }
    }

    [Fact]
    public async Task Seed_places_the_real_email_accounts_into_the_reminder_scenarios()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using (connection)
        await using (db)
        {
            await new DemoDataSeeder(db, Today).RunAsync(commit: true, default);
            var reader = new NotificationCandidateReader(db);

            var registrationCandidates = await reader.ListRegistrationRemindersAsync(Today.AddDays(1));
            registrationCandidates.Should().Contain(c => c.Email == "stankovicandjela53@gmail.com");

            var missingResultCandidates = await reader.ListMissingExamResultsAsync(Today.AddDays(-30));
            missingResultCandidates.Should().Contain(c => c.Email == "milanmima2000@gmail.com");
            missingResultCandidates
                .Where(c => c.Email == "milanmima2000@gmail.com")
                .Sum(c => c.MissingResultCount)
                .Should().BeGreaterThanOrEqualTo(3);
        }
    }

    [Fact]
    public async Task DryRun_rolls_back_and_writes_nothing()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using (connection)
        await using (db)
        {
            var result = await new DemoDataSeeder(db, Today).RunAsync(commit: false, default);

            result.WasCreated.Should().BeTrue();
            result.DryRun.Should().BeTrue();
            result.Students.Should().Be(200);

            db.ChangeTracker.Clear();
            (await db.Users.AnyAsync()).Should().BeFalse();
            (await db.Subjects.AnyAsync()).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Seed_refuses_a_non_empty_database_without_the_marker()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using (connection)
        await using (db)
        {
            db.Subjects.Add(new() { Code = "EXIST", Name = "Existing", ECTS = 6, IsActive = true });
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            var act = () => new DemoDataSeeder(db, Today).RunAsync(commit: true, default);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*non-empty*");
        }
    }
}
