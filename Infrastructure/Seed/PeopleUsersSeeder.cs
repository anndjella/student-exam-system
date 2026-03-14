using Application.Auth;
using Domain.Common;
using Domain.Entity;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seed
{
    public sealed class PeopleUsersSeeder
    {
        private readonly AppDbContext _db;

        public PeopleUsersSeeder(AppDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            if (await _db.Users.AnyAsync(ct) || await _db.Students.AnyAsync(ct) || await _db.Teachers.AnyAsync(ct))
                return;

            var usedJmbg = new HashSet<string>(
                await _db.People.AsNoTracking().Select(p => p.JMBG).ToListAsync(ct));

            var usedUsernames = new HashSet<string>(
                await _db.Users.AsNoTracking().Select(u => u.Username).ToListAsync(ct),
                StringComparer.OrdinalIgnoreCase);

            var usedStudentIndexes = new HashSet<string>(
                await _db.Students.AsNoTracking().Select(s => s.IndexNumber).ToListAsync(ct),
                StringComparer.OrdinalIgnoreCase);

            var usedEmployeeNumbers = new HashSet<string>(
                await _db.Teachers.AsNoTracking().Select(t => t.EmployeeNumber).ToListAsync(ct),
                StringComparer.OrdinalIgnoreCase);

            var students = new List<Student>();
            var teachers = new List<Teacher>();
            var users = new List<User>();

            for (int i = 0; i < 120; i++)
            {
                var first = FirstNames[Random.Shared.Next(FirstNames.Length)];
                var last = LastNames[Random.Shared.Next(LastNames.Length)];

                var dob = RandomDob(2000, 2006);
                var jmbg = GenerateJmbg(dob, usedJmbg);
                var index = GenerateUniqueStudentIndex(usedStudentIndexes);

                var student = new Student
                {
                    FirstName = first,
                    LastName = last,
                    JMBG = jmbg,
                    DateOfBirth = dob,
                    IndexNumber = index
                };

                students.Add(student);
            }

            for (int i = 0; i < 20; i++)
            {
                var first = FirstNames[Random.Shared.Next(FirstNames.Length)];
                var last = LastNames[Random.Shared.Next(LastNames.Length)];

                var dob = RandomDob(1970, 1990);
                var jmbg = GenerateJmbg(dob, usedJmbg);
                var emp = GenerateUniqueEmployeeNumber(usedEmployeeNumbers);

                var teacher = new Teacher
                {
                    FirstName = first,
                    LastName = last,
                    JMBG = jmbg,
                    DateOfBirth = dob,
                    EmployeeNumber = emp,
                    Title = (Title)Random.Shared.Next(1, 5)
                };

                teachers.Add(teacher);
            }

            _db.Students.AddRange(students);
            _db.Teachers.AddRange(teachers);
            await _db.SaveChangesAsync(ct);

            foreach (var s in students)
            {
                var username = CredentialsGenerator.StudentUsername(
                    s.FirstName, s.LastName, s.IndexNumber);

                username = EnsureUniqueUsername(username, usedUsernames);

                var plainPassword = CredentialsGenerator.InitialPasswordPlain(s.JMBG);

                var user = new User(UserRole.Student, username, "", s.ID);
                user.SetPasswordHash(PasswordService.Hash(user, plainPassword));

                users.Add(user);
            }

            foreach (var t in teachers)
            {
                var username = CredentialsGenerator.TeacherUsername(
                    t.FirstName, t.LastName, t.EmployeeNumber);

                username = EnsureUniqueUsername(username, usedUsernames);

                var plainPassword = CredentialsGenerator.InitialPasswordPlain(t.JMBG);

                var user = new User(UserRole.Teacher, username, "", t.ID);
                user.SetPasswordHash(PasswordService.Hash(user, plainPassword));

                users.Add(user);
            }

            _db.Users.AddRange(users);
            await _db.SaveChangesAsync(ct);
        }

        private static string GenerateUniqueStudentIndex(HashSet<string> used)
        {
            while (true)
            {
                var year = Random.Shared.Next(2019, 2026);
                var serial = Random.Shared.Next(1, 9999);
                var value = $"{year}/{serial:0000}";

                if (used.Add(value))
                    return value;
            }
        }

        private static string GenerateUniqueEmployeeNumber(HashSet<string> used)
        {
            while (true)
            {
                var year = Random.Shared.Next(1995, 2026);
                var serial = Random.Shared.Next(1, 9999);
                var value = $"{year}/{serial:0000}";

                if (used.Add(value))
                    return value;
            }
        }

        private static string EnsureUniqueUsername(string baseName, HashSet<string> used)
        {
            if (baseName.Length > 20)
                baseName = baseName.Substring(0, 20);

            if (used.Add(baseName))
                return baseName;

            int suffix = 2;

            while (true)
            {
                var suffixText = suffix.ToString();
                var trimmed = baseName;

                if (trimmed.Length + suffixText.Length > 20)
                    trimmed = trimmed.Substring(0, 20 - suffixText.Length);

                var candidate = trimmed + suffixText;

                if (used.Add(candidate))
                    return candidate;

                suffix++;
            }
        }

        private static DateOnly RandomDob(int fromYearInclusive, int toYearExclusive)
        {
            int year = Random.Shared.Next(fromYearInclusive, toYearExclusive);
            int month = Random.Shared.Next(1, 13);
            int day = Random.Shared.Next(1, DateTime.DaysInMonth(year, month) + 1);
            return new DateOnly(year, month, day);
        }

        private static string GenerateJmbg(DateOnly dob, HashSet<string> used)
        {
            while (true)
            {
                int rr = Random.Shared.Next(70, 100);
                int bbb = Random.Shared.Next(0, 1000);

                int yyy = dob.Year >= 2000 ? dob.Year - 2000 : dob.Year - 1000;

                string first12 = $"{dob.Day:00}{dob.Month:00}{yyy:000}{rr:00}{bbb:000}";
                int k = Checksum(first12);
                string jmbg = first12 + k;

                if (used.Add(jmbg))
                    return jmbg;
            }
        }

        private static int Checksum(string first12)
        {
            var d = first12.Select(c => c - '0').ToArray();

            int m = 11 - (7 * (d[0] + d[6]) +
                           6 * (d[1] + d[7]) +
                           5 * (d[2] + d[8]) +
                           4 * (d[3] + d[9]) +
                           3 * (d[4] + d[10]) +
                           2 * (d[5] + d[11])) % 11;

            return m >= 1 && m <= 9 ? m : 0;
        }

        private static readonly string[] FirstNames =
        {
            "Marko","Nikola","Stefan","Milan","Petar","Filip",
            "Ana","Jovana","Minja","Teodora","Tatjana","Marija"
        };

        private static readonly string[] LastNames =
        {
            "Jović","Petrović","Nikolić","Stanić","Marković",
            "Ilić","Pavlović","Matić","Ristić","Savić"
        };
    }
}