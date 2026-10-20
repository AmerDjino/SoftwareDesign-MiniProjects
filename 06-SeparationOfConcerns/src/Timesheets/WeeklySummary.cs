namespace Timesheets;

/// <summary>Hours of one employee in one ISO 8601 week.</summary>
/// <param name="IsoYear">The ISO week-numbering year, which can differ from the calendar year (2025-12-29 is in 2026-W01).</param>
public sealed record WeeklySummary(
    string Employee,
    int IsoYear,
    int IsoWeek,
    decimal TotalHours,
    decimal RegularHours,
    decimal OvertimeHours);
