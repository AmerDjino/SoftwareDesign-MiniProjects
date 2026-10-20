namespace Timesheets.Tests;

public sealed class TimesheetReportPipelineTests
{
    [Fact]
    public void Run_CsvWithValidAndInvalidRows_ProducesCompleteReport()
    {
        const string csv =
            "employee,date,hours\n" +
            "Ana Kovac,2026-09-28,9\n" +
            "Ana Kovac,2026-09-29,9\n" +
            "Ana Kovac,2026-09-30,9.5\n" +
            "Ana Kovac,2026-10-01,9\n" +
            "Ana Kovac,2026-10-02,6\n" +
            "Emir Hadzic,2026-09-28,8\n" +
            "Emir Hadzic,2026-09-29,7,5\n" +
            "Emir Hadzic,2026-09-30,26\n";
        // bs-Latn-BA writes 7.5 as "7,5" and uses "." as the thousands separator.
        using var culture = new CultureScope("bs-Latn-BA");

        var report = TimesheetReportPipeline.Run(csv);

        const string expected =
            "# Overtime report\n" +
            "\n" +
            "## Weekly hours\n" +
            "\n" +
            "| Employee | Week | Total | Regular | Overtime |\n" +
            "|---|---|---:|---:|---:|\n" +
            "| Ana Kovac | 2026-W40 | 42.50 | 40.00 | 2.50 |\n" +
            "| Emir Hadzic | 2026-W40 | 8.00 | 8.00 | 0.00 |\n" +
            "\n" +
            "## Rejected rows\n" +
            "\n" +
            "- Line 8: Expected 3 fields but found 4.\n" +
            "- Line 9: Hours '26' must be greater than 0 and at most 24.\n";
        Assert.Equal(expected, report);
    }
}
