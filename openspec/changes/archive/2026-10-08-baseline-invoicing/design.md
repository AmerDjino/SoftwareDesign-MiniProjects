# Design

## Context

`03-RepositoryPattern` already implements and tests this behaviour (5 test methods, 9 test cases: three contract tests run against both stores, plus the payment tests); this change records it as specs and changes no code. See proposal.md for why. The original specification, including the repository design and decisions D1 to D11, is `03-RepositoryPattern/docs/SPEC.md`, and the refactoring history is in `docs/REFACTOR-NOTES.md`.

## Goals / Non-Goals

**Goals:**
- Living `invoice-payments` and `invoice-storage` specs that later changes extend with ADDED or MODIFIED requirements.
- Every scenario traceable to the test that checks it, or listed as untested.

**Non-Goals:**
- Changing behaviour, or closing the test gaps listed below.
- Specifying what happens with a duplicate invoice number or an update of an invoice that was never added. The two stores differ there today (see Risks), so a requirement would describe behaviour that one store does not have.

## Decisions

- **Two capabilities.** Payments (business rules) and storage (the contract every store meets) change for different reasons and have different consumers: the billing team relies on the first, the payment code and future store implementations on the second.
- **The storage contract is behaviour, the repository pattern is design.** The questions storage answers, and its persistence semantics (save immediately, return detached copies), are what the payment code relies on, so they are in the spec. The repository interface, the snapshot type in the in-memory store and the EF Core mapping are design and stay in `docs/SPEC.md` §5 and `docs/REFACTOR-NOTES.md`.
- **"Storage session" instead of class names.** Scenarios say "a new storage session" where the tests create a fresh repository over the same store (a new `DbContext`, or a new in-memory repository over the same backing store). That keeps the spec free of implementation names while keeping the point: tests prove what was stored, not which objects happen to be in memory.

## Risks / Trade-offs

- [Two documents describe the same behaviour] → `docs/SPEC.md` is now a frozen record of the pre-code spec, and its header says so; only `openspec/specs/` changes from here on.
- [The error contract is unspecified: for a duplicate number the database store throws and the in-memory store accepts it; updating an unknown invoice makes the database store throw and the in-memory store insert it] → Idea 2 in `docs/REFACTOR-NOTES.md` is the follow-up: write the contract tests first, then make both stores behave the same, as a change with ADDED requirements on `invoice-storage`.
- ["Unknown number" is checked only for the in-memory store, through the payment test for `INV-404`] → Listed in tasks.md; adding it to the contract tests would cover the database store too.
