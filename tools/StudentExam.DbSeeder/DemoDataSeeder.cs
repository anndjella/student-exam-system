using Application.Auth;
using Domain.Common;
using Domain.Entity;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StudentExam.DbSeeder.Data;

namespace StudentExam.DbSeeder;

/// <summary>
/// Builds the full demo dataset in a single transaction. Deterministic (fixed RNG seed),
/// idempotent (guarded by the student-service marker accounts) and refuses to run against a
/// non-empty database that was not produced by this seeder.
/// </summary>
public sealed class DemoDataSeeder
{
    public const int StudentServiceCount = 10;
    public const int TeacherCount = 50;
    public const int StudentCount = 200;

    private const string MarkerUsername = "studentservice01";
    private const string RealStudentEmail = "stankovicandjela53@gmail.com";
    private const string RealTeacherEmail = "milanmima2000@gmail.com";
    private const int RealStudentIndex = 0;   // index into the students list
    private const int RealTeacherIndex = 0;   // index into the teachers list

    private readonly AppDbContext _db;
    private readonly DateOnly _today;
    private readonly Random _rng = new(20260826);
    private readonly ValueFactories _values = new();
    private readonly Dictionary<string, int> _scenarios = new();
    private readonly HashSet<(int Student, int Subject, int Term)> _examKeys = new();
    private readonly Dictionary<(int Student, int Subject, int Term), Registration> _regByKey = new();
    private readonly HashSet<(int Student, int Subject)> _enrollmentKeys = new();

    public DemoDataSeeder(AppDbContext db, DateOnly today)
    {
        _db = db;
        _today = today;
    }

