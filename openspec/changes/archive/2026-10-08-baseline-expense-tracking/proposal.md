# Proposal

## Why

The expense API of `04-LayeredArchitecture` is written down only in its `docs/SPEC.md`, which was written before the code and is kept as a record of that step. Later changes, such as adding `GET /expenses/{id}`, need a living spec to propose deltas against, so this change records the behaviour that the code and its tests already have.

## What Changes

- Add the `expense-tracking` spec: submitting an expense over HTTP, the validation rules and the validation response, the monthly summary, and the summary route's month range.
- Link the new spec from `04-LayeredArchitecture/README.md` and from the header of `04-LayeredArchitecture/docs/SPEC.md`.
- No code, test or behaviour changes.

## Capabilities

### New Capabilities
- `expense-tracking`: an HTTP API for recording employee expenses and reporting monthly totals per category.

### Modified Capabilities

None.

## Impact

- Mini-project: `04-LayeredArchitecture` (documentation only).
- After archiving: `openspec/specs/expense-tracking/spec.md`.
