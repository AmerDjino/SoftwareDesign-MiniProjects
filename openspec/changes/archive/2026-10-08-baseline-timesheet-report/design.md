# Design

## Context

`06-SeparationOfConcerns` already implements and tests this behaviour (5 test methods, 5 test cases); this change records it as a spec and changes no code. See proposal.md for why. The original specification, including the table of concerns, the pure core and the I/O shell, is `06-SeparationOfConcerns/docs/SPEC.md`, and the refactoring history is in `docs/REFACTOR-NOTES.md`.

## Goals / Non-Goals

**Goals:**
- A living `timesheet-report` spec that later changes extend with ADDED or MODIFIED requirements.
- Every scenario traceable to a test, to a manual run of the command-line tool, or listed as untested.

**Non-Goals:**
- Changing behaviour, or closing the test gaps listed below.
- Putting the separation of concerns (parser, validator, calculator, formatter, pipeline, shell) into the spec.

## Decisions

- **Requirements follow the data, not the classes.** The requirements read like the path a CSV takes (header, rows, validation, overtime, layout), so they line up with the concerns without naming them. Which unit owns which rule is design and stays in `docs/SPEC.md` §5; a refactoring that moves a rule between units leaves the spec untouched.
- **The command-line tool is in the spec.** Payroll staff run it, so its arguments, output and exit codes are observable behaviour, even though it is deliberately a thin shell without unit tests.
- **Exact text where a user sees text.** Validation messages and the report layout are copied exactly, because the tests assert them exactly and payroll reads them.

## Risks / Trade-offs

- [Two documents describe the same behaviour] → `docs/SPEC.md` is now a frozen record of the pre-code spec, and its header says so; only `openspec/specs/timesheet-report/spec.md` changes from here on.
- [Five scenarios have no automated test: "First line is not the header", "No rejected rows" and the three command-line scenarios] → They are checked by running the tool (tasks.md, task 2.6). `docs/SPEC.md` §6 already lists them as outside the five-test budget.
