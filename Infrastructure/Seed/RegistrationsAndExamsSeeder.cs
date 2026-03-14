using Bogus;
using Domain.Entity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seed
{
    public sealed class RegistrationsExamsSeeder
    {
        private readonly AppDbContext _db;

        public RegistrationsExamsSeeder(AppDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync(int currentTermId = 2002, CancellationToken ct = default)
        {
            if (await _db.Registrations.AnyAsync(ct) || await _db.Exams.AnyAsync(ct))
                return;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var terms = await _db.Terms
                .AsNoTracking()
                .OrderBy(t => t.StartDate)
                .ToListAsync(ct);

            var enrollments = await _db.Enrollments.ToListAsync(ct);

            var tas = await _db.TeachingAssignments
                .AsNoTracking()
                .Where(x => x.CanGrade)
                .ToListAsync(ct);

            if (terms.Count == 0 || enrollments.Count == 0 || tas.Count == 0)
                return;

            var currentTerm = terms.FirstOrDefault(t => t.ID == currentTermId)
                ?? throw new InvalidOperationException($"Current term (ID={currentTermId}) not found.");

            var oldTerms = terms
                .Where(t => t.ID != currentTerm.ID)
                .OrderBy(t => t.StartDate)
                .ToList();

            var teacherBySubject = tas
                .GroupBy(x => x.SubjectID)
                .ToDictionary(g => g.Key, g => g.First().TeacherID);

            var faker = new Faker("en");
            var rng = new Random(12345);

            var registrations = new List<Registration>();
            var exams = new List<Exam>();

            var regKeys = new HashSet<(int StudentID, int SubjectID, int TermID)>();
            var passedPairs = new HashSet<(int StudentID, int SubjectID)>();

            var eligibleEnrollments = enrollments
                .Where(e => teacherBySubject.ContainsKey(e.SubjectID))
                .ToList();

            foreach (var enrollment in eligibleEnrollments.OrderBy(_ => rng.Next()))
            {
                var pairKey = (enrollment.StudentID, enrollment.SubjectID);

                if (enrollment.IsPassed || passedPairs.Contains(pairKey))
                    continue;

                int attemptsCount = oldTerms.Count == 0
                    ? 0
                    : rng.Next(1, Math.Min(4, oldTerms.Count) + 1);

                var selectedOldTerms = oldTerms
                    .OrderBy(_ => rng.Next())
                    .Take(attemptsCount)
                    .OrderBy(t => t.StartDate)
                    .ToList();

                foreach (var term in selectedOldTerms)
                {
                    if (passedPairs.Contains(pairKey))
                        break;

                    var regKey = (enrollment.StudentID, enrollment.SubjectID, term.ID);
                    if (regKeys.Contains(regKey))
                        continue;

                    var regDate = FindRegistrationDate(term);

                    if (regDate > today)
                        continue;

                    bool cancelled = rng.NextDouble() < 0.08;

                    var regRegisteredAt = ToUtcDateTime(regDate, 10, 0);
                    var regCancelledAt = cancelled
                        ? ToUtcDateTime(regDate, 12, 0)
                        : (DateTime?)null;

                    var reg = new Registration
                    {
                        StudentID = enrollment.StudentID,
                        SubjectID = enrollment.SubjectID,
                        TermID = term.ID,
                        RegisteredAt = regRegisteredAt,
                        IsActive = false,
                        CancelledAt = regCancelledAt
                    };

                    registrations.Add(reg);
                    regKeys.Add(regKey);

                    if (cancelled)
                        continue;

                    if (!TryFindExamDate(term, out var examDate))
                        continue;

                    if (examDate > today)
                        continue;

                    byte? grade = GenerateGrade(rng);

                    if (passedPairs.Contains(pairKey) && grade >= 6)
                        grade = 5;

                    var signedDate = term.EndDate.AddDays(1);

                    if (signedDate > today)
                        signedDate = today;

                    if (signedDate < examDate)
                        signedDate = examDate;

                    var signedAt = ToUtcDateTime(signedDate, 12, 0);

                    exams.Add(new Exam
                    {
                        StudentID = enrollment.StudentID,
                        SubjectID = enrollment.SubjectID,
                        TermID = term.ID,
                        TeacherID = teacherBySubject[enrollment.SubjectID],
                        Grade = grade,
                        Date = examDate,
                        Note = rng.NextDouble() < 0.12
                            ? string.Join(" ", faker.Lorem.Words(rng.Next(2, 4)))
                            : null,
                        SignedAt = signedAt
                    });

                    if (grade is >= 6)
                    {
                        passedPairs.Add(pairKey);
                        enrollment.IsPassed = true;
                        enrollment.PassedAt = signedAt;
                        break;
                    }
                }

                if (!passedPairs.Contains(pairKey) && !enrollment.IsPassed)
                {
                    var currentKey = (enrollment.StudentID, enrollment.SubjectID, currentTerm.ID);

                    if (!regKeys.Contains(currentKey))
                    {
                        var regDate = FindRegistrationDate(currentTerm);

                        if (regDate <= today)
                        {
                            registrations.Add(new Registration
                            {
                                StudentID = enrollment.StudentID,
                                SubjectID = enrollment.SubjectID,
                                TermID = currentTerm.ID,
                                RegisteredAt = ToUtcDateTime(regDate, 10, 0),
                                IsActive = true,
                                CancelledAt = null
                            });

                            regKeys.Add(currentKey);
                        }
                    }
                }
            }

            _db.Registrations.AddRange(registrations);
            await _db.SaveChangesAsync(ct);

            _db.Exams.AddRange(exams);
            await _db.SaveChangesAsync(ct);
        }

        private static DateOnly FindRegistrationDate(Term term)
        {
            for (var d = term.RegistrationStartDate; d <= term.RegistrationEndDate; d = d.AddDays(1))
                return d;

            return term.RegistrationStartDate;
        }

        private static bool TryFindExamDate(Term term, out DateOnly examDate)
        {
            for (var d = term.StartDate; d <= term.EndDate; d = d.AddDays(1))
            {
                if (term.IsInTermWindow(d) && !term.IsInRegistrationWindow(d))
                {
                    examDate = d;
                    return true;
                }
            }

            examDate = default;
            return false;
        }

        private static byte? GenerateGrade(Random rng)
        {
            var p = rng.NextDouble();

            if (p < 0.10) return null;
            if (p < 0.35) return 5;
            return (byte)rng.Next(6, 11);
        }

        private static DateTime ToUtcDateTime(DateOnly date, int hour, int minute)
        {
            return DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(hour, minute)), DateTimeKind.Utc);
        }
    }
}