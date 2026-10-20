# Refactor notes — 06 Separation of Concerns

## Starting point

The spec already designed one unit per concern, so the first-green code had the class-level separation from the start. Each unit was then filled in the most direct way that made its test pass, without polishing.

- `dotnet test`: `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 149 ms - Timesheets.Tests.dll (net9.0)`
- `dotnet build`: 0 warnings, 0 errors. The console app printed the expected report for the sample, exited with 1 for a missing file and with 2 for a file without the header.

The review after first green therefore found its problems *inside* units. That is SoC at function level: one method mixing two decisions, or one decision spread over many lines.

| File | First green | After refactor |
|---|---:|---:|
| `TimesheetCsvParser.cs` | 34 lines | 34 lines |
| `TimesheetValidator.cs` | 54 lines | 80 lines |
| `OvertimeCalculator.cs` | 35 lines | 43 lines |
| `MarkdownReportFormatter.cs` | 44 lines | 37 lines |
| `TimesheetReportPipeline.cs`, `Program.cs`, records, tests | — | unchanged |

## 1. Parser: one definition of the CSV dialect

**Smell.** "Split on commas and trim" was written twice (header and data lines), and the header text twice (the array and the error message).

**Reason.** The parser's only reason to change is the CSV dialect. With two copies, a dialect change (say, `;` as the delimiter) could reach the data lines but not the header check, and then every file is rejected.

**Change.** One `SplitFields` method, using `StringSplitOptions.TrimEntries`, serves both. A `HeaderFields` constant feeds both the check and the message. The 1-based line number is named at the call site.

```csharp
// Before
var header = lines[0].Split(',').Select(field => field.Trim()).ToArray();
if (!header.SequenceEqual(["employee", "date", "hours"], StringComparer.OrdinalIgnoreCase))
{
    throw new FormatException("Line 1 must be the header 'employee,date,hours'.");
}
// ...
var fields = lines[i].Split(',').Select(field => field.Trim()).ToArray();
rows.Add(new RawRow(i + 1, fields));

// After
if (!SplitFields(lines[0]).SequenceEqual(HeaderFields, StringComparer.OrdinalIgnoreCase))
{
    throw new FormatException($"Line 1 must be the header '{string.Join(',', HeaderFields)}'.");
}
// ...
rows.Add(new RawRow(LineNumber: index + 1, SplitFields(lines[index])));

private static string[] SplitFields(string line) => line.Split(',', StringSplitOptions.TrimEntries);
```

## 2. Validator: checking a row separated from collecting results

**Smell.** The loop repeated `errors.Add(new ValidationError(row.LineNumber, ...)); continue;` five times. Three things were mixed in one 35-line loop: the rules, the policy "the first failing rule wins" (expressed by the `continue`s) and the result lists. `3`, `24` and `"yyyy-MM-dd"` each appeared twice, in the check and in its message, so a message could drift away from its rule. `row.Fields[1]` and `row.Fields[2]` were repeated without names.

**Reason.** The rules change for their own reasons (a new limit, a new format); collecting results does not. Keeping each rule next to its message in one place stops them drifting apart.

**Change.** `Validate` now only sorts rows into entries and errors. `TryCreateEntry(fields, out entry, out problem)` holds the rules, returns at the first failure and knows nothing about line numbers. The limits and the date format are named constants, and the fields get names. Messages that contain numbers are built with `string.Create(CultureInfo.InvariantCulture, ...)`. That became necessary once the limit is a `decimal` constant inside the message, because an interpolated `decimal` follows the current culture. It also fixes the field count, which first green formatted with the current culture.

```csharp
// Before (two of the five rules)
foreach (var row in rows)
{
    if (row.Fields.Count != 3)
    {
        errors.Add(new ValidationError(row.LineNumber, $"Expected 3 fields but found {row.Fields.Count}."));
        continue;
    }
    // ... three more rules in the same shape ...
    if (hours <= 0 || hours > 24)
    {
        errors.Add(new ValidationError(row.LineNumber, $"Hours '{row.Fields[2]}' must be greater than 0 and at most 24."));
        continue;
    }

    entries.Add(new TimeEntry(employee, date, hours));
}

// After
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
// ... inside TryCreateEntry:
if (hours <= 0 || hours > MaxHoursPerDay)
{
    problem = string.Create(CultureInfo.InvariantCulture, $"Hours '{hoursText}' must be greater than 0 and at most {MaxHoursPerDay}.");
    return false;
}
```

**Cost.** The file grew from 54 to 80 lines, because the Try pattern needs both `out` parameters assigned on every path (`[NotNullWhen]` keeps it null-safe without `!`). I think it pays off because the rules are the part that will change. With two rules I would not have split it.

## 3. Calculator: the payroll rule gets a name

**Smell.** `40m` was written twice inside a nested lambda, so the business rule was the hardest part of the method to see. `entry.Date.ToDateTime(TimeOnly.MinValue)` was computed twice per entry to build the ISO key.

**Reason.** This unit changes when the payroll rule changes, so that rule should be the easiest thing in it to find.

**Change.** The rule now has names: the `RegularHoursPerWeek` constant (private, so no public API change), `Summarize` with named arguments for the split, and `IsoWeekOf`, which converts the date once. The chain now reads: group by employee, group by ISO week, summarize, order.

