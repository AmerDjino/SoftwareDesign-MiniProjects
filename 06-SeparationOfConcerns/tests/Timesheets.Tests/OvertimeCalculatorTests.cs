namespace Timesheets.Tests;

public sealed class OvertimeCalculatorTests
{
    [Fact]
    public void Calculate_HoursAbove40PerIsoWeek_AreOvertime()
    {
        // ISO week 2026-W01 runs from Monday 2025-12-29 to Sunday 2026-01-04.
        TimeEntry[] entries =
        [
            new("Lejla Begic", new DateOnly(2026, 1, 6), 6m),
            new("Ana Kovac", new DateOnly(2026, 1, 5), 7.5m),
            new("Ana Kovac", new DateOnly(2025, 12, 29), 10m),
            new("Ana Kovac", new DateOnly(2025, 12, 30), 10m),
            new("ana kovac", new DateOnly(2025, 12, 31), 10m),
            new("Ana Kovac", new DateOnly(2026, 1, 2), 12.5m),
        ];

        var summaries = OvertimeCalculator.Calculate(entries);

        WeeklySummary[] expected =
        [
            new("Ana Kovac", 2026, 1, 42.5m, 40m, 2.5m),
            new("Ana Kovac", 2026, 2, 7.5m, 7.5m, 0m),
            new("Lejla Begic", 2026, 2, 6m, 6m, 0m),
        ];
        Assert.Equal(expected, summaries);
    }
}
