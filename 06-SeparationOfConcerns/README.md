# 06 — Separation of Concerns

Separation of Concerns means that each part of a program deals with one concern and does not know how the other parts do their work. This mini-project turns a weekly CSV export from a time-tracking tool into a Markdown overtime report for payroll. For each employee and ISO week the report shows total, regular (up to 40 h) and overtime hours, followed by the rows that were rejected and why. Six concerns each live in their own unit: reading the file, parsing CSV, validating rows, calculating overtime, rendering Markdown and running the steps in order. Everything except the console shell is a pure function, so the tests need no files and no test doubles.

## Artifacts

| Artifact | Location |
|---|---|
| Specification | [docs/SPEC.md](docs/SPEC.md) |
| Unit tests | [tests/Timesheets.Tests](tests/Timesheets.Tests) |
| Implementation | [src/Timesheets](src/Timesheets) (pure core), [src/Timesheets.ConsoleApp](src/Timesheets.ConsoleApp) (I/O shell) |
| Refactor notes | [docs/REFACTOR-NOTES.md](docs/REFACTOR-NOTES.md) |
| Sample input | [samples/timesheet-sample.csv](samples/timesheet-sample.csv) |
| Living spec (OpenSpec) | [openspec/specs/timesheet-report/spec.md](../openspec/specs/timesheet-report/spec.md) |

## How to run

From this folder:

```bash
dotnet test
dotnet run --project src/Timesheets.ConsoleApp                      # bundled sample
dotnet run --project src/Timesheets.ConsoleApp -- path/to/export.csv
```

Exit codes: `0` report written to stdout; `1` file not found; `2` not a timesheet export (line 1 is not the header). Messages go to stderr.

Output for the bundled sample (real run, exit code 0):

```text
# Overtime report

## Weekly hours

| Employee | Week | Total | Regular | Overtime |
|---|---|---:|---:|---:|
| Ana Kovac | 2026-W39 | 40.00 | 40.00 | 0.00 |
| Ana Kovac | 2026-W40 | 42.50 | 40.00 | 2.50 |
| Emir Hadzic | 2026-W39 | 44.75 | 40.00 | 4.75 |
| Emir Hadzic | 2026-W40 | 31.00 | 31.00 | 0.00 |
| Lejla Begic | 2026-W39 | 24.00 | 24.00 | 0.00 |
| Lejla Begic | 2026-W40 | 12.00 | 12.00 | 0.00 |

## Rejected rows

- Line 24: Hours '60' must be greater than 0 and at most 24.
- Line 26: Expected 3 fields but found 4.
- Line 27: Date '01.10.2026' is not a valid yyyy-MM-dd date.
```

Line 26 is `Emir Hadzic,2026-10-01,7,5`: a decimal comma turns one field into two. The week 2026-W40 runs from Monday 28 September to Sunday 4 October, so it spans two months.

Manual checks of the shell, which has no unit test by design: a missing file gives exit code 1 and `File not found: missing.csv` on stderr. A file without the header gives exit code 2 and `Not a timesheet export: Line 1 must be the header 'employee,date,hours'.` A UTF-8 file with a byte-order mark and no invalid rows gives exit code 0 and `None.` under "Rejected rows".

## Project layout

```text
06-SeparationOfConcerns/
├── docs/                          SPEC.md, REFACTOR-NOTES.md
├── samples/timesheet-sample.csv
├── src/
│   ├── Timesheets/                pure core, no I/O
│   │   ├── TimesheetCsvParser.cs       parsing        -> RawRow
│   │   ├── TimesheetValidator.cs       validation     -> TimeEntry, ValidationError, ValidationResult
│   │   ├── OvertimeCalculator.cs       calculation    -> WeeklySummary
│   │   ├── MarkdownReportFormatter.cs  presentation   -> report text
│   │   ├── TimesheetReportPipeline.cs  orchestration
│   │   └── RawRow.cs, TimeEntry.cs, ValidationError.cs, ValidationResult.cs, WeeklySummary.cs
│   └── Timesheets.ConsoleApp/
│       └── Program.cs             humble shell: file in, stdout/stderr out, exit codes
└── tests/Timesheets.Tests/        one test class per unit, plus the CultureScope helper
```

## Process log

| Step | What happened | `dotnet test` result |
|---|---|---|
| 1. Specification | `docs/SPEC.md` written before any code: rules with exact numbers and messages, the exact report layout, the public API, and the acceptance criteria mapped to tests. | — |
| 2a. Tests first | Empty library, test project, solution and the five tests against the spec's API. `dotnet build`: `Build FAILED.`, `1 Error(s)` (CS0246: `RawRow` not found). The compiler stops after errors in member signatures, so errors inside the test bodies were not listed yet. | does not compile |
| 2b. Stubs (red) | Records written out; the five static methods `throw new NotImplementedException()`. Build clean; all five tests fail with `NotImplementedException`. | `Failed!  - Failed:     5, Passed:     0, Skipped:     0, Total:     5, Duration: 269 ms - Timesheets.Tests.dll (net9.0)` |
| 3. First green | Straightforward implementation of each unit, then the console app and the sample. Console app checked by hand (see above). | `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 149 ms - Timesheets.Tests.dll (net9.0)` |
| 4. Refactor | Four refactorings inside the parser, validator, calculator and formatter; tests unchanged; same console output. `dotnet build --no-incremental`: 0 warnings, 0 errors. | `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 495 ms - Timesheets.Tests.dll (net9.0)` |
| 5. OpenSpec baseline | Behaviour recorded as the [`timesheet-report`](../openspec/specs/timesheet-report/spec.md) spec through the OpenSpec change `baseline-timesheet-report`: validated with `openspec validate --strict`, each scenario traced to a test or a manual check, then archived. The console app was run again: exit codes 0, 1 and 2, and `None.` when no row is rejected. No code changed. | `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 220 ms - Timesheets.Tests.dll (net9.0)` |

## Key takeaways

- A useful test for a concern is whether it changes for its own reason. Trimming belongs to parsing, the 24-hour limit to validation, the 40-hour rule to calculation, and `42.50` to presentation.
- With all I/O in a thin shell, the core is values in, values out: five tests, no mocks, no files. The one piece of environment that matters, the culture, is pinned in the pipeline test (`bs-Latn-BA`, where `7.5` would otherwise parse as 75).
- SoC is not free: ten small types do the job of a 15-line script. It is worth it when input rules, payroll rules and the report layout change independently, and not for a one-off script.
