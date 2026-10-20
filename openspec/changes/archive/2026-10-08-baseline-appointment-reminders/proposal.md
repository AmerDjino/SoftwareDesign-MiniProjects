# Proposal

## Why

The reminder behaviour of `02-DependencyInjection` is written down only in its `docs/SPEC.md`, which was written before the code and is kept as a record of that step. Later changes need a living spec to propose deltas against, so this change records the behaviour that the code and its tests already have.

## What Changes

- Add the `appointment-reminders` spec: which appointments are due, one reminder per appointment, the message format, what happens when sending fails, and what a host application can rely on when it sets up the reminder job (the default setup, one shared reminder history, and replaceable clock, appointment source and sender).
- Link the new spec from `02-DependencyInjection/README.md` and from the header of `02-DependencyInjection/docs/SPEC.md`.
- No code, test or behaviour changes.

## Capabilities

### New Capabilities
- `appointment-reminders`: reminds patients of appointments that start within the next 24 hours, once per appointment.

### Modified Capabilities

None.

## Impact

- Mini-project: `02-DependencyInjection` (documentation only).
- After archiving: `openspec/specs/appointment-reminders/spec.md`.
