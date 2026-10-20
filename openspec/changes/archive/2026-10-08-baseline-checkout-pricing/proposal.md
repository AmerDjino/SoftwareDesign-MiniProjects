# Proposal

## Why

The checkout-pricing behaviour of `01-SolidPrinciples` is written down only in its `docs/SPEC.md`, which was written before the code and is kept as a record of that step. Later changes need a living spec to propose deltas against, so this change records the behaviour that the code and its tests already have.

## What Changes

- Add the `checkout-pricing` spec: the subtotal, the three built-in discount rules, how discounts stack and are capped, money rounding, and the contract every discount rule follows.
- Link the new spec from `01-SolidPrinciples/README.md` and from the header of `01-SolidPrinciples/docs/SPEC.md`.
- No code, test or behaviour changes.

## Capabilities

### New Capabilities
- `checkout-pricing`: prices an order with stackable discount rules and explains every discount in a price breakdown.

### Modified Capabilities

None.

## Impact

- Mini-project: `01-SolidPrinciples` (documentation only).
- After archiving: `openspec/specs/checkout-pricing/spec.md`.