```csharp
// Before
.GroupBy(entry => (
    Year: ISOWeek.GetYear(entry.Date.ToDateTime(TimeOnly.MinValue)),
    Week: ISOWeek.GetWeekOfYear(entry.Date.ToDateTime(TimeOnly.MinValue))))
.Select(weekEntries =>
{
    var total = weekEntries.Sum(entry => entry.Hours);
    return new WeeklySummary(employeeEntries.Key, weekEntries.Key.Year, weekEntries.Key.Week,
        total, Math.Min(total, 40m), Math.Max(total - 40m, 0m));
}))

// After
.GroupBy(entry => IsoWeekOf(entry.Date))
.Select(weekEntries => Summarize(employeeEntries.Key, weekEntries.Key, weekEntries.Sum(entry => entry.Hours))))

private static WeeklySummary Summarize(string employee, (int Year, int Week) isoWeek, decimal totalHours) =>
    new(employee, isoWeek.Year, isoWeek.Week, totalHours,
        RegularHours: Math.Min(totalHours, RegularHoursPerWeek),
        OvertimeHours: Math.Max(totalHours - RegularHoursPerWeek, 0m));
```

(Both snippets are condensed onto fewer lines than in the source; the code is otherwise identical.)

## 4. Formatter: layout as data, culture and line endings decided once

**Smell.** Twelve `Append` calls each carried their own `\n`. Culture was handled per value: three `ToString("0.00", CultureInfo.InvariantCulture)` calls, while `IsoYear`, `IsoWeek` and `LineNumber` were interpolated without a format provider, so with the current culture. That is harmless for these integers today, but it is a half-applied rule that breaks the day someone adds a decimal column.

**Reason.** The line ending, the number format and the culture are presentation decisions, and each should be made in one place.

**Change.** The report is a collection expression of lines that reads top to bottom like the output. `\n` is added in one place. Each data line is built with `string.Create(CultureInfo.InvariantCulture, ...)`, with the format specifiers inline.

```csharp
// Before
report.Append("|---|---|---:|---:|---:|\n");
foreach (var summary in summaries)
{
    report.Append(
        $"| {summary.Employee} | {summary.IsoYear}-W{summary.IsoWeek:00} | " +
        $"{summary.TotalHours.ToString("0.00", CultureInfo.InvariantCulture)} | " +
        $"{summary.RegularHours.ToString("0.00", CultureInfo.InvariantCulture)} | " +
        $"{summary.OvertimeHours.ToString("0.00", CultureInfo.InvariantCulture)} |\n");
}

// After
List<string> lines =
[
    "# Overtime report",
    // ...
    "|---|---|---:|---:|---:|",
    .. summaries.Select(SummaryRow),
    "",
    "## Rejected rows",
    "",
    .. errors.Count == 0 ? ["None."] : errors.Select(ErrorLine),
];

return string.Concat(lines.Select(line => line + "\n"));

private static string SummaryRow(WeeklySummary summary) => string.Create(
    CultureInfo.InvariantCulture,
    $"| {summary.Employee} | {summary.IsoYear}-W{summary.IsoWeek:00} | {summary.TotalHours:0.00} | {summary.RegularHours:0.00} | {summary.OvertimeHours:0.00} |");
```

## Deliberately not changed

- **`TimesheetReportPipeline`** was already a four-line composition.
- **`Program.cs`** already did only I/O: resolve the path, check that the file exists, read it, write the report, and map the two failures to exit codes. Moving the default path or the error messages into the library would look tidier, but it would put shell concerns into the core.
- **Tests** were not touched during the refactor.

## Result

- `dotnet test`: `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 495 ms - Timesheets.Tests.dll (net9.0)`
- `dotnet build --no-incremental` on the solution: 0 warnings, 0 errors.
- Public API: unchanged; every new member is private. The console output for the sample is the same as before (LF line endings only), and the exit codes are the same.

## Not done — ideas for a further manual pass

1. **One definition of the column layout.** The parser knows the header names; the validator knows the field positions and the count. Adding a column touches both. Try a small shared layout definition, or let the parser map fields by header name so the column order stops mattering.
2. **Escape `|` in employee names** in the formatter. A name containing `|` breaks the table. The fix belongs in presentation. Rejecting such names in the validator would let the Markdown format dictate input rules.
3. **Make the threshold a parameter** when part-time contracts arrive. `RegularHoursPerWeek` is already the seam, for example `Calculate(entries, regularHoursPerWeek)` or an `OvertimePolicy` record. That is also when a static class may stop being the right shape.
4. **An `IsoWeek` value type.** `(int Year, int Week)` is a tuple in the calculator and two ints in `WeeklySummary`. Replace them with `readonly record struct IsoWeek(int Year, int Week)`. Then decide where the `2026-W05` label belongs; it is presentation, so probably still in the formatter.
5. **Stream the input.** With `Parse(IEnumerable<string> lines)`, the shell could pass `File.ReadLines(path)` for large files, and the core would still not know where the lines come from.

## Lessons learned

- Designing the units in the spec set the class boundaries before any code. Refactoring then happened at function level, where one method mixed two decisions or one decision was spread out.
- Pure units need no test doubles. All five tests call the real code with plain values. The only part of the environment the tests control is the culture, and it is pinned explicitly to `bs-Latn-BA`.
- Culture is a cross-cutting trap: an interpolated string silently uses the current culture. Keeping text-to-number conversions in two units made the rule easy to apply and to review, and even so the first-green formatter applied it only halfway.
- Two kinds of errors need two channels. Bad rows are data and flow through the pipeline into the report. A wrong file is an exception that only the shell turns into an exit code.
- The cost is real: ten types and five test classes for what a 15-line script does, and a validator that got longer when it got cleaner. It pays off only because each concern changes on its own schedule.
