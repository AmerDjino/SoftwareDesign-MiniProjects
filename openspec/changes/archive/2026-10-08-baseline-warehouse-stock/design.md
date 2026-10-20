# Design

## Context

`05-CqrsBasics` already implements and tests this behaviour (5 test methods, 6 test cases); this change records it as a spec and changes no code. See proposal.md for why. The original specification, including the write and read models, the projection, the hand-written dispatcher and decisions D-1 to D-7, is `05-CqrsBasics/docs/SPEC.md`, and the refactoring history is in `docs/REFACTOR-NOTES.md`.

## Goals / Non-Goals

**Goals:**
- A living `warehouse-stock` spec that later changes extend with ADDED or MODIFIED requirements.
- Every scenario traceable to the test that checks it, or listed as untested.

**Non-Goals:**
- Changing behaviour, or closing the test gaps listed below.
- Putting the CQRS structure (separate write and read models, the projection, the dispatcher) into the spec.

## Decisions

- **The CQRS split is design; its promises are behaviour.** Callers can observe that commands return only success or failure, that queries change nothing and that a query right after a command sees the change. Those promises are in the spec. How they are kept (a `Product` aggregate, a denormalised `StockLevelView`, a synchronous projection, query handlers that only see a read-only reader) is design and stays in `docs/SPEC.md` §5.
- **"Queries change nothing" is guarded structurally.** No test observes state before and after a query. Instead `ArchitectureTests.QueryHandlers_DependOnlyOnTheReadModel` checks that query handlers can only reach the read-only side, which makes a state change from a query impossible to write without failing that test.
- **Failure messages are part of the spec.** Their exact text is user-facing, and two tests assert it, so they are written into the requirements.

## Risks / Trade-offs

- [Two documents describe the same behaviour] → `docs/SPEC.md` is now a frozen record of the pre-code spec, and its header says so; only `openspec/specs/warehouse-stock/spec.md` changes from here on.
- [Four scenarios have no automated test: "Unregistered SKU" for receiving, the unregistered half of shipping, "Zero quantity" and an unregistered lookup; the SKU tie-break of the low-stock list is not tested either] → Listed in tasks.md, matching the gaps `docs/SPEC.md` §6 already named.
