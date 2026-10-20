namespace Timesheets;

public sealed record ValidationResult(IReadOnlyList<TimeEntry> Entries, IReadOnlyList<ValidationError> Errors);
