using Application.DTO.Notifications;
using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public sealed class NotificationCandidateReader : INotificationCandidateReader
{
    private readonly AppDbContext _db;

    public NotificationCandidateReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RegistrationReminderCandidateResponse>> ListRegistrationRemindersAsync(
        DateOnly registrationEndsOn,
        CancellationToken ct = default)
    {
        var rows = await (
            from term in _db.Terms
            where term.RegistrationEndDate == registrationEndsOn
            from enrollment in _db.Enrollments
            where !enrollment.IsPassed
            join subject in _db.Subjects on enrollment.SubjectID equals subject.ID
            join user in _db.Users on enrollment.StudentID equals user.PersonID
            join person in _db.People on user.PersonID equals person.ID
            where subject.IsActive && user.isActive && user.Role == UserRole.Student
            where !_db.Registrations.Any(registration =>
                registration.StudentID == enrollment.StudentID &&
                registration.SubjectID == enrollment.SubjectID &&
                registration.TermID == term.ID &&
                registration.IsActive)
            select new
            {
                UserId = user.ID,
                person.Email,
                RecipientName = person.FirstName + " " + person.LastName,
                TermId = term.ID,
                TermName = term.Name,
                term.RegistrationEndDate,
                SubjectName = subject.Name
            })
            .AsNoTracking()
            .ToListAsync(ct);

        return rows
            .GroupBy(row => new
            {
                row.UserId,
                row.Email,
                row.RecipientName,
                row.TermId,
                row.TermName,
                row.RegistrationEndDate
            })
            .Select(group => new RegistrationReminderCandidateResponse
            {
                UserId = group.Key.UserId,
                Email = group.Key.Email,
                RecipientName = group.Key.RecipientName,
                TermId = group.Key.TermId,
                TermName = group.Key.TermName,
                RegistrationEndDate = group.Key.RegistrationEndDate,
                SubjectNames = group.Select(row => row.SubjectName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .ToList();
    }

    public async Task<IReadOnlyList<MissingExamResultCandidateResponse>> ListMissingExamResultsAsync(
        DateOnly examDate,
        CancellationToken ct = default)
    {
        var rows = await (
            from exam in _db.Exams
            join subject in _db.Subjects on exam.SubjectID equals subject.ID
            join term in _db.Terms on exam.TermID equals term.ID
            join user in _db.Users on exam.TeacherID equals user.PersonID
            join person in _db.People on user.PersonID equals person.ID
            where exam.Date <= examDate
                && exam.Grade == null
                && exam.SignedAt == null
                && user.isActive
                && user.Role == UserRole.Teacher
            select new
            {
                UserId = user.ID,
                TeacherId = exam.TeacherID,
                person.Email,
                RecipientName = person.FirstName + " " + person.LastName,
                SubjectId = subject.ID,
                SubjectName = subject.Name,
                TermId = term.ID,
                TermName = term.Name,
                exam.Date,
                exam.StudentID
            })
            .AsNoTracking()
            .ToListAsync(ct);

        return rows
            .GroupBy(row => new
            {
                row.UserId,
                row.TeacherId,
                row.Email,
                row.RecipientName,
                row.SubjectId,
                row.SubjectName,
                row.TermId,
                row.TermName,
                row.Date
            })
            .Select(group => new MissingExamResultCandidateResponse
            {
                UserId = group.Key.UserId,
                TeacherId = group.Key.TeacherId,
                Email = group.Key.Email,
                RecipientName = group.Key.RecipientName,
                SubjectId = group.Key.SubjectId,
                SubjectName = group.Key.SubjectName,
                TermId = group.Key.TermId,
                TermName = group.Key.TermName,
                ExamDate = group.Key.Date,
                MissingResultCount = group.Select(row => row.StudentID).Distinct().Count()
            })
            .ToList();
    }
}
