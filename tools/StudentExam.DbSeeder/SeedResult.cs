namespace StudentExam.DbSeeder;

public sealed record SeedResult(
    bool WasCreated,
    bool DryRun,
    int StudentServices,
    int Teachers,
    int Students,
    int Subjects,
    int Terms,
    int TeachingAssignments,
    int Enrollments,
    int Registrations,
    int ActiveRegistrations,
    int CancelledRegistrations,
    int Exams,
    int SignedExams,
    int UnsignedExams,
    int NullGradeExams,
    int PassedEnrollments,
    IReadOnlyDictionary<string, int> ScenarioHistogram)
{
    public void Print(TextWriter output)
    {
        output.WriteLine();
        output.WriteLine(DryRun
            ? "=== DRY RUN — transaction rolled back, no changes committed ==="
            : WasCreated
                ? "=== Demo dataset created ==="
                : "=== Demo dataset already present (no changes) ===");
        output.WriteLine($"  Student-service accounts : {StudentServices}");
        output.WriteLine($"  Teachers                : {Teachers}");
        output.WriteLine($"  Students                : {Students}");
        output.WriteLine($"  Subjects                : {Subjects}");
        output.WriteLine($"  Terms                   : {Terms}");
        output.WriteLine($"  Teaching assignments    : {TeachingAssignments}");
        output.WriteLine($"  Enrollments             : {Enrollments}");
        output.WriteLine($"  Registrations           : {Registrations} (active {ActiveRegistrations}, cancelled {CancelledRegistrations})");
        output.WriteLine($"  Exams                   : {Exams} (signed {SignedExams}, unsigned {UnsignedExams}, no-grade {NullGradeExams})");
        output.WriteLine($"  Passed enrollments      : {PassedEnrollments}");
        if (ScenarioHistogram.Count > 0)
        {
            output.WriteLine("  Scenario histogram:");
            foreach (var (name, count) in ScenarioHistogram.OrderBy(x => x.Key))
                output.WriteLine($"    - {name,-32} {count}");
        }
        output.WriteLine();
    }
}
