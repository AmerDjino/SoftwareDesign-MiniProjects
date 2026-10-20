namespace Timesheets;

public sealed record ValidationError(int LineNumber, string Message);
