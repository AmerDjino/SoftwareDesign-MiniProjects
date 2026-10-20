using System.Globalization;

namespace Timesheets;

public static class OvertimeCalculator
{
    private const decimal RegularHoursPerWeek = 40m;

    /// <summary>
    /// Totals the hours per employee (names compared case-insensitively) and ISO week and splits each total
    /// into regular and overtime hours. Ordered by employee, ISO year and ISO week.
    /// </summary>
    public static IReadOnlyList<WeeklySummary> Calculate(IEnumerable<TimeEntry> entries)
    {
        return entries
            .GroupBy(entry => entry.Employee, StringComparer.OrdinalIgnoreCase)
            .SelectMany(employeeEntries => employeeEntries
                .GroupBy(entry => IsoWeekOf(entry.Date))
                .Select(weekEntries => Summarize(
                    employeeEntries.Key,
                    weekEntries.Key,
                    weekEntries.Sum(entry => entry.Hours))))
            .OrderBy(summary => summary.Employee, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.IsoYear)
            .ThenBy(summary => summary.IsoWeek)
            .ToList();
    }

    private static (int Year, int Week) IsoWeekOf(DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        return (ISOWeek.GetYear(dateTime), ISOWeek.GetWeekOfYear(dateTime));
    }

    private static WeeklySummary Summarize(string employee, (int Year, int Week) isoWeek, decimal totalHours) =>
        new(
            employee,
            isoWeek.Year,
            isoWeek.Week,
            totalHours,
            RegularHours: Math.Min(totalHours, RegularHoursPerWeek),
            OvertimeHours: Math.Max(totalHours - RegularHoursPerWeek, 0m));
}
