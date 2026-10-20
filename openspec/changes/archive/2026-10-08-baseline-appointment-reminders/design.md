# Design

## Context

`02-DependencyInjection` already implements and tests this behaviour (5 test methods, 5 test cases); this change records it as a spec and changes no code. See proposal.md for why. The original specification, including the lifetimes table and the reasoning about what to inject, is `02-DependencyInjection/docs/SPEC.md`, and the refactoring history is in `docs/REFACTOR-NOTES.md`.

## Goals / Non-Goals

**Goals:**
- A living `appointment-reminders` spec that later changes extend with ADDED or MODIFIED requirements.
- Every scenario traceable to the test that checks it, or listed as untested.

**Non-Goals:**
- Changing behaviour, or closing the test gaps listed below.
- Describing the dependency-injection design itself (constructor injection, which dependencies are abstractions, the composition root) in the spec.

## Decisions

- **Two audiences, one capability.** Patients see the due rule, the message and the no-repeat promise. Developers hosting the job see the default setup, the shared history and the replaceable dependencies. Both are observable behaviour of the library, so both are in the spec. How the job gets its dependencies (constructor injection, a pure message formatter that is deliberately not injected) is design and stays in `docs/SPEC.md` §5 and `docs/REFACTOR-NOTES.md`.
- **Lifetimes appear only as behaviour.** The spec promises one shared history and one job per run; the singleton and scoped registrations that deliver this stay in the design documents. Alternative considered: listing every lifetime in the spec. Rejected, because a lifetime can change (for example a singleton sender becoming transient) without any observable difference.
- **Scenario values come from the tests**, so each scenario can be checked against a test line by line (see tasks.md, group 2).

## Risks / Trade-offs

- [Two documents describe the same behaviour] → `docs/SPEC.md` is now a frozen record of the pre-code spec, and its header says so; only `openspec/specs/appointment-reminders/spec.md` changes from here on.
- [Two scenarios have no automated test: "Sending fails halfway" and "Replacement registered before the defaults"] → The second was checked in a throwaway experiment during the refactor (see `docs/REFACTOR-NOTES.md`, step 4); idea 1 in the same file turns it into a test.
