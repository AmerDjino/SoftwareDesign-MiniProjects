# Proposal

## Why

The stock-keeping behaviour of `05-CqrsBasics` is written down only in its `docs/SPEC.md`, which was written before the code and is kept as a record of that step. Later changes, such as giving failures a code, need a living spec to propose deltas against, so this change records the behaviour that the code and its tests already have.

## What Changes

- Add the `warehouse-stock` spec: registering products, receiving and shipping stock with their failure messages, what a command reports, malformed commands, the stock-level lookup, the low-stock list, and queries that change nothing.
- Link the new spec from `05-CqrsBasics/README.md` and from the header of `05-CqrsBasics/docs/SPEC.md`.
- No code, test or behaviour changes.

## Capabilities

### New Capabilities
- `warehouse-stock`: keeps stock per product and answers the stock-lookup and reorder screens.

### Modified Capabilities

None.

## Impact

- Mini-project: `05-CqrsBasics` (documentation only).
- After archiving: `openspec/specs/warehouse-stock/spec.md`.
