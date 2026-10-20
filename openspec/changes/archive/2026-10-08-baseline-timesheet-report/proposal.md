# Proposal

## Why

The timesheet report of `06-SeparationOfConcerns` is written down only in its `docs/SPEC.md`, which was written before the code and is kept as a record of that step. Later changes, such as escaping `|` in employee names, need a living spec to propose deltas against, so this change records the behaviour that the code and its tests already have.

## What Changes

- Add the `timesheet-report` spec: the input header, how rows are read, row validation with its exact messages, the weekly overtime rule, the report layout, culture independence and the command-line tool's exit codes.
- Link the new spec from `06-SeparationOfConcerns/README.md` and from the header of `06-SeparationOfConcerns/docs/SPEC.md`.
- No code, test or behaviour changes.

## Capabilities

### New Capabilities
- `timesheet-report`: turns a time-tracking CSV export into a Markdown overtime report per employee and ISO week.

### Modified Capabilities

None.

## Impact

- Mini-project: `06-SeparationOfConcerns` (documentation only).
- After archiving: `openspec/specs/timesheet-report/spec.md`.
