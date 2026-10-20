# Design

## Context

`04-LayeredArchitecture` already implements and tests this behaviour (5 test methods, 7 test cases); this change records it as a spec and changes no code. See proposal.md for why. The original specification, including the layer rules AR-1 to AR-4, the mapping at each boundary and decisions D-1 to D-12, is `04-LayeredArchitecture/docs/SPEC.md`, and the refactoring history is in `docs/REFACTOR-NOTES.md`.

## Goals / Non-Goals

**Goals:**
- A living `expense-tracking` spec that later changes extend with ADDED or MODIFIED requirements.
- Every scenario traceable to a test, to a manual check of the running API, or listed as untested.

**Non-Goals:**
- Changing behaviour, or closing the test gaps listed below.
- Putting the layer rules into the spec.

## Decisions

- **Layer rules are design, guarded by tests.** "Presentation never references Data Access" (AR-1) and "no layer references a layer above it" (AR-2) can be broken without any change a client could see, so by OpenSpec's own test they are design, not spec. They stay in `docs/SPEC.md` §4 and §7, and two architecture tests keep them honest (tasks.md, task 2.6). The same goes for AR-3 (one DI entry point per layer) and AR-4 (each layer owns its models).
- **One requirement per validation rule.** Each field's rule is its own requirement, so a later change can modify one limit (for example the maximum amount) with a MODIFIED delta without rewriting the others.
- **Scenarios at the level the tests work on.** Some scenarios are checked over HTTP (the handler test), others in the Business layer (the service tests). The spec describes the API either way, because the handler only maps between the two.

## Risks / Trade-offs

- [Two documents describe the same behaviour] → `docs/SPEC.md` is now a frozen record of the pre-code spec, and its header says so; only `openspec/specs/expense-tracking/spec.md` changes from here on.
- [Four scenarios are checked only by calling the running API, not by an automated test: "Valid expense", "Combined category names", "Month 13" and "Month without expenses"] → Listed in tasks.md (task 2.4). An HTTP-level test with `WebApplicationFactory` would automate all four.
