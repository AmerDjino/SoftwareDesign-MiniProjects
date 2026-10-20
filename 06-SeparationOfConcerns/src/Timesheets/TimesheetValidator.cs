using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Timesheets;

public static class TimesheetValidator
{
    private const int FieldCount = 3;
    private const string DateFormat = "yyyy-MM-dd";
    private const NumberStyles HoursStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
    private const decimal MaxHoursPerDay = 24m;

    /// <summary>
    /// Turns raw rows into typed entries. A row that breaks a rule gives exactly one error:
    /// the first rule it breaks. Entries and errors keep the order of the rows.
    /// </summary>
    public static ValidationResult Validate(IEnumerable<RawRow> rows)
    {
        var entries = new List<TimeEntry>();
        var errors = new List<ValidationError>();

        foreach (var row in rows)
        {
            if (TryCreateEntry(row.Fields, out var entry, out var problem))
            {
                entries.Add(entry);
            }
            else
            {
                errors.Add(new ValidationError(row.LineNumber, problem));
            }
        }

        return new ValidationResult(entries, errors);
    }

    private static bool TryCreateEntry(
        IReadOnlyList<string> fields,
        [NotNullWhen(true)] out TimeEntry? entry,
        [NotNullWhen(false)] out string? problem)
    {
        entry = null;

        if (fields.Count != FieldCount)
        {
            problem = string.Create(CultureInfo.InvariantCulture, $"Expected {FieldCount} fields but found {fields.Count}.");
            return false;
        }

        var (employee, dateText, hoursText) = (fields[0], fields[1], fields[2]);

        if (employee.Length == 0)
        {
            problem = "Employee is missing.";
            return false;
        }

        if (!DateOnly.TryParseExact(dateText, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            problem = $"Date '{dateText}' is not a valid {DateFormat} date.";
            return false;
        }

        if (!decimal.TryParse(hoursText, HoursStyle, CultureInfo.InvariantCulture, out var hours))
        {
            problem = $"Hours '{hoursText}' is not a number.";
            return false;
        }

        if (hours <= 0 || hours > MaxHoursPerDay)
        {
            problem = string.Create(CultureInfo.InvariantCulture, $"Hours '{hoursText}' must be greater than 0 and at most {MaxHoursPerDay}.");
            return false;
        }

        entry = new TimeEntry(employee, date, hours);
        problem = null;
        return true;
    }
}
