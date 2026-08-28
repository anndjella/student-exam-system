using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudentExam.DbSeeder;

const string Usage = """
StudentExam.DbSeeder — explicit, transactional demo-data seeder for the Student Exam System.

USAGE
  dotnet run --project tools/StudentExam.DbSeeder -- <command> [options]

COMMANDS
  status      Report whether the target database is empty, already seeded, or foreign.
  dry-run     Build the full dataset inside a transaction, print the summary, then ROLL BACK.
  seed        Build and COMMIT the dataset. Refuses a non-empty database that this tool did not create.

OPTIONS
  --connection "<cs>"   ADO.NET connection string. Falls back to env SEED_CONNECTION_STRING,
                        then ConnectionStrings__DefaultConnection.
  --today YYYY-MM-DD     Anchor date for time-relative scenarios (default: local today).
  --timezone <id>       IANA/Windows time-zone id for 'today' (default: Europe/Belgrade).
  --migrate             Apply pending EF Core migrations before seeding (off by default).

EXIT CODES
  0  success / database already seeded
  1  usage or runtime error
  2  refused: database is not empty and was not produced by this seeder
  3  refused: pending EF Core migrations (run 'dotnet ef database update' or pass --migrate)
""";

try
{
    var argList = args.ToList();
    if (argList.Count == 0 || argList[0] is "-h" or "--help" or "help")
    {
        Console.WriteLine(Usage);
        return 0;
    }

    var command = argList[0].ToLowerInvariant();
    var dryRunFlag = argList.Remove("--dry-run");
    var migrate = argList.Remove("--migrate");
    var connection = TakeOption(argList, "--connection")
        ?? Environment.GetEnvironmentVariable("SEED_CONNECTION_STRING")
        ?? new ConfigurationBuilder().AddEnvironmentVariables().Build()
            .GetConnectionString("DefaultConnection");
    var todayText = TakeOption(argList, "--today");
    var timeZoneId = TakeOption(argList, "--timezone") ?? "Europe/Belgrade";

    if (string.IsNullOrWhiteSpace(connection))
    {
        Console.Error.WriteLine("No connection string. Pass --connection or set SEED_CONNECTION_STRING.");
        return 1;
    }

    var today = ResolveToday(todayText, timeZoneId);

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connection, sql => sql.CommandTimeout(180))
        .Options;
    await using var db = new AppDbContext(options);

    Console.WriteLine($"Target      : {SafeDataSource(connection)}");
    Console.WriteLine($"Anchor date : {today:yyyy-MM-dd}");

    var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
    var seeder = new DemoDataSeeder(db, today);

    switch (command)
    {
        case "status":
        {
            var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();
            Console.WriteLine($"Migrations  : {applied.Count} applied, {pending.Count} pending");
            if (await seeder.IsAlreadySeededAsync(default))
            {
                Console.WriteLine("State       : ALREADY SEEDED (demo marker present).");
                (await seeder.RunAsync(commit: true, default)).Print(Console.Out);
                return 0;
            }
            if (await seeder.ContainsForeignDataAsync(default))
            {
                Console.WriteLine("State       : NOT EMPTY and no demo marker — seeding would be REFUSED.");
                return 2;
            }
            Console.WriteLine("State       : EMPTY — ready to seed.");
            return 0;
        }

        case "dry-run":
            (await seeder.RunAsync(commit: false, default)).Print(Console.Out);
            return 0;

        case "seed":
        {
            if (pending.Count > 0 && !migrate)
            {
                Console.Error.WriteLine(
                    $"{pending.Count} pending migration(s). Run 'dotnet ef database update' first or pass --migrate.");
                return 3;
            }
            if (migrate && pending.Count > 0)
            {
                Console.WriteLine($"Applying {pending.Count} migration(s)...");
                await db.Database.MigrateAsync();
            }

            var result = await seeder.RunAsync(commit: !dryRunFlag, default);
            result.Print(Console.Out);
            return 0;
        }

        default:
            Console.Error.WriteLine($"Unknown command '{command}'.\n\n{Usage}");
            return 1;
    }
}
catch (InvalidOperationException ex) when (ex.Message.Contains("non-empty"))
{
    Console.Error.WriteLine($"REFUSED: {ex.Message}");
    return 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    return 1;
}

static string? TakeOption(List<string> args, string name)
{
    var i = args.IndexOf(name);
    if (i < 0 || i + 1 >= args.Count) return null;
    var value = args[i + 1];
    args.RemoveRange(i, 2);
    return value;
}

static DateOnly ResolveToday(string? text, string timeZoneId)
{
    if (!string.IsNullOrWhiteSpace(text))
        return DateOnly.ParseExact(text, "yyyy-MM-dd");
    try
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).DateTime);
    }
    catch (TimeZoneNotFoundException)
    {
        return DateOnly.FromDateTime(DateTime.UtcNow);
    }
}

static string SafeDataSource(string connectionString)
{
    try
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        return $"{builder.DataSource} / {builder.InitialCatalog}";
    }
    catch
    {
        return "(unparsed connection string)";
    }
}
