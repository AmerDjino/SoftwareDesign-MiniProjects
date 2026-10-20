namespace Timesheets;

public sealed record TimeEntry(string Employee, DateOnly Date, decimal Hours);
