using Domain.Entity;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seed;

public sealed class TeacherNotificationScenarioSeeder
{
    public const int TargetTeacherId = 11011;
    public const string TargetTeacherEmail = "notification.teacher@example.com";
    public const string TestSubjectCode = "NOTIF-TEST";
    public const string TestTermName = "Notification test term";
    public const int MissingResultStudentCount = 5;

    private readonly AppDbContext _db;

    public TeacherNotificationScenarioSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TeacherNotificationScenarioSeedResult> SeedAsync(
        DateOnly today,
        CancellationToken ct = default)
    {
        var teacher = await _db.Teachers
            .SingleOrDefaultAsync(candidate => candidate.ID == TargetTeacherId, ct)
            ?? throw new InvalidOperationException(
                $"Teacher with ID {TargetTeacherId} is required for the notification test scenario.");

        var teacherUserExists = await _db.Users.AnyAsync(user =>
            user.PersonID == TargetTeacherId &&
            user.Role == UserRole.Teacher &&
            user.isActive, ct);

        if (!teacherUserExists)
            throw new InvalidOperationException(
                $"An active teacher user for PersonID {TargetTeacherId} is required for the notification test scenario.");

        if (!string.Equals(teacher.Email, TargetTeacherEmail, StringComparison.OrdinalIgnoreCase))
        {
            teacher.Email = TargetTeacherEmail;
            await _db.SaveChangesAsync(ct);
        }

        var examDate = today.AddDays(-30);

        var subject = await _db.Subjects
            .SingleOrDefaultAsync(candidate => candidate.Code == TestSubjectCode, ct);
        if (subject is null)
        {
            subject = new Subject
            {
                Code = TestSubjectCode,
                Name = "Notification Testing",
                ECTS = 6,
                IsActive = true
            };
            _db.Subjects.Add(subject);
            await _db.SaveChangesAsync(ct);
        }

        var term = await _db.Terms
            .SingleOrDefaultAsync(candidate => candidate.Name == TestTermName, ct);
        if (term is null)
        {
            term = new Term
            {
                Name = TestTermName,
                RegistrationStartDate = examDate.AddDays(-14),
                RegistrationEndDate = examDate.AddDays(-3),
                StartDate = examDate.AddDays(-1),
                EndDate = examDate.AddDays(1)
            };
            _db.Terms.Add(term);
            await _db.SaveChangesAsync(ct);
        }

        var assignment = await _db.TeachingAssignments.FindAsync(
            [subject.ID, TargetTeacherId],
            ct);
        if (assignment is null)
        {
            _db.TeachingAssignments.Add(new TeachingAssignment
            {
                SubjectID = subject.ID,
                TeacherID = TargetTeacherId,
                CanGrade = true
            });
        }
        else if (!assignment.CanGrade)
        {
            assignment.CanGrade = true;
        }

        var studentIds = await (
            from student in _db.Students
            join user in _db.Users on student.ID equals user.PersonID
            where user.Role == UserRole.Student && user.isActive
            orderby student.ID
            select student.ID)
            .Take(MissingResultStudentCount)
            .ToListAsync(ct);

        if (studentIds.Count < MissingResultStudentCount)
            throw new InvalidOperationException(
                $"At least {MissingResultStudentCount} active students are required for the notification test scenario.");

        foreach (var studentId in studentIds)
        {
            var enrollment = await _db.Enrollments.FindAsync([studentId, subject.ID], ct);
            if (enrollment is null)
            {
                _db.Enrollments.Add(new Enrollment
                {
                    StudentID = studentId,
                    SubjectID = subject.ID,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    IsPassed = false,
                    PassedAt = null
                });
            }

            var registration = await _db.Registrations.FindAsync(
                [subject.ID, studentId, term.ID],
                ct);
            if (registration is null)
            {
                _db.Registrations.Add(new Registration
                {
                    StudentID = studentId,
                    SubjectID = subject.ID,
                    TermID = term.ID,
                    RegisteredAt = ToUtcDateTime(term.RegistrationStartDate.AddDays(1)),
                    IsActive = true,
                    CancelledAt = null
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        foreach (var studentId in studentIds)
        {
            var examExists = await _db.Exams.AnyAsync(exam =>
                exam.StudentID == studentId &&
                exam.SubjectID == subject.ID &&
                exam.TermID == term.ID, ct);

            if (!examExists)
            {
                _db.Exams.Add(new Exam
                {
                    StudentID = studentId,
                    SubjectID = subject.ID,
                    TermID = term.ID,
                    TeacherID = TargetTeacherId,
                    Date = examDate,
                    Grade = null,
                    SignedAt = null,
                    Note = "Development scenario for the missing exam result reminder."
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        return new TeacherNotificationScenarioSeedResult(
            TargetTeacherId,
            studentIds,
            subject.ID,
            term.ID,
            examDate,
            TargetTeacherEmail);
    }

    private static DateTime ToUtcDateTime(DateOnly date)
        => DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(10, 0)), DateTimeKind.Utc);
}

public sealed record TeacherNotificationScenarioSeedResult(
    int TeacherId,
    IReadOnlyList<int> StudentIds,
    int SubjectId,
    int TermId,
    DateOnly ExamDate,
    string TeacherEmail);
