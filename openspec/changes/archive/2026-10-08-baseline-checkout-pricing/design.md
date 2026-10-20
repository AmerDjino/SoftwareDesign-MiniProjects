# Design

## Context

`01-SolidPrinciples` already implements and tests this behaviour (5 test methods, 21 test cases); this change records it as a spec and changes no code. See proposal.md for why. The original specification, including how each SOLID principle shapes the code, is `01-SolidPrinciples/docs/SPEC.md`, and the refactoring history is in `docs/REFACTOR-NOTES.md`.

## Goals / Non-Goals

**Goals:**
- A living `checkout-pricing` spec that later changes extend with ADDED or MODIFIED requirements.
- Every scenario traceable to the test that checks it, or listed as untested.

**Non-Goals:**
- Changing behaviour, or closing the test gaps listed below; that is new work for its own change.
- Describing the SOLID design in the spec.

## Decisions

- **Behaviour in the spec, SOLID in the design documents.** The spec states what a caller of the pricing component can observe. How the principles shape the code (the calculator only orchestrates, one class per rule, rules behind one interface, the coupon rule depending on a read-only coupon lookup) stays in `docs/SPEC.md` §5. Alternative considered: a requirement per principle. Rejected, because the spec would then change with every refactoring even when no behaviour changes.
- **The rule contract and extensibility are behaviour.** Developers who write their own discount rules are users of the pricing component, so the promises their rules must keep (`Discount rule contract`) and the fact that their rules are applied like built-in ones (`Additional discount rules`) belong in the spec.
- **Scenario values come from the tests.** Scenarios reuse the tests' worked examples, so each one can be checked against a test line by line (see tasks.md, group 2).

## Risks / Trade-offs

- [Two documents describe the same behaviour] → `docs/SPEC.md` is now a frozen record of the pre-code spec, and its header says so; only `openspec/specs/checkout-pricing/spec.md` changes from here on.
- [Four scenarios have no dedicated test: "No lines", "Silver customer", "Coupon larger than the subtotal" (the exact amount) and "Midpoint"] → They are listed in tasks.md. Three of them match idea 3 in `docs/REFACTOR-NOTES.md`, which makes a natural first change on top of this baseline.
