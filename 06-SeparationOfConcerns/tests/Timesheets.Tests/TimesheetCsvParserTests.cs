namespace Timesheets.Tests;

public sealed class TimesheetCsvParserTests
{
    [Fact]
    public void Parse_SkipsHeaderAndBlankLines_AndKeepsPhysicalLineNumbers()
    {
        const string csv =
            "Employee,Date,Hours\r\n" +
            "Ana Kovac,2026-09-28,8.5\r\n" +
            "\r\n" +
            "   \r\n" +
            "  Emir Hadzic , 2026-09-28 , 7 \r\n" +
            "Lejla Begic,2026-09-28\r\n";

        var rows = TimesheetCsvParser.Parse(csv);

        Assert.Collection(
            rows,
            row => AssertRow(row, 2, "Ana Kovac", "2026-09-28", "8.5"),
            row => AssertRow(row, 5, "Emir Hadzic", "2026-09-28", "7"),
            row => AssertRow(row, 6, "Lejla Begic", "2026-09-28"));
    }

    private static void AssertRow(RawRow row, int expectedLineNumber, params string[] expectedFields)
    {
        Assert.Equal(expectedLineNumber, row.LineNumber);
        Assert.Equal(expectedFields, row.Fields);
    }
}
