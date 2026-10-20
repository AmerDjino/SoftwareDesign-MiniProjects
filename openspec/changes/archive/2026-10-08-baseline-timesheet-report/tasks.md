# Tasks

## 1. Specify the existing behaviour

- [x] 1.1 Turn FR-1 to FR-14 of `06-SeparationOfConcerns/docs/SPEC.md` into requirements and scenarios in `specs/timesheet-report/spec.md`; verify with `openspec validate baseline-timesheet-report --strict`

## 2. Trace every scenario to a test

- [x] 2.1 "Header in another letter case" and "Blank lines and padded fields": verified by `TimesheetCsvParserTests.Parse_SkipsHeaderAndBlankLines_AndKeepsPhysicalLineNumbers`
- [x] 2.2 The six "Row validation" scenarios: verified by `TimesheetValidatorTests.Validate_InvalidRows_AreRejectedWithLineNumberAndReason`
- [x] 2.3 "Week across New Year": verified by `OvertimeCalculatorTests.Calculate_HoursAbove40PerIsoWeek_AreOvertime`
- [x] 2.4 "Summaries and rejected rows": verified by `MarkdownReportFormatterTests.Format_RendersSummaryTableAndRejectedRows`
- [x] 2.5 "Bosnian regional settings": verified by `TimesheetReportPipelineTests.Run_CsvWithValidAndInvalidRows_ProducesCompleteReport`
- [x] 2.6 "First line is not the header", "No rejected rows", "Bundled sample", "Missing file" and "File without the header": verified by running the command-line tool with the bundled sample, a missing path, a file without the header and a file with only valid rows, and checking the output and exit codes
- [x] 2.7 Run `dotnet test 06-SeparationOfConcerns/SeparationOfConcerns.sln` and verify that all 5 test cases pass

## 3. Point readers to the living spec

- [x] 3.1 Link `openspec/specs/timesheet-report/spec.md` from `06-SeparationOfConcerns/README.md` and from the header of `06-SeparationOfConcerns/docs/SPEC.md`; verify that every link points to the spec path that archiving creates
