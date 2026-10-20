namespace Timesheets;

public static class TimesheetReportPipeline
{
    /// <summary>CSV text in, Markdown report out: parse, validate, calculate, format. No I/O.</summary>
    /// <exception cref="FormatException">The text is not a timesheet export (line 1 is not the header).</exception>
    public static string Run(string csvText)
    {
        var rows = TimesheetCsvParser.Parse(csvText);
        var validation = TimesheetValidator.Validate(rows);
        var summaries = OvertimeCalculator.Calculate(validation.Entries);
        return MarkdownReportFormatter.Format(summaries, validation.Errors);
    }
}
