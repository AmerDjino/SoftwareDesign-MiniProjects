namespace Timesheets.Tests;

public sealed class MarkdownReportFormatterTests
{
    [Fact]
    public void Format_RendersSummaryTableAndRejectedRows()
    {
        WeeklySummary[] summaries =
        [
            new("Ana Kovac", 2026, 5, 42.5m, 40m, 2.5m),
            new("Emir Hadzic", 2026, 40, 38m, 38m, 0m),
        ];
        ValidationError[] errors =
        [
            new(4, "Expected 3 fields but found 4."),
            new(7, "Hours '26' must be greater than 0 and at most 24."),
        ];

        var report = MarkdownReportFormatter.Format(summaries, errors);

        const string expected =
            "# Overtime report\n" +
            "\n" +
            "## Weekly hours\n" +
            "\n" +
            "| Employee | Week | Total | Regular | Overtime |\n" +
            "|---|---|---:|---:|---:|\n" +
            "| Ana Kovac | 2026-W05 | 42.50 | 40.00 | 2.50 |\n" +
            "| Emir Hadzic | 2026-W40 | 38.00 | 38.00 | 0.00 |\n" +
            "\n" +
            "## Rejected rows\n" +
            "\n" +
            "- Line 4: Expected 3 fields but found 4.\n" +
            "- Line 7: Hours '26' must be greater than 0 and at most 24.\n";
        Assert.Equal(expected, report);
    }
}
