namespace Timesheets.Tests;

public sealed class TimesheetValidatorTests
{
    [Fact]
    public void Validate_InvalidRows_AreRejectedWithLineNumberAndReason()
    {
        RawRow[] rows =
        [
            new(2, ["Ana Kovac", "2026-09-28", "8.5"]),
            new(3, ["Ana Kovac", "2026-09-29"]),
            new(4, ["", "2026-13-01", "eight"]),
            new(5, ["Ana Kovac", "29.09.2026", "8"]),
            new(6, ["Ana Kovac", "2026-09-30", "eight"]),
            new(7, ["Ana Kovac", "2026-10-01", "0"]),
            new(8, ["Ana Kovac", "2026-10-02", "24.5"]),
            new(9, ["Emir Hadzic", "2026-10-02", "24"]),
        ];

        var result = TimesheetValidator.Validate(rows);

        TimeEntry[] expectedEntries =
        [
            new("Ana Kovac", new DateOnly(2026, 9, 28), 8.5m),
            new("Emir Hadzic", new DateOnly(2026, 10, 2), 24m),
        ];
        ValidationError[] expectedErrors =
        [
            new(3, "Expected 3 fields but found 2."),
            new(4, "Employee is missing."),
            new(5, "Date '29.09.2026' is not a valid yyyy-MM-dd date."),
            new(6, "Hours 'eight' is not a number."),
            new(7, "Hours '0' must be greater than 0 and at most 24."),
            new(8, "Hours '24.5' must be greater than 0 and at most 24."),
        ];
        Assert.Equal(expectedEntries, result.Entries);
        Assert.Equal(expectedErrors, result.Errors);
    }
}
