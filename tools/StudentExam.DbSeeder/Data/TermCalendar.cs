using Domain.Entity;

namespace StudentExam.DbSeeder.Data;

public static class TermCalendar
{
    private sealed record Spec(
        string Name,
        DateOnly RegStart, DateOnly RegEnd,
        DateOnly ExamStart, DateOnly ExamEnd);

    private static DateOnly D(int y, int m, int d) => new(y, m, d);

    private static readonly Spec[] Specs =
    [
        new("January 24/25",      D(2024, 12, 16), D(2024, 12, 27), D(2025, 1, 13), D(2025, 1, 24)),
        new("February 24/25",     D(2025, 1, 20),  D(2025, 1, 29),  D(2025, 2, 3),  D(2025, 2, 12)),
        new("March-April 24/25",  D(2025, 3, 10),  D(2025, 3, 19),  D(2025, 3, 31), D(2025, 4, 11)),
        new("June 24/25",         D(2025, 5, 19),  D(2025, 5, 28),  D(2025, 6, 9),  D(2025, 6, 27)),
        new("July 24/25",         D(2025, 6, 16),  D(2025, 6, 25),  D(2025, 7, 7),  D(2025, 7, 18)),
        new("September 24/25",    D(2025, 8, 18),  D(2025, 8, 27),  D(2025, 9, 1),  D(2025, 9, 19)),
        new("October 24/25",      D(2025, 9, 22),  D(2025, 9, 26),  D(2025, 10, 6), D(2025, 10, 15)),
        new("January 25/26",      D(2025, 12, 15), D(2025, 12, 26), D(2026, 1, 12), D(2026, 1, 23)),
        new("February 25/26",     D(2026, 1, 19),  D(2026, 1, 28),  D(2026, 2, 2),  D(2026, 2, 11)),
        new("March-April 25/26",  D(2026, 3, 9),   D(2026, 3, 18),  D(2026, 3, 30), D(2026, 4, 10)),
        new("June 25/26",         D(2026, 5, 18),  D(2026, 5, 27),  D(2026, 6, 8),  D(2026, 6, 26)),
        new("July 25/26",         D(2026, 6, 15),  D(2026, 6, 24),  D(2026, 7, 6),  D(2026, 7, 17)),
        new("September 25/26",    D(2026, 8, 17),  D(2026, 8, 26),  D(2026, 9, 1),  D(2026, 9, 18)),
        new("October 25/26",      D(2026, 9, 21),  D(2026, 9, 25),  D(2026, 10, 5), D(2026, 10, 14)),
    ];

    public sealed class ResolvedCalendar
    {
        public required IReadOnlyList<Term> Ordered { get; init; }
        public required Term ReminderTerm { get; init; }
        public required Term CurrentTerm { get; init; }
        public required Term LateGradingTerm { get; init; }
        public required Term SecondLateGradingTerm { get; init; }
        public required IReadOnlyList<Term> HistoricalTerms { get; init; }
        public required IReadOnlyList<Term> FutureTerms { get; init; }
    }

    public static ResolvedCalendar Resolve(DateOnly today)
    {
        var terms = Specs
            .Select(s => new Term
            {
                Name = s.Name,
                RegistrationStartDate = s.RegStart,
                RegistrationEndDate = s.RegEnd,
                StartDate = s.ExamStart,
                EndDate = s.ExamEnd,
            })
            .ToList();

        var reminderTerm = terms.FirstOrDefault(t => t.StartDate > today.AddDays(10))
                           ?? terms[^1];

        reminderTerm.RegistrationEndDate = today.AddDays(1);
        reminderTerm.RegistrationStartDate = today.AddDays(-9);
        if (reminderTerm.StartDate <= reminderTerm.RegistrationEndDate)
        {
            reminderTerm.StartDate = reminderTerm.RegistrationEndDate.AddDays(14);
            reminderTerm.EndDate = reminderTerm.StartDate.AddDays(10);
        }

        var currentTerm =
            terms.Where(t => t.EndDate <= today).OrderByDescending(t => t.EndDate).FirstOrDefault()
            ?? terms[0];

        var lateTerms = terms
            .Where(t => t.EndDate <= today.AddDays(-31) && t != reminderTerm)
            .OrderByDescending(t => t.EndDate)
            .ToList();
        var lateGradingTerm = lateTerms.ElementAtOrDefault(0) ?? currentTerm;
        var secondLateGradingTerm = lateTerms.ElementAtOrDefault(1) ?? lateGradingTerm;

        var historical = terms
            .Where(t => t != reminderTerm && t.EndDate < currentTerm.StartDate)
            .OrderBy(t => t.StartDate)
            .ToList();

        var future = terms
            .Where(t => t.StartDate > today && t != reminderTerm)
            .OrderBy(t => t.StartDate)
            .ToList();

        return new ResolvedCalendar
        {
            Ordered = terms.OrderBy(t => t.StartDate).ToList(),
            ReminderTerm = reminderTerm,
            CurrentTerm = currentTerm,
            LateGradingTerm = lateGradingTerm,
            SecondLateGradingTerm = secondLateGradingTerm,
            HistoricalTerms = historical,
            FutureTerms = future,
        };
    }
}
