using Domain.Common;

namespace StudentExam.DbSeeder.Data;

internal sealed class ValueFactories
{
    private readonly HashSet<string> _jmbg = new();
    private readonly HashSet<string> _indexNumbers = new();
    private readonly HashSet<string> _employeeNumbers = new();
    private readonly HashSet<string> _usernames = new(StringComparer.OrdinalIgnoreCase);
    private int _jmbgSequence;

    public string NextJmbg(DateOnly dateOfBirth)
    {
        while (true)
        {
            var seq = _jmbgSequence++;
            var yearPart = dateOfBirth.Year >= 2000 ? dateOfBirth.Year - 2000 : dateOfBirth.Year - 1000;
            var region = 70 + (seq / 1000) % 30;
            var serial = seq % 1000;
            var first12 = $"{dateOfBirth.Day:00}{dateOfBirth.Month:00}{yearPart:000}{region:00}{serial:000}";
            var jmbg = first12 + Checksum(first12);
            if (_jmbg.Add(jmbg))
                return jmbg;
        }
    }

    public string NextIndexNumber(int entryYear, int serial)
        => Unique(_indexNumbers, $"{entryYear:0000}/{serial:0000}", () => throw new InvalidOperationException(
            "Ran out of unique index numbers."));

    public string NextEmployeeNumber(int employmentYear, int serial)
        => Unique(_employeeNumbers, $"{employmentYear:0000}/{serial:0000}", () => throw new InvalidOperationException(
            "Ran out of unique employee numbers."));

    public string StudentUsername(string first, string last, string indexNumber)
        => UniqueUsername(CredentialsGenerator.StudentUsername(first, last, indexNumber));

    public string TeacherUsername(string first, string last, string employeeNumber)
        => UniqueUsername(CredentialsGenerator.TeacherUsername(first, last, employeeNumber));

    public string ServiceUsername(int ordinal)
        => UniqueUsername($"studentservice{ordinal:00}");

    private static string Unique(HashSet<string> set, string candidate, Action onExhausted)
    {
        if (set.Add(candidate))
            return candidate;
        onExhausted();
        return candidate;
    }

    private string UniqueUsername(string baseName)
    {
        if (baseName.Length > 20)
            baseName = baseName[..20];
        if (_usernames.Add(baseName))
            return baseName;

        for (var suffix = 2; ; suffix++)
        {
            var suffixText = suffix.ToString();
            var trimmed = baseName.Length + suffixText.Length > 20
                ? baseName[..(20 - suffixText.Length)]
                : baseName;
            var candidate = trimmed + suffixText;
            if (_usernames.Add(candidate))
                return candidate;
        }
    }

    private static int Checksum(string first12)
    {
        var d = first12.Select(c => c - '0').ToArray();
        var m = 11 - (7 * (d[0] + d[6]) + 6 * (d[1] + d[7]) + 5 * (d[2] + d[8]) +
                      4 * (d[3] + d[9]) + 3 * (d[4] + d[10]) + 2 * (d[5] + d[11])) % 11;
        return m is >= 1 and <= 9 ? m : 0;
    }
}
