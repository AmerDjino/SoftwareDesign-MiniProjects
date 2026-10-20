# 03 — Repository Pattern

A small billing module: invoices are created, become overdue after their due date and are marked as paid. Domain code works with an `IInvoiceRepository` whose methods use billing language (`ListOverdueAsync`, `GetByNumberAsync`), and it never sees EF Core: the `Invoicing` project has no package references, so persistence ignorance is enforced by the compiler. There are two implementations, in-memory and EF Core over SQLite, and one abstract contract test suite runs against both to prove they behave the same. Because the in-memory implementation behaves like a database (it stores and returns copies), the business-logic tests that use it can be trusted.

## Artifacts

| Artifact | Location |
|---|---|
| Specification | [docs/SPEC.md](docs/SPEC.md) |
| Unit tests | [tests/Invoicing.Tests](tests/Invoicing.Tests) |
| Implementation | [src/Invoicing](src/Invoicing) (domain and service), [src/Invoicing.Infrastructure](src/Invoicing.Infrastructure) (in-memory and EF Core repositories) |
| Refactor notes | [docs/REFACTOR-NOTES.md](docs/REFACTOR-NOTES.md) |
| Living specs (OpenSpec) | [invoice-payments](../openspec/specs/invoice-payments/spec.md), [invoice-storage](../openspec/specs/invoice-storage/spec.md) |

## How to run

From this folder:

```
dotnet test
```

There is no application to run; the tests are the entry point. The EF Core tests use SQLite in memory, so no database server or files are needed.

## Project layout

```
03-RepositoryPattern/
  RepositoryPattern.sln
  docs/
    SPEC.md
    REFACTOR-NOTES.md
  src/
    Invoicing/                          domain and service; no package references
      Invoice.cs
      IInvoiceRepository.cs
      InvoicePaymentService.cs
      PaymentResult.cs
    Invoicing.Infrastructure/           references Invoicing and EF Core (SQLite)
      InMemoryInvoiceStore.cs
      InMemoryInvoiceRepository.cs
      InvoiceSnapshot.cs
      InvoicingDbContext.cs
      EfInvoiceRepository.cs
  tests/
    Invoicing.Tests/
      InvoiceRepositoryContract.cs      3 contract tests, inherited by the two classes below
      InMemoryInvoiceRepositoryTests.cs
      EfInvoiceRepositoryTests.cs
      InvoicePaymentServiceTests.cs     2 service tests (one is a 2-case theory)
```

5 test methods, 9 test cases: the 3 contract tests run once per implementation, plus 1 + 2 service cases.

## Process log

| Step | What happened | `dotnet test` result |
|---|---|---|
| 1. Spec | `docs/SPEC.md` written before any code. | — |
| 2. Tests first | The 5 test methods were written against the spec's API while the `src` projects were still empty. `dotnet build`: `0 Warning(s)`, `18 Error(s)`, all "type or namespace not found" (CS0246, CS0234, CS0103). | does not compile |
| 2. API stubs | Signatures from the spec, bodies throw `NotImplementedException`. Every case fails; none passes against the stubs. | `Failed!  - Failed:     9, Passed:     0, Skipped:     0, Total:     9, Duration: 625 ms - Invoicing.Tests.dll (net9.0)` |
| 3. First implementation | Everything implemented, with the obvious in-memory fake that keeps the caller's own objects. One contract test fails, for the in-memory implementation only: the trap is caught. | `Failed!  - Failed:     1, Passed:     8, Skipped:     0, Total:     9, Duration: 7 s - Invoicing.Tests.dll (net9.0)` |
| 3. Experiment (reverted) | `UpdateAsync` call deleted from the service, with the naive fake. All 3 service cases still pass (a false green); the 1 failure is the same contract test. | `Failed!  - Failed:     1, Passed:     8, Skipped:     0, Total:     9, Duration: 1 s - Invoicing.Tests.dll (net9.0)` |
| 3. First green | The in-memory repository copies invoices on the way in and on the way out. | `Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 1 s - Invoicing.Tests.dll (net9.0)` |
| 3. Experiment (reverted) | The same `UpdateAsync` deletion, with the copying fake. Now `MarkPaidAsync_UnpaidInvoice_MarksItPaidAndPersistsIt` fails, as it should. | `Failed!  - Failed:     1, Passed:     8, Skipped:     0, Total:     9, Duration: 1 s - Invoicing.Tests.dll (net9.0)` |
| 4. Experiment (reverted) | Three of the five `Copy` calls (in `GetByNumberAsync`, `ListOverdueAsync` and `UpdateAsync`) deleted from the first-green fake. Still all green, so those calls were unguarded. Deleting only the `AddAsync` copy, or only the `GetByIdAsync` copy, fails AC-3. | `Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 7 s - Invoicing.Tests.dll (net9.0)` |
| 4. Refactor | Immutable snapshots in the in-memory store; one detached query root in the EF Core repository. Test code unchanged. | `Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 1 s - Invoicing.Tests.dll (net9.0)` |
| 4. Build | `dotnet build RepositoryPattern.sln --no-incremental` | `0 Warning(s)`, `0 Error(s)` |
| 5. OpenSpec baseline | Behaviour recorded as the [`invoice-payments`](../openspec/specs/invoice-payments/spec.md) and [`invoice-storage`](../openspec/specs/invoice-storage/spec.md) specs through the OpenSpec change `baseline-invoicing`: validated with `openspec validate --strict`, each scenario traced to its test, then archived. No code changed. | `Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 1 s - Invoicing.Tests.dll (net9.0)` |

## Key takeaways

- An in-memory fake has to behave like the database (copy in, copy out). The naive version passed 8 of 9 tests and hid a service that never saved.
- One abstract contract suite, run against every implementation, turns "swappable" into a checked claim. It caught the trap on the first run, but it only protects what it covers (see the ideas in the refactor notes).
- Persistence ignorance is enforced by the project graph, but EF Core still shaped the entity and forced the overdue rule to be written twice.
