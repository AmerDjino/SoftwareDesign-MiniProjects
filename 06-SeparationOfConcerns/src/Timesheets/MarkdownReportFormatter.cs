using System.Globalization;

namespace Timesheets;

public static class MarkdownReportFormatter
{
    /// <summary>
    /// Renders the summaries and errors in the order given, with <c>\n</c> line endings on every platform.
    /// </summary>
    public static string Format(IReadOnlyList<WeeklySummary> summaries, IReadOnlyList<ValidationError> errors)
    {
        List<string> lines =
        [
            "# Overtime report",
            "",
            "## Weekly hours",
            "",
            "| Employee | Week | Total | Regular | Overtime |",
            "|---|---|---:|---:|---:|",
            .. summaries.Select(SummaryRow),
            "",
            "## Rejected rows",
            "",
            .. errors.Count == 0 ? ["None."] : errors.Select(ErrorLine),
        ];

        return string.Concat(lines.Select(line => line + "\n"));
    }

    private static string SummaryRow(WeeklySummary summary) => string.Create(
        CultureInfo.InvariantCulture,
        $"| {summary.Employee} | {summary.IsoYear}-W{summary.IsoWeek:00} | {summary.TotalHours:0.00} | {summary.RegularHours:0.00} | {summary.OvertimeHours:0.00} |");

    private static string ErrorLine(ValidationError error) => string.Create(
        CultureInfo.InvariantCulture,
        $"- Line {error.LineNumber}: {error.Message}");
}
