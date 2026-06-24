using Domain.Interfaces;

namespace Tests.TestDoubles;

internal sealed class TestClock : IClock
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 20, 12, 0, 0, DateTimeKind.Utc);
    public DateOnly Today { get; set; } = new(2026, 1, 20);
}
