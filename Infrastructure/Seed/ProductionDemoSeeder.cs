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

namespace Infrastructure.Seed;

public sealed class ProductionDemoSeeder
{
    public const string MarkerUsername = "studentservice01";
    public const int StudentServiceCount = 5;
    public const int StudentCount = 100;
    public const int TeacherCount = 50;
    public const int SubjectCount = 30;

    private const int EnrollmentsPerStudent = 15;

    private readonly AppDbContext _db;

    public ProductionDemoSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProductionDemoSeedResult> SeedAsync(
        DateOnly today,
        string? initialPassword,
        CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(user => user.Username == MarkerUsername, ct))
        {
            await UpgradeLegacyTermNamesAsync(today, ct);
            await RemoveLegacyDemoNotesAsync(ct);
            return await ReadResultAsync(wasCreated: false, ct);
        }

        if (string.IsNullOrWhiteSpace(initialPassword) || initialPassword.Length < 12)
        {
            throw new InvalidOperationException(
                "SeedData:DemoInitialPassword must contain at least 12 characters when SeedData:Mode is Demo.");
        }

        if (await ContainsAcademicDataAsync(ct))
        {
            throw new InvalidOperationException(
                "Production demo data can only be installed into an empty academic database. " +
                "The database already contains data and the demo marker account is missing.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var people = CreatePeople();
        _db.People.AddRange(people.StudentServices);
        _db.Students.AddRange(people.Students);
        _db.Teachers.AddRange(people.Teachers);
        await _db.SaveChangesAsync(ct);

        _db.Users.AddRange(CreateUsers(people, initialPassword));

        var subjects = CreateSubjects();
        var terms = CreateTerms(today);
        _db.Subjects.AddRange(subjects);
        _db.Terms.AddRange(terms.All);
        await _db.SaveChangesAsync(ct);

        var teacherBySubject = AddTeachingAssignments(people.Teachers, subjects);
        var enrollments = AddEnrollments(people.Students, subjects, today);
        await _db.SaveChangesAsync(ct);

        AddAcademicHistory(
            enrollments,
            subjects,
            terms,
            teacherBySubject,
            today);
        await _db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
        return await ReadResultAsync(wasCreated: true, ct);
    }

    private async Task<bool> ContainsAcademicDataAsync(CancellationToken ct)
        => await _db.People.IgnoreQueryFilters().AnyAsync(ct)
           || await _db.Users.IgnoreQueryFilters().AnyAsync(ct)
           || await _db.Subjects.AnyAsync(ct)
           || await _db.Terms.AnyAsync(ct)
           || await _db.Enrollments.AnyAsync(ct)
           || await _db.TeachingAssignments.AnyAsync(ct)
           || await _db.Registrations.AnyAsync(ct)
           || await _db.Exams.AnyAsync(ct);

    private static DemoPeople CreatePeople()
    {
        var jmbgSequence = 1;
        var studentServices = new List<Person>(StudentServiceCount);
        var students = new List<Student>(StudentCount);
        var teachers = new List<Teacher>(TeacherCount);

        for (var i = 0; i < StudentServiceCount; i++)
        {
            var dateOfBirth = new DateOnly(1978 + i, 2 + i, 10 + i);
            studentServices.Add(new Person
            {
                FirstName = ServiceFirstNames[i],
                LastName = ServiceLastNames[i],
                Email = $"student.service{i + 1:00}@demo.studentexam.example",
                JMBG = GenerateJmbg(dateOfBirth, jmbgSequence++),
                DateOfBirth = dateOfBirth
            });
        }

        for (var i = 0; i < StudentCount; i++)
        {
            var dateOfBirth = new DateOnly(
                1998 + i % 9,
                1 + i % 12,
                1 + i % 27);
            var entryYear = 2020 + i % 6;

            students.Add(new Student
            {
                FirstName = FirstNames[i % FirstNames.Length],
                LastName = LastNames[(i * 7) % LastNames.Length],
                Email = $"student{i + 1:000}@demo.studentexam.example",
                JMBG = GenerateJmbg(dateOfBirth, jmbgSequence++),
                DateOfBirth = dateOfBirth,
                IndexNumber = $"{entryYear}/{i + 1:0000}"
            });
        }

        for (var i = 0; i < TeacherCount; i++)
        {
            var dateOfBirth = new DateOnly(
                1958 + i % 32,
                1 + i % 12,
                1 + i % 27);
            var employmentYear = 1995 + i % 27;

            teachers.Add(new Teacher
            {
                FirstName = FirstNames[(i * 3 + 2) % FirstNames.Length],
                LastName = LastNames[(i * 5 + 1) % LastNames.Length],
                Email = $"teacher{i + 1:000}@demo.studentexam.example",
                JMBG = GenerateJmbg(dateOfBirth, jmbgSequence++),
                DateOfBirth = dateOfBirth,
                EmployeeNumber = $"{employmentYear}/{i + 1001:0000}",
                Title = (Title)(i % 4 + 1)
            });
        }

        return new DemoPeople(studentServices, students, teachers);
    }

    private static IEnumerable<User> CreateUsers(DemoPeople people, string initialPassword)
    {
        var users = new List<User>(
            people.StudentServices.Count + people.Students.Count + people.Teachers.Count);

        for (var i = 0; i < people.StudentServices.Count; i++)
        {
            users.Add(CreateUser(
                UserRole.StudentService,
                $"studentservice{i + 1:00}",
                people.StudentServices[i].ID,
                initialPassword));
        }

        foreach (var student in people.Students)
        {
            users.Add(CreateUser(
                UserRole.Student,
                CredentialsGenerator.StudentUsername(
                    student.FirstName,
                    student.LastName,
                    student.IndexNumber),
                student.ID,
                initialPassword));
        }

        foreach (var teacher in people.Teachers)
        {
            users.Add(CreateUser(
                UserRole.Teacher,
                CredentialsGenerator.TeacherUsername(
                    teacher.FirstName,
                    teacher.LastName,
                    teacher.EmployeeNumber),
                teacher.ID,
                initialPassword));
        }

        return users;
    }

    private static User CreateUser(
        UserRole role,
        string username,
        int personId,
        string initialPassword)
    {
        var user = new User(role, username, "TEMP", personId);
        user.SetPasswordHash(PasswordService.Hash(user, initialPassword));
        return user;
    }

    private static List<Subject> CreateSubjects()
        => SubjectDefinitions
            .Select((definition, index) => new Subject
            {
                Code = definition.Code,
                Name = definition.Name,
                ECTS = definition.Ects,
                IsActive = index < SubjectCount - 2
            })
            .ToList();

    private static DemoTerms CreateTerms(DateOnly today)
    {
        var academicYearStart = today.Month >= 9 ? today.Year : today.Year - 1;
        var previousAcademicYear = AcademicYear(academicYearStart - 1);
        var currentAcademicYear = AcademicYear(academicYearStart);
        var nextAcademicYear = AcademicYear(academicYearStart + 1);

        var historical = new List<Term>
        {
            CreateScheduledTerm(
                $"June {previousAcademicYear}",
                new DateOnly(academicYearStart, 6, 9),
                13),
            CreateScheduledTerm(
                $"July {previousAcademicYear}",
                new DateOnly(academicYearStart, 7, 7),
                13),
            CreateScheduledTerm(
                $"September {previousAcademicYear}",
                new DateOnly(academicYearStart, 9, 1),
                13),
            CreateScheduledTerm(
                $"October {previousAcademicYear}",
                new DateOnly(academicYearStart, 10, 1),
                9),
            CreateScheduledTerm(
                $"January {currentAcademicYear}",
                new DateOnly(academicYearStart + 1, 1, 12),
                13),
            CreateScheduledTerm(
                $"February {currentAcademicYear}",
                new DateOnly(academicYearStart + 1, 2, 9),
                13),
            CreateScheduledTerm(
                $"March-April {currentAcademicYear}",
                new DateOnly(academicYearStart + 1, 3, 23),
                13)
        };

        var missingResult = CreateScheduledTerm(
            $"June {currentAcademicYear}",
            new DateOnly(academicYearStart + 1, 6, 8),
            13);

        var current = CreateTerm(
            $"July {currentAcademicYear}",
            today.AddDays(-3),
            today.AddDays(7),
            today.AddDays(-18),
            today.AddDays(-8));

        var openRegistration = CreateTerm(
            $"September {currentAcademicYear}",
            new DateOnly(academicYearStart + 1, 9, 1),
            new DateOnly(academicYearStart + 1, 9, 14),
            today.AddDays(-4),
            today.AddDays(1));

        var future = new List<Term>
        {
            CreateScheduledTerm(
                $"October {currentAcademicYear}",
                new DateOnly(academicYearStart + 1, 10, 1),
                9),
            CreateScheduledTerm(
                $"January {nextAcademicYear}",
                new DateOnly(academicYearStart + 2, 1, 11),
                13),
            CreateScheduledTerm(
                $"February {nextAcademicYear}",
                new DateOnly(academicYearStart + 2, 2, 8),
                13)
        };

        return new DemoTerms(
            [.. historical, missingResult, current, openRegistration, .. future],
            historical,
            missingResult,
            current,
            openRegistration,
            future);
    }

    private async Task UpgradeLegacyTermNamesAsync(DateOnly today, CancellationToken ct)
    {
        var terms = await _db.Terms
            .OrderBy(term => term.StartDate)
            .ThenBy(term => term.ID)
            .ToListAsync(ct);
        var expectedTerms = CreateTerms(today).All;
        var hasLegacyNames = terms.All(term => IsLegacyTermName(term.Name));
        var hasExpectedNames = terms
            .Select(term => term.Name)
            .SequenceEqual(expectedTerms.Select(term => term.Name));

        if (terms.Count != expectedTerms.Count ||
            (!hasLegacyNames && !hasExpectedNames))
        {
            return;
        }

        var requiresDateUpgrade = terms
            .Zip(expectedTerms)
            .Any(pair =>
                pair.First.StartDate != pair.Second.StartDate ||
                pair.First.EndDate != pair.Second.EndDate ||
                pair.First.RegistrationStartDate != pair.Second.RegistrationStartDate ||
                pair.First.RegistrationEndDate != pair.Second.RegistrationEndDate);

        if (!hasLegacyNames && !requiresDateUpgrade)
            return;

        var registrations = await _db.Registrations.ToListAsync(ct);
        var exams = await _db.Exams.ToListAsync(ct);

        for (var index = 0; index < terms.Count; index++)
        {
            var existing = terms[index];
            var expected = expectedTerms[index];
            var termDateDelta = expected.StartDate.DayNumber - existing.StartDate.DayNumber;
            var registrationDateDelta =
                expected.RegistrationStartDate.DayNumber -
                existing.RegistrationStartDate.DayNumber;

            foreach (var registration in registrations.Where(item => item.TermID == existing.ID))
            {
                registration.RegisteredAt = registration.RegisteredAt.AddDays(registrationDateDelta);
                if (registration.CancelledAt is not null)
                    registration.CancelledAt = registration.CancelledAt.Value.AddDays(registrationDateDelta);
            }

            foreach (var exam in exams.Where(item => item.TermID == existing.ID))
            {
                exam.Date = exam.Date.AddDays(termDateDelta);
                if (exam.SignedAt is not null)
                    exam.SignedAt = exam.SignedAt.Value.AddDays(termDateDelta);
            }

            existing.Name = expected.Name;
            existing.StartDate = expected.StartDate;
            existing.EndDate = expected.EndDate;
            existing.RegistrationStartDate = expected.RegistrationStartDate;
            existing.RegistrationEndDate = expected.RegistrationEndDate;
        }

        var passedEnrollments = await _db.Enrollments
            .Where(enrollment => enrollment.IsPassed)
            .ToListAsync(ct);
        foreach (var enrollment in passedEnrollments)
        {
            enrollment.PassedAt = exams
                .Where(exam =>
                    exam.StudentID == enrollment.StudentID &&
                    exam.SubjectID == enrollment.SubjectID &&
                    exam.Grade >= 6 &&
                    exam.SignedAt != null)
                .Select(exam => exam.SignedAt)
                .Max();
        }

        await _db.SaveChangesAsync(ct);
    }

    private static bool IsLegacyTermName(string name)
        => name.StartsWith("Historical Exam Term", StringComparison.Ordinal)
           || name.StartsWith("Late Results Exam Term", StringComparison.Ordinal)
           || name.StartsWith("Current Exam Term", StringComparison.Ordinal)
           || name.StartsWith("Upcoming Exam Term", StringComparison.Ordinal)
           || name.StartsWith("Future Exam Term", StringComparison.Ordinal);

    private static string AcademicYear(int startYear)
        => $"{startYear % 100:00}/{(startYear + 1) % 100:00}";

    private static Term CreateScheduledTerm(
        string name,
        DateOnly start,
        int durationDays)
        => CreateTerm(
            name,
            start,
            start.AddDays(durationDays),
            start.AddDays(-20),
            start.AddDays(-8));

    private static Term CreateTerm(
        string name,
        DateOnly start,
        DateOnly end,
        DateOnly registrationStart,
        DateOnly registrationEnd)
        => new()
        {
            Name = name,
            StartDate = start,
            EndDate = end,
            RegistrationStartDate = registrationStart,
            RegistrationEndDate = registrationEnd
        };

    private Dictionary<int, int> AddTeachingAssignments(
        IReadOnlyList<Teacher> teachers,
        IReadOnlyList<Subject> subjects)
    {
        var primaryTeacherBySubject = new Dictionary<int, int>(subjects.Count);

        for (var subjectIndex = 0; subjectIndex < subjects.Count; subjectIndex++)
        {
            var teacherIndexes = new[]
            {
                subjectIndex % teachers.Count,
                (subjectIndex + 30) % teachers.Count,
                (subjectIndex + 15) % teachers.Count
            }.Distinct().ToArray();

            primaryTeacherBySubject[subjects[subjectIndex].ID] = teachers[teacherIndexes[0]].ID;

            for (var assignmentIndex = 0; assignmentIndex < teacherIndexes.Length; assignmentIndex++)
            {
                _db.TeachingAssignments.Add(new TeachingAssignment
                {
                    SubjectID = subjects[subjectIndex].ID,
                    TeacherID = teachers[teacherIndexes[assignmentIndex]].ID,
                    CanGrade = assignmentIndex == 0 ||
                               (assignmentIndex == 1 && subjectIndex % 4 == 0)
                });
            }
        }

        return primaryTeacherBySubject;
    }

    private List<Enrollment> AddEnrollments(
        IReadOnlyList<Student> students,
        IReadOnlyList<Subject> subjects,
        DateOnly today)
    {
        var enrollments = new List<Enrollment>(students.Count * EnrollmentsPerStudent);

        for (var studentIndex = 0; studentIndex < students.Count; studentIndex++)
        {
            var subjectIndexes = Enumerable.Range(0, EnrollmentsPerStudent)
                .Select(offset => (studentIndex + offset * 2) % subjects.Count)
                .ToHashSet();

            if (studentIndex < 5 && !subjectIndexes.Contains(0))
            {
                subjectIndexes.Remove(subjectIndexes.Max());
                subjectIndexes.Add(0);
            }

            foreach (var subjectIndex in subjectIndexes.Order())
            {
                var enrollment = new Enrollment
                {
                    StudentID = students[studentIndex].ID,
                    SubjectID = subjects[subjectIndex].ID,
                    CreatedAt = ToUtcDateTime(
                        today.AddDays(-900 + (studentIndex + subjectIndex) % 500),
                        9),
                    IsPassed = false,
                    PassedAt = null
                };
                enrollments.Add(enrollment);
                _db.Enrollments.Add(enrollment);
            }
        }

        return enrollments;
    }

    private void AddAcademicHistory(
        IReadOnlyList<Enrollment> enrollments,
        IReadOnlyList<Subject> subjects,
        DemoTerms terms,
        IReadOnlyDictionary<int, int> teacherBySubject,
        DateOnly today)
    {
        var subjectIndexById = subjects
            .Select((subject, index) => new { subject.ID, Index = index })
            .ToDictionary(item => item.ID, item => item.Index);
        var studentOrdinalById = enrollments
            .Select(enrollment => enrollment.StudentID)
            .Distinct()
            .Order()
            .Select((studentId, index) => new { StudentId = studentId, Ordinal = index })
            .ToDictionary(item => item.StudentId, item => item.Ordinal);

        var enrollmentIndex = 0;
        foreach (var enrollment in enrollments)
        {
            var subjectIndex = subjectIndexById[enrollment.SubjectID];
            var studentOrdinal = studentOrdinalById[enrollment.StudentID];

            if (studentOrdinal < 5 && subjectIndex == 0)
                continue;

            var scenario = (studentOrdinal * 17 + subjectIndex * 13 + enrollmentIndex++) % 10;
            if (!subjects[subjectIndex].IsActive && scenario >= 5)
                scenario = scenario % 5;
            if (subjectIndex == 0 && scenario is 5 or 6)
                scenario = 3;

            switch (scenario)
            {
                case 0:
                    AddSignedExam(enrollment, terms.Historical[2], teacherBySubject, 8);
                    break;
                case 1:
                    AddSignedExam(enrollment, terms.Historical[1], teacherBySubject, 5);
                    AddSignedExam(enrollment, terms.Historical[3], teacherBySubject, 9);
                    break;
                case 2:
                    AddSignedExam(enrollment, terms.Historical[2], teacherBySubject, null);
                    AddSignedExam(enrollment, terms.Historical[4], teacherBySubject, 5);
                    break;
                case 3:
                    AddSignedExam(enrollment, terms.Historical[3], teacherBySubject, 5);
                    AddSignedExam(enrollment, terms.Historical[6], teacherBySubject, 5);
                    break;
                case 4:
                    AddCancelledRegistration(enrollment, terms.Historical[5]);
                    break;
                case 5:
                    AddSignedExam(enrollment, terms.Historical[5], teacherBySubject, 5);
                    AddUnsignedExam(
                        enrollment,
                        terms.Current,
                        teacherBySubject,
                        (studentOrdinal + subjectIndex) % 2 == 0 ? (byte)7 : (byte)5,
                        today);
                    break;
                case 6:
                    AddSignedExam(enrollment, terms.Historical[5], teacherBySubject, null);
                    AddActiveRegistration(enrollment, terms.Current);
                    break;
                case 7:
                    AddSignedExam(enrollment, terms.Historical[6], teacherBySubject, 5);
                    AddActiveRegistration(enrollment, terms.OpenRegistration);
                    break;
                case 8:
                    AddCancelledRegistration(enrollment, terms.OpenRegistration);
                    break;
                case 9:
                    break;
            }
        }

        var notificationSubject = subjects[0];
        var notificationTeacherId = teacherBySubject[notificationSubject.ID];
        var targetEnrollments = enrollments
            .Where(enrollment =>
                studentOrdinalById[enrollment.StudentID] < 5 &&
                enrollment.SubjectID == notificationSubject.ID)
            .OrderBy(enrollment => enrollment.StudentID)
            .ToList();

        foreach (var enrollment in targetEnrollments)
        {
            var registration = CreateRegistration(
                enrollment,
                terms.MissingResult,
                isActive: true,
                cancelledAt: null);
            _db.Registrations.Add(registration);
            _db.Exams.Add(new Exam
            {
                StudentID = enrollment.StudentID,
                SubjectID = enrollment.SubjectID,
                TermID = terms.MissingResult.ID,
                TeacherID = notificationTeacherId,
                Grade = null,
                Date = terms.MissingResult.StartDate.AddDays(5),
                Note = null,
                SignedAt = null
            });
        }
    }

    private void AddSignedExam(
        Enrollment enrollment,
        Term term,
        IReadOnlyDictionary<int, int> teacherBySubject,
        byte? grade)
    {
        var signedAt = ToUtcDateTime(term.EndDate.AddDays(1), 12);
        var registration = CreateRegistration(
            enrollment,
            term,
            isActive: false,
            cancelledAt: null);
        _db.Registrations.Add(registration);
        _db.Exams.Add(new Exam
        {
            StudentID = enrollment.StudentID,
            SubjectID = enrollment.SubjectID,
            TermID = term.ID,
            TeacherID = teacherBySubject[enrollment.SubjectID],
            Grade = grade,
            Date = term.StartDate.AddDays(2),
            Note = null,
            SignedAt = signedAt
        });

        if (grade is >= 6)
        {
            enrollment.IsPassed = true;
            enrollment.PassedAt = signedAt;
        }
    }

    private void AddUnsignedExam(
        Enrollment enrollment,
        Term term,
        IReadOnlyDictionary<int, int> teacherBySubject,
        byte grade,
        DateOnly today)
    {
        _db.Registrations.Add(CreateRegistration(
            enrollment,
            term,
            isActive: true,
            cancelledAt: null));
        _db.Exams.Add(new Exam
        {
            StudentID = enrollment.StudentID,
            SubjectID = enrollment.SubjectID,
            TermID = term.ID,
            TeacherID = teacherBySubject[enrollment.SubjectID],
            Grade = grade,
            Date = today,
            Note = null,
            SignedAt = null
        });
    }

    private async Task RemoveLegacyDemoNotesAsync(CancellationToken ct)
    {
        var legacyNotes = new[]
        {
            "Student did not attend the exam.",
            "The required learning outcomes were not demonstrated.",
            "Result entry is intentionally pending for the notification demo.",
            "Result entered and awaiting final lock."
        };
        var exams = await _db.Exams
            .Where(exam => exam.Note != null && legacyNotes.Contains(exam.Note))
            .ToListAsync(ct);

        if (exams.Count == 0)
            return;

        foreach (var exam in exams)
            exam.Note = null;

        await _db.SaveChangesAsync(ct);
    }

    private void AddActiveRegistration(Enrollment enrollment, Term term)
        => _db.Registrations.Add(CreateRegistration(
            enrollment,
            term,
            isActive: true,
            cancelledAt: null));

    private void AddCancelledRegistration(Enrollment enrollment, Term term)
    {
        var cancelledAt = ToUtcDateTime(term.RegistrationStartDate.AddDays(2), 14);
        _db.Registrations.Add(CreateRegistration(
            enrollment,
            term,
            isActive: false,
            cancelledAt));
    }

    private static Registration CreateRegistration(
        Enrollment enrollment,
        Term term,
        bool isActive,
        DateTime? cancelledAt)
        => new()
        {
            StudentID = enrollment.StudentID,
            SubjectID = enrollment.SubjectID,
            TermID = term.ID,
            RegisteredAt = ToUtcDateTime(term.RegistrationStartDate.AddDays(1), 10),
            IsActive = isActive,
            CancelledAt = cancelledAt
        };

    private async Task<ProductionDemoSeedResult> ReadResultAsync(
        bool wasCreated,
        CancellationToken ct)
        => new(
            wasCreated,
            await _db.Users.CountAsync(user => user.Role == UserRole.StudentService, ct),
            await _db.Students.CountAsync(ct),
            await _db.Teachers.CountAsync(ct),
            await _db.Subjects.CountAsync(ct),
            await _db.Terms.CountAsync(ct),
            await _db.TeachingAssignments.CountAsync(ct),
            await _db.Enrollments.CountAsync(ct),
            await _db.Registrations.CountAsync(ct),
            await _db.Exams.CountAsync(ct));

    private static DateTime ToUtcDateTime(DateOnly date, int hour)
        => DateTime.SpecifyKind(
            date.ToDateTime(new TimeOnly(hour, 0)),
            DateTimeKind.Utc);

    private static string GenerateJmbg(DateOnly dateOfBirth, int sequence)
    {
        var yearPart = dateOfBirth.Year >= 2000
            ? dateOfBirth.Year - 2000
            : dateOfBirth.Year - 1000;
        var region = 70 + sequence % 30;
        var serial = sequence % 1000;
        var firstTwelve =
            $"{dateOfBirth.Day:00}{dateOfBirth.Month:00}{yearPart:000}{region:00}{serial:000}";
        return firstTwelve + CalculateJmbgChecksum(firstTwelve);
    }

    private static int CalculateJmbgChecksum(string firstTwelve)
    {
        var digits = firstTwelve.Select(character => character - '0').ToArray();
        var checksum = 11 - (
            7 * (digits[0] + digits[6]) +
            6 * (digits[1] + digits[7]) +
            5 * (digits[2] + digits[8]) +
            4 * (digits[3] + digits[9]) +
            3 * (digits[4] + digits[10]) +
            2 * (digits[5] + digits[11])) % 11;
        return checksum is >= 1 and <= 9 ? checksum : 0;
    }

    private sealed record DemoPeople(
        List<Person> StudentServices,
        List<Student> Students,
        List<Teacher> Teachers);

    private sealed record DemoTerms(
        List<Term> All,
        List<Term> Historical,
        Term MissingResult,
        Term Current,
        Term OpenRegistration,
        List<Term> Future);

    private sealed record SubjectDefinition(string Code, string Name, byte Ects);

    private static readonly string[] ServiceFirstNames =
    [
        "Milena", "Jelena", "Nikola", "Ivana", "Miloš"
    ];

    private static readonly string[] ServiceLastNames =
    [
        "Jovanović", "Petrović", "Marković", "Nikolić", "Stojanović"
    ];

    private static readonly string[] FirstNames =
    [
        "Aleksandar", "Ana", "Bojan", "Danica", "Dejan", "Dragana",
        "Dušan", "Ivana", "Jelena", "Jovan", "Katarina", "Lazar",
        "Marija", "Marko", "Milan", "Milica", "Miloš", "Nemanja",
        "Nikola", "Petar", "Sanja", "Sara", "Stefan", "Tamara"
    ];

    private static readonly string[] LastNames =
    [
        "Ilić", "Janković", "Jovanović", "Kovačević", "Lazić", "Marković",
        "Matić", "Milošević", "Nikolić", "Pavlović", "Petrović", "Popović",
        "Ristić", "Savić", "Simić", "Stanković", "Stojanović", "Todorović",
        "Vasić", "Đorđević"
    ];

    private static readonly SubjectDefinition[] SubjectDefinitions =
    [
        new("CS101", "Introduction to Programming", 8),
        new("CS102", "Object-Oriented Programming", 8),
        new("CS201", "Data Structures and Algorithms", 8),
        new("CS202", "Database Systems", 7),
        new("CS203", "Computer Architecture", 6),
        new("CS204", "Operating Systems", 7),
        new("CS301", "Software Engineering", 8),
        new("CS302", "Web Application Development", 7),
        new("CS303", "Computer Networks", 7),
        new("CS304", "Distributed Systems", 7),
        new("CS305", "Information Security", 6),
        new("CS306", "Cloud Computing", 6),
        new("CS401", "Machine Learning", 8),
        new("CS402", "Artificial Intelligence", 8),
        new("CS403", "Data Mining", 6),
        new("CS404", "Mobile Application Development", 6),
        new("CS405", "Software Testing and Quality Assurance", 6),
        new("CS406", "Human-Computer Interaction", 5),
        new("MATH101", "Calculus I", 8),
        new("MATH102", "Calculus II", 8),
        new("MATH201", "Linear Algebra", 7),
        new("MATH202", "Discrete Mathematics", 7),
        new("MATH301", "Probability and Statistics", 7),
        new("ENG101", "Academic English", 4),
        new("BUS201", "IT Project Management", 5),
        new("BUS301", "Technology Entrepreneurship", 5),
        new("ELEC201", "Digital Electronics", 6),
        new("ELEC301", "Embedded Systems", 7),
        new("LEGACY01", "Legacy Information Systems", 5),
        new("LEGACY02", "Classic Software Platforms", 5)
    ];
}

public sealed record ProductionDemoSeedResult(
    bool WasCreated,
    int StudentServices,
    int Students,
    int Teachers,
    int Subjects,
    int Terms,
    int TeachingAssignments,
    int Enrollments,
    int Registrations,
    int Exams);
