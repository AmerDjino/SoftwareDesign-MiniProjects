## Purpose

Turns a weekly time-tracking CSV export into a Markdown overtime report for payroll: hours per employee and ISO week, split into regular and overtime hours, plus the rows that could not be used and why.

## ADDED Requirements

### Requirement: Input header
The first line of the input SHALL be the header `employee,date,hours`, compared field by field after trimming and ignoring letter case. Otherwise the whole input SHALL be rejected with `Line 1 must be the header 'employee,date,hours'.`

#### Scenario: Header in another letter case
- **WHEN** the first line is `Employee,Date,Hours`
- **THEN** the input is accepted

#### Scenario: First line is not the header
- **WHEN** the first line is anything other than the header
- **THEN** the input is rejected with `Line 1 must be the header 'employee,date,hours'.`

### Requirement: Reading rows
The input SHALL be accepted with LF or CRLF line breaks. Lines SHALL be numbered by physical position, starting with the header as line 1, and blank or whitespace-only lines SHALL be skipped but counted. Every other line SHALL be split on commas, each field trimmed and not yet interpreted.

#### Scenario: Blank lines and padded fields
- **GIVEN** CRLF input with a data row on line 2, an empty line 3, a whitespace-only line 4, ` Emir Hadzic , 2026-09-28 , 7 ` on line 5 and a row with only two fields on line 6
- **WHEN** the rows are read
- **THEN** there are three rows, on lines 2, 5 and 6, with trimmed fields
- **AND** the two-field row is passed on to validation as it is

### Requirement: Row validation
Each row SHALL be checked against these rules in order: exactly 3 fields; an employee name; a date in the format `yyyy-MM-dd`; hours written as a number with `.` as the decimal separator; hours greater than 0 and at most 24. The first rule a row breaks SHALL be its only error, reported with its line number, and invalid rows SHALL be left out of the calculation.

#### Scenario: Wrong number of fields
- **WHEN** a row has only two fields
- **THEN** its error is `Expected 3 fields but found 2.`

#### Scenario: Several problems in one row
- **WHEN** a row has no employee name, the date `2026-13-01` and the hours `eight`
- **THEN** its only error is `Employee is missing.`

#### Scenario: Date in another format
- **WHEN** a row has the date `29.09.2026`
- **THEN** its error is `Date '29.09.2026' is not a valid yyyy-MM-dd date.`

#### Scenario: Hours not a number
- **WHEN** a row has the hours `eight`
- **THEN** its error is `Hours 'eight' is not a number.`

#### Scenario: Hours out of range
- **WHEN** a row has the hours `0` or `24.5`
- **THEN** its error is `Hours '0' must be greater than 0 and at most 24.` or `Hours '24.5' must be greater than 0 and at most 24.`

#### Scenario: Valid rows up to 24 hours
- **WHEN** the rows `Ana Kovac,2026-09-28,8.5` and `Emir Hadzic,2026-10-02,24` are validated
- **THEN** both become time entries with exactly those values

### Requirement: Weekly overtime
Valid entries SHALL be grouped by employee and ISO 8601 week (ISO year and week number), with employee names matched ignoring letter case and shown as first spelled. Per group, the total SHALL be the exact sum of the hours, regular hours the total up to 40, and overtime the rest. Groups SHALL be ordered by employee (ordinal, ignoring case), then ISO year, then week.

#### Scenario: Week across New Year
- **GIVEN** Ana Kovac works 10 hours on each of 2025-12-29, 2025-12-30 and 2025-12-31 (the last spelled `ana kovac`), 12.5 hours on 2026-01-02 and 7.5 hours on 2026-01-05
- **AND** Lejla Begic works 6 hours on 2026-01-06
- **WHEN** the weekly totals are calculated
- **THEN** the result is, in this order: Ana Kovac 2026-W01 with 42.5 total, 40 regular and 2.5 overtime; Ana Kovac 2026-W02 with 7.5, 7.5 and 0; Lejla Begic 2026-W02 with 6, 6 and 0

### Requirement: Report layout
The report SHALL be Markdown: the title `# Overtime report`, a `## Weekly hours` table with the columns Employee, Week, Total, Regular and Overtime, and a `## Rejected rows` section with one `- Line {n}: {message}` line per error, or `None.` when there are none. Weeks SHALL be written `{ISO year}-W{two-digit week}`, hours with two decimals and `.`, and every line SHALL end with `\n`.

#### Scenario: Summaries and rejected rows
- **GIVEN** the summaries Ana Kovac 2026-W05 (42.5 total, 40 regular, 2.5 overtime) and Emir Hadzic 2026-W40 (38, 38, 0), and errors on lines 4 and 7
- **WHEN** the report is written
- **THEN** the table rows are `| Ana Kovac | 2026-W05 | 42.50 | 40.00 | 2.50 |` and `| Emir Hadzic | 2026-W40 | 38.00 | 38.00 | 0.00 |`
- **AND** the rejected rows are `- Line 4: Expected 3 fields but found 4.` and `- Line 7: Hours '26' must be greater than 0 and at most 24.`

#### Scenario: No rejected rows
- **WHEN** a report has no errors
- **THEN** the `## Rejected rows` section holds the single line `None.`

### Requirement: Same report on every machine
The report SHALL be identical whatever the machine's regional settings, including settings that use a decimal comma.

#### Scenario: Bosnian regional settings
- **GIVEN** the machine uses the `bs-Latn-BA` culture, in which `7.5` would normally be read as 75
- **WHEN** a CSV with 42.5 hours for Ana Kovac in 2026-W40 (one row is 9.5 hours), 8 valid hours for Emir Hadzic and two invalid rows on lines 8 and 9 is turned into a report
- **THEN** the report shows 42.50, 40.00 and 2.50 for Ana Kovac and 8.00, 8.00 and 0.00 for Emir Hadzic
- **AND** it lists the invalid rows on lines 8 and 9

### Requirement: Command-line tool
The command-line tool SHALL read the CSV file named by its first argument, or the bundled sample when there is none, write the report to standard output and exit with code 0. A missing file SHALL exit with code 1 and an input without the header with code 2, each with a message on standard error.

#### Scenario: Bundled sample
- **WHEN** the tool runs without arguments
- **THEN** it writes the report for the bundled sample and exits with code 0

#### Scenario: Missing file
- **WHEN** the tool runs with a path that does not exist
- **THEN** it writes `File not found: {path}` to standard error and exits with code 1

#### Scenario: File without the header
- **WHEN** the tool runs with a file whose first line is not the header
- **THEN** it writes `Not a timesheet export: Line 1 must be the header 'employee,date,hours'.` to standard error and exits with code 2
