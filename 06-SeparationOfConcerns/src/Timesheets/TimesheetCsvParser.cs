namespace Timesheets;

public static class TimesheetCsvParser
{
    private static readonly string[] HeaderFields = ["employee", "date", "hours"];

    /// <summary>
    /// CSV syntax only: skips the header and blank lines, splits every other line on commas and trims the fields.
    /// Field values are not interpreted.
    /// </summary>
    /// <exception cref="FormatException">Line 1 is not the header <c>employee,date,hours</c>.</exception>
    public static IReadOnlyList<RawRow> Parse(string csvText)
    {
        var lines = csvText.ReplaceLineEndings("\n").Split('\n');

        if (!SplitFields(lines[0]).SequenceEqual(HeaderFields, StringComparer.OrdinalIgnoreCase))
        {
            throw new FormatException($"Line 1 must be the header '{string.Join(',', HeaderFields)}'.");
        }

        var rows = new List<RawRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            if (!string.IsNullOrWhiteSpace(lines[index]))
            {
                rows.Add(new RawRow(LineNumber: index + 1, SplitFields(lines[index])));
            }
        }

        return rows;
    }

    private static string[] SplitFields(string line) => line.Split(',', StringSplitOptions.TrimEntries);
}