    public async Task<bool> IsAlreadySeededAsync(CancellationToken ct)
        => await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == MarkerUsername, ct);

    public async Task<bool> ContainsForeignDataAsync(CancellationToken ct)
        => await _db.People.IgnoreQueryFilters().AnyAsync(ct)
           || await _db.Users.IgnoreQueryFilters().AnyAsync(ct)
           || await _db.Subjects.AnyAsync(ct)
           || await _db.Terms.AnyAsync(ct)
           || await _db.Enrollments.AnyAsync(ct)
           || await _db.TeachingAssignments.AnyAsync(ct)
           || await _db.Registrations.AnyAsync(ct)
           || await _db.Exams.AnyAsync(ct);

    /// <param name="commit">false = dry run: everything is inserted then rolled back.</param>
    public async Task<SeedResult> RunAsync(bool commit, CancellationToken ct)
    {
        if (await IsAlreadySeededAsync(ct))
            return await SummariseAsync(wasCreated: false, dryRun: !commit, ct);

        if (await ContainsForeignDataAsync(ct))
            throw new InvalidOperationException(
                "The database already contains data and the demo marker account is missing. " +
                "Refusing to seed a non-empty database. Point the seeder at an empty database.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var calendar = TermCalendar.Resolve(_today);
        _db.Terms.AddRange(calendar.Ordered);

        var subjects = BuildSubjects();
        _db.Subjects.AddRange(subjects);
        await _db.SaveChangesAsync(ct);

        var services = BuildStudentServices();
        var teachers = BuildTeachers();
        var students = BuildStudents();
        _db.AddRange(services);
        _db.Students.AddRange(students);
        _db.Teachers.AddRange(teachers);
        await _db.SaveChangesAsync(ct);

        _db.Users.AddRange(BuildUsers(services, teachers, students));
        await _db.SaveChangesAsync(ct);

        var graderBySubject = BuildTeachingAssignments(subjects, teachers);
        await _db.SaveChangesAsync(ct);

        var enrollments = BuildEnrollments(students, subjects);
        await _db.SaveChangesAsync(ct);

        BuildAcademicHistory(students, subjects, teachers, enrollments, graderBySubject, calendar);
        await _db.SaveChangesAsync(ct);

        var result = await SummariseAsync(wasCreated: true, dryRun: !commit, ct);

        if (commit)
            await tx.CommitAsync(ct);
        else
            await tx.RollbackAsync(ct);

        return result;
    }

    // ----- people -------------------------------------------------------------------------

    private List<Person> BuildStudentServices()
    {
        var list = new List<Person>(StudentServiceCount);
        for (var i = 0; i < StudentServiceCount; i++)
        {
            var dob = new DateOnly(1975 + i % 15, 1 + i % 12, 1 + i % 27);
            list.Add(new Person
            {
                FirstName = SerbianNames.FirstNames[(i * 5 + 1) % SerbianNames.FirstNames.Length],
                LastName = SerbianNames.LastNames[(i * 3 + 2) % SerbianNames.LastNames.Length],
                Email = $"service{i + 1:00}@office.studentexam.example",
                JMBG = _values.NextJmbg(dob),
                DateOfBirth = dob,
            });
        }
        return list;
    }

    private List<Teacher> BuildTeachers()
    {
        var list = new List<Teacher>(TeacherCount);
        for (var i = 0; i < TeacherCount; i++)
        {
            var dob = new DateOnly(1958 + i % 34, 1 + (i * 2) % 12, 1 + (i * 3) % 27);
            var first = SerbianNames.FirstNames[(i * 7 + 3) % SerbianNames.FirstNames.Length];
            var last = SerbianNames.LastNames[(i * 11 + 5) % SerbianNames.LastNames.Length];
            var employeeNumber = _values.NextEmployeeNumber(1996 + i % 28, 1000 + i);
            list.Add(new Teacher
            {
                FirstName = first,
                LastName = last,
                Email = i == RealTeacherIndex
                    ? RealTeacherEmail
                    : $"t{employeeNumber.Replace("/", "")}@staff.studentexam.example",
                JMBG = _values.NextJmbg(dob),
                DateOfBirth = dob,
                EmployeeNumber = employeeNumber,
                Title = (Title)(i % 4 + 1),
            });
        }
        return list;
    }

    private List<Student> BuildStudents()
    {
        var list = new List<Student>(StudentCount);
        for (var i = 0; i < StudentCount; i++)
        {
            var dob = new DateOnly(1998 + i % 9, 1 + (i * 5) % 12, 1 + (i * 7) % 27);
            var first = SerbianNames.FirstNames[(i * 13 + 2) % SerbianNames.FirstNames.Length];
            var last = SerbianNames.LastNames[(i * 17 + 9) % SerbianNames.LastNames.Length];
            var index = _values.NextIndexNumber(2019 + i % 7, 1 + i);
            list.Add(new Student
            {
                FirstName = first,
                LastName = last,
                Email = i == RealStudentIndex
                    ? RealStudentEmail
                    : $"s{index.Replace("/", "")}@students.studentexam.example",
                JMBG = _values.NextJmbg(dob),
                DateOfBirth = dob,
                IndexNumber = index,
            });
        }
        return list;
    }

    private List<User> BuildUsers(
        List<Person> services, List<Teacher> teachers, List<Student> students)
    {
        var users = new List<User>(services.Count + teachers.Count + students.Count);

        for (var i = 0; i < services.Count; i++)
            users.Add(NewUser(UserRole.StudentService, _values.ServiceUsername(i + 1), services[i]));

        foreach (var t in teachers)
            users.Add(NewUser(UserRole.Teacher,
                _values.TeacherUsername(t.FirstName, t.LastName, t.EmployeeNumber), t));

        foreach (var s in students)
            users.Add(NewUser(UserRole.Student,
                _values.StudentUsername(s.FirstName, s.LastName, s.IndexNumber), s));

        return users;
    }

    private static User NewUser(UserRole role, string username, Person person)
    {
        var user = new User(role, username, "TEMP", person.ID);
        user.SetPasswordHash(PasswordService.Hash(user, CredentialsGenerator.InitialPasswordPlain(person.JMBG)));
        return user;
    }

    // ----- catalog ----------------------------------------------------------------------

    private static List<Subject> BuildSubjects()
        => SubjectCatalog.All
            .Select(d => new Subject { Code = d.Code, Name = d.Name, ECTS = d.Ects, IsActive = d.IsActive })
            .ToList();

    /// <summary>Every subject gets 1-3 teaching assignments and at least one grader.</summary>
    private Dictionary<int, int> BuildTeachingAssignments(List<Subject> subjects, List<Teacher> teachers)
    {
        var graderBySubject = new Dictionary<int, int>(subjects.Count);
        for (var i = 0; i < subjects.Count; i++)
        {
            var primary = teachers[i % teachers.Count];
            var secondary = teachers[(i * 3 + 7) % teachers.Count];
            var tertiary = teachers[(i * 5 + 13) % teachers.Count];

            var assigned = new HashSet<int> { primary.ID };
            _db.TeachingAssignments.Add(new TeachingAssignment
            { SubjectID = subjects[i].ID, TeacherID = primary.ID, CanGrade = true });

            if (assigned.Add(secondary.ID))
                _db.TeachingAssignments.Add(new TeachingAssignment
                { SubjectID = subjects[i].ID, TeacherID = secondary.ID, CanGrade = i % 3 == 0 });

            if (i % 2 == 0 && assigned.Add(tertiary.ID))
                _db.TeachingAssignments.Add(new TeachingAssignment
                { SubjectID = subjects[i].ID, TeacherID = tertiary.ID, CanGrade = false });

            graderBySubject[subjects[i].ID] = primary.ID;
        }

        // Give the real-email teacher a grading assignment on a popular active subject.
        var realTeacherId = teachers[RealTeacherIndex].ID;
        var lateSubject = subjects.First(s => s.IsActive);
        if (!_db.ChangeTracker.Entries<TeachingAssignment>()
                .Any(e => e.Entity.SubjectID == lateSubject.ID && e.Entity.TeacherID == realTeacherId))
        {
            _db.TeachingAssignments.Add(new TeachingAssignment
            { SubjectID = lateSubject.ID, TeacherID = realTeacherId, CanGrade = true });
        }
        graderBySubject[lateSubject.ID] = realTeacherId;
        return graderBySubject;
    }

    private List<Enrollment> BuildEnrollments(List<Student> students, List<Subject> subjects)
    {
        var enrollments = new List<Enrollment>();
        for (var si = 0; si < students.Count; si++)
        {
            var count = 6 + _rng.Next(6); // 6-11 subjects per student
            var chosen = new HashSet<int>();
            while (chosen.Count < count)
                chosen.Add(_rng.Next(subjects.Count));

            foreach (var ci in chosen.Order())
            {
                var e = new Enrollment
                {
                    StudentID = students[si].ID,
                    SubjectID = subjects[ci].ID,
                    CreatedAt = ToUtc(_today.AddDays(-700 - (si + ci) % 400), 9),
                    IsPassed = false,
                };
                enrollments.Add(e);
                _db.Enrollments.Add(e);
                _enrollmentKeys.Add((students[si].ID, subjects[ci].ID));
            }
        }
        return enrollments;
    }

    // ----- academic history / scenarios -------------------------------------------------

    private void BuildAcademicHistory(
        List<Student> students, List<Subject> subjects, List<Teacher> teachers,
        List<Enrollment> enrollments, Dictionary<int, int> graderBySubject,
        TermCalendar.ResolvedCalendar cal)
    {
        var subjectById = subjects.ToDictionary(s => s.ID);
        var enrollmentsByStudent = enrollments.GroupBy(e => e.StudentID)
            .ToDictionary(g => g.Key, g => g.ToList());

        var realStudentId = students[RealStudentIndex].ID;
        var realTeacherId = teachers[RealTeacherIndex].ID;

        foreach (var student in students)
        {
            var mine = enrollmentsByStudent[student.ID];
            var isReal = student.ID == realStudentId;

            for (var k = 0; k < mine.Count; k++)
            {
                var enrollment = mine[k];
                var subject = subjectById[enrollment.SubjectID];
                var scenario = ChooseScenario(isReal, k, mine.Count, subject);
                ApplyScenario(scenario, enrollment, subject, cal, graderBySubject);
            }
        }

        // Guarantee the two real-email reminder scenarios regardless of RNG outcome.
        EnsureStudentReminderScenario(realStudentId, subjects, enrollmentsByStudent, cal);
        EnsureTeacherReminderScenario(realTeacherId, subjects, students, graderBySubject, cal);

        // A handful of other teachers also left with stale unsigned results.
        foreach (var teacher in teachers.Where((_, i) => i is 4 or 9 or 17 or 26))
            EnsureTeacherReminderScenario(teacher.ID, subjects, students, graderBySubject, cal, count: 3);
    }

    private enum Scenario
    {
        NoActivity, EligibleOpenTerm, RegisteredOpenTerm, CancelledHistory,
        PassedEarly, PassedAfterRetake, FailedOnly, Unattended,
        RegisteredCurrentUnsigned, SignedCurrent,
    }

    private Scenario ChooseScenario(bool isReal, int k, int total, Subject subject)
    {
        if (isReal)
        {
            // rich, explicit spread for the demo student
            return k switch
            {
                0 or 1 or 2 => Scenario.EligibleOpenTerm,
                3 => Scenario.PassedEarly,
                4 => Scenario.FailedOnly,
                5 => Scenario.RegisteredCurrentUnsigned,
                _ => Scenario.NoActivity,
            };
        }

        var roll = _rng.Next(100);
        var s = roll switch
        {
            < 18 => Scenario.PassedEarly,
            < 28 => Scenario.PassedAfterRetake,
            < 38 => Scenario.FailedOnly,
            < 46 => Scenario.Unattended,
            < 55 => Scenario.CancelledHistory,
            < 66 => Scenario.RegisteredCurrentUnsigned,
            < 76 => Scenario.SignedCurrent,
            < 88 => Scenario.EligibleOpenTerm,
            < 96 => Scenario.RegisteredOpenTerm,
            _ => Scenario.NoActivity,
        };

        // Retired subjects only carry historical outcomes.
        if (!subject.IsActive && s is Scenario.EligibleOpenTerm or Scenario.RegisteredOpenTerm
            or Scenario.RegisteredCurrentUnsigned or Scenario.SignedCurrent)
            s = Scenario.PassedEarly;

        return s;
    }

    private void ApplyScenario(
        Scenario scenario, Enrollment enrollment, Subject subject,
        TermCalendar.ResolvedCalendar cal, Dictionary<int, int> graderBySubject)
    {
        Count(scenario.ToString());
        var grader = graderBySubject[subject.ID];

        switch (scenario)
        {
            case Scenario.NoActivity:
                break;

            case Scenario.EligibleOpenTerm:
                // enrolled, not passed, no registration in the reminder term -> eligible to register
                Maybe(0.4, () => AddSignedExam(enrollment, PickHistorical(cal), grader, grade: 5));
                break;

            case Scenario.RegisteredOpenTerm:
                AddActiveRegistration(enrollment, cal.ReminderTerm);
                break;

            case Scenario.CancelledHistory:
                AddCancelledRegistration(enrollment, PickHistorical(cal));
                break;

            case Scenario.PassedEarly:
                AddSignedExam(enrollment, PickHistorical(cal), grader, grade: (byte)_rng.Next(6, 11));
                break;

            case Scenario.PassedAfterRetake:
            {
                var (first, second) = PickTwoHistorical(cal);
                AddSignedExam(enrollment, first, grader, grade: 5);
                if (!enrollment.IsPassed)
                    AddSignedExam(enrollment, second, grader, grade: (byte)_rng.Next(6, 11));
                break;
            }

            case Scenario.FailedOnly:
                AddSignedExam(enrollment, PickHistorical(cal), grader, grade: 5);
                break;

            case Scenario.Unattended:
                AddSignedExam(enrollment, PickHistorical(cal), grader, grade: null);
                break;

            case Scenario.RegisteredCurrentUnsigned:
                // result entered but not signed yet
                AddUnsignedExam(enrollment, cal.CurrentTerm, grader,
                    grade: _rng.Next(3) == 0 ? (byte)5 : (byte)_rng.Next(6, 11));
                break;

            case Scenario.SignedCurrent:
                AddSignedExam(enrollment, cal.CurrentTerm, grader,
                    grade: _rng.Next(4) == 0 ? (byte)5 : (byte)_rng.Next(6, 11));
                break;
        }
    }

    private void EnsureStudentReminderScenario(
        int studentId, List<Subject> subjects,
        Dictionary<int, List<Enrollment>> enrollmentsByStudent,
        TermCalendar.ResolvedCalendar cal)
    {
        var mine = enrollmentsByStudent[studentId];
        var activeSubjectIds = subjects.Where(s => s.IsActive).Select(s => s.ID).ToHashSet();
        var eligible = mine
            .Where(e => activeSubjectIds.Contains(e.SubjectID) && !e.IsPassed)
            .Take(3)
            .ToList();

        foreach (var e in eligible)
        {
            // drop any active registration this student holds in the reminder term
            if (_regByKey.TryGetValue((studentId, e.SubjectID, cal.ReminderTerm.ID), out var reg))
                reg.IsActive = false;
        }
        Count($"__GUARANTEED_StudentReminder({eligible.Count})");
    }

    private void EnsureTeacherReminderScenario(
        int teacherId, List<Subject> subjects, List<Student> students,
        Dictionary<int, int> graderBySubject, TermCalendar.ResolvedCalendar cal, int count = 5)
    {
        var subjectId = graderBySubject.First(kv => kv.Value == teacherId).Key;
        var terms = new[] { cal.LateGradingTerm, cal.SecondLateGradingTerm }.Distinct().ToArray();

        var added = 0;
        for (var i = 0; i < count; i++)
        {
            var term = terms[i % terms.Length];
            var student = students[(teacherId * 7 + i * 3) % students.Count];

            if (_examKeys.Contains((student.ID, subjectId, term.ID))) continue;

            EnsureEnrollment(student.ID, subjectId);
            AddRegistration(student.ID, subjectId, term, active: true, cancelledAt: null);
            _db.Exams.Add(new Exam
            {
                StudentID = student.ID,
                SubjectID = subjectId,
                TermID = term.ID,
                TeacherID = teacherId,
                Grade = null,
                SignedAt = null,
                Date = ExamDate(term, student.ID),
                Note = null,
            });
            _examKeys.Add((student.ID, subjectId, term.ID));
            added++;
        }
        Count($"__GUARANTEED_TeacherReminder({added})");
    }

    // ----- low-level builders ----------------------------------------------------------

    private void AddSignedExam(Enrollment enrollment, Term term, int graderId, byte? grade)
    {
        if (enrollment.IsPassed) return;
        var key = (enrollment.StudentID, enrollment.SubjectID, term.ID);
        if (_examKeys.Contains(key)) return;

        AddRegistration(enrollment.StudentID, enrollment.SubjectID, term, active: false, cancelledAt: null);
        var signedAt = ToUtc(Min(term.EndDate.AddDays(1), _today), 12);
        _db.Exams.Add(new Exam
        {
            StudentID = enrollment.StudentID,
            SubjectID = enrollment.SubjectID,
            TermID = term.ID,
            TeacherID = graderId,
            Grade = grade,
            SignedAt = signedAt,
            Date = ExamDate(term, enrollment.StudentID),
            Note = null,
        });
        _examKeys.Add(key);

        if (grade is >= 6)
        {
            enrollment.IsPassed = true;
            enrollment.PassedAt = signedAt;
        }
    }

    private void AddUnsignedExam(Enrollment enrollment, Term term, int graderId, byte? grade)
    {
        if (enrollment.IsPassed) return;
        var key = (enrollment.StudentID, enrollment.SubjectID, term.ID);
        if (_examKeys.Contains(key)) return;

        AddRegistration(enrollment.StudentID, enrollment.SubjectID, term, active: true, cancelledAt: null);
        _db.Exams.Add(new Exam
        {
            StudentID = enrollment.StudentID,
            SubjectID = enrollment.SubjectID,
            TermID = term.ID,
            TeacherID = graderId,
            Grade = grade,
            SignedAt = null,
            Date = ExamDate(term, enrollment.StudentID),
            Note = null,
        });
        _examKeys.Add(key);
    }

    private void AddActiveRegistration(Enrollment enrollment, Term term)
    {
        if (enrollment.IsPassed) return;
        AddRegistration(enrollment.StudentID, enrollment.SubjectID, term, active: true, cancelledAt: null);
    }

    private void AddCancelledRegistration(Enrollment enrollment, Term term)
    {
        if (_regByKey.ContainsKey((enrollment.StudentID, enrollment.SubjectID, term.ID))) return;
        AddRegistration(enrollment.StudentID, enrollment.SubjectID, term, active: false,
            cancelledAt: ToUtc(term.RegistrationStartDate.AddDays(2), 14));
    }

    private void AddRegistration(int studentId, int subjectId, Term term, bool active, DateTime? cancelledAt)
    {
        var key = (studentId, subjectId, term.ID);
        if (_regByKey.ContainsKey(key)) return;
        var registration = new Registration
        {
            StudentID = studentId,
            SubjectID = subjectId,
            TermID = term.ID,
            RegisteredAt = ToUtc(term.RegistrationStartDate.AddDays(1), 10),
            IsActive = active,
            CancelledAt = cancelledAt,
        };
        _db.Registrations.Add(registration);
        _regByKey[key] = registration;
    }

    private void EnsureEnrollment(int studentId, int subjectId)
    {
        if (!_enrollmentKeys.Add((studentId, subjectId))) return;
        _db.Enrollments.Add(new Enrollment
        {
            StudentID = studentId,
            SubjectID = subjectId,
            CreatedAt = ToUtc(_today.AddDays(-500), 9),
            IsPassed = false,
        });
    }

    // ----- helpers -------------------------------------------------------------------

    private Term PickHistorical(TermCalendar.ResolvedCalendar cal)
        => cal.HistoricalTerms[_rng.Next(cal.HistoricalTerms.Count)];

    private (Term, Term) PickTwoHistorical(TermCalendar.ResolvedCalendar cal)
    {
        var a = _rng.Next(cal.HistoricalTerms.Count);
        var b = _rng.Next(cal.HistoricalTerms.Count);
        if (a == b) b = (b + 1) % cal.HistoricalTerms.Count;
        var (lo, hi) = a < b ? (a, b) : (b, a);
        return (cal.HistoricalTerms[lo], cal.HistoricalTerms[hi]);
    }

    private DateOnly ExamDate(Term term, int studentId)
        => Min(term.StartDate.AddDays(1 + (studentId + _rng.Next(4)) % Math.Max(1, term.EndDate.DayNumber - term.StartDate.DayNumber - 1)), term.EndDate);

    private void Maybe(double probability, Action action)
    {
        if (_rng.NextDouble() < probability) action();
    }

    private void Count(string key)
        => _scenarios[key] = _scenarios.GetValueOrDefault(key) + 1;

    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

    private static DateTime ToUtc(DateOnly date, int hour)
        => DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(hour, 0)), DateTimeKind.Utc);

    private async Task<SeedResult> SummariseAsync(bool wasCreated, bool dryRun, CancellationToken ct)
    {
        // For a freshly created dataset the entities are tracked but (for dry run) not
        // committed, so count from the change tracker; otherwise query the database.
        var exams = wasCreated
            ? _db.ChangeTracker.Entries<Exam>().Select(e => e.Entity).ToList()
            : await _db.Exams.AsNoTracking().ToListAsync(ct);
        var regs = wasCreated
            ? _db.ChangeTracker.Entries<Registration>().Select(e => e.Entity).ToList()
            : await _db.Registrations.AsNoTracking().ToListAsync(ct);
        var enrollments = wasCreated
            ? _db.ChangeTracker.Entries<Enrollment>().Select(e => e.Entity).ToList()
            : await _db.Enrollments.AsNoTracking().ToListAsync(ct);

        return new SeedResult(
            WasCreated: wasCreated,
            DryRun: dryRun,
            StudentServices: await Users(UserRole.StudentService),
            Teachers: await Users(UserRole.Teacher),
            Students: await Users(UserRole.Student),
            Subjects: wasCreated
                ? _db.ChangeTracker.Entries<Subject>().Count()
                : await _db.Subjects.CountAsync(ct),
            Terms: wasCreated
                ? _db.ChangeTracker.Entries<Term>().Count()
                : await _db.Terms.CountAsync(ct),
            TeachingAssignments: wasCreated
                ? _db.ChangeTracker.Entries<TeachingAssignment>().Count()
                : await _db.TeachingAssignments.CountAsync(ct),
            Enrollments: enrollments.Count,
            Registrations: regs.Count,
            ActiveRegistrations: regs.Count(r => r.IsActive),
            CancelledRegistrations: regs.Count(r => r.CancelledAt != null),
            Exams: exams.Count,
            SignedExams: exams.Count(e => e.SignedAt != null),
            UnsignedExams: exams.Count(e => e.SignedAt == null),
            NullGradeExams: exams.Count(e => e.Grade == null),
            PassedEnrollments: enrollments.Count(e => e.IsPassed),
            ScenarioHistogram: new Dictionary<string, int>(_scenarios));

        async Task<int> Users(UserRole role) => wasCreated
            ? _db.ChangeTracker.Entries<User>().Count(e => e.Entity.Role == role)
            : await _db.Users.IgnoreQueryFilters().CountAsync(u => u.Role == role, ct);
    }
}
