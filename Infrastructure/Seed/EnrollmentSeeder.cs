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
    public sealed class EnrollmentSeeder
    {
        private readonly AppDbContext _db;

        public EnrollmentSeeder(AppDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            if (await _db.Enrollments.AnyAsync(ct))
                return;

            var students = await _db.Students
                .AsNoTracking()
                .Select(s => s.ID)
                .ToListAsync(ct);

            var subjects = await _db.Subjects
                .AsNoTracking()
                .Select(s => s.ID)
                .ToListAsync(ct);

            if (students.Count == 0 || subjects.Count == 0)
                return;

            var rng = new Random(1234);

            var enrollments = new List<Enrollment>();

            foreach (var studentId in students)
            {
                int subjectCount = rng.Next(5, Math.Min(9, subjects.Count));

                var selectedSubjects = subjects
                    .OrderBy(_ => rng.Next())
                    .Take(subjectCount);

                foreach (var subjectId in selectedSubjects)
                {
                    enrollments.Add(new Enrollment
                    {
                        StudentID = studentId,
                        SubjectID = subjectId,
                        CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(300, 900)),
                        IsPassed = false,
                        PassedAt = null
                    });
                }
            }

            _db.Enrollments.AddRange(enrollments);
            await _db.SaveChangesAsync(ct);
        }
    }
}
