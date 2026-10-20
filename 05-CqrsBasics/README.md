# 05 — CQRS basics

CQRS (Command Query Responsibility Segregation) gives state changes and reads separate paths and separate models. This mini-project applies it to warehouse stock. Commands (`RegisterProduct`, `ReceiveStock`, `ShipStock`) change a `Product` aggregate that enforces the rules, and return only success or failure. Queries (`GetStockLevel`, `GetLowStockProducts`) read flat `StockLevelView` rows shaped for two screens and never change anything. A projection updates the read model after every successful command, synchronously and in process. Instead of MediatR, a small hand-written dispatcher on top of `Microsoft.Extensions.DependencyInjection` routes each message to its handler.

## Artifacts

| Artifact | Location |
|---|---|
| Specification | [docs/SPEC.md](docs/SPEC.md) |
| Unit tests | [tests/Inventory.Tests](tests/Inventory.Tests) |
| Implementation | [src/Inventory](src/Inventory) |
| Refactor notes | [docs/REFACTOR-NOTES.md](docs/REFACTOR-NOTES.md) |
| Living spec (OpenSpec) | [openspec/specs/warehouse-stock/spec.md](../openspec/specs/warehouse-stock/spec.md) |

## How to run

From this folder:

```
dotnet test
```

There is no app to run. The tests drive the module through `IDispatcher` and the real DI container.

## Project layout

```
05-CqrsBasics/
  CqrsBasics.sln
  docs/
    SPEC.md                 specification, written before the code
    REFACTOR-NOTES.md       first green, refactorings, ideas for a manual pass
  src/Inventory/
    Abstractions/           ICommand, IQuery<T>, handler interfaces, CommandResult, IDispatcher
    Dispatching/            Dispatcher, QueryHandlerInvoker
    WriteSide/              commands and handlers, Product, product repository, StockLevelProjection, ProductErrors
    ReadSide/               queries and handlers, StockLevelView, reader and writer interfaces, in-memory store
    ServiceCollectionExtensions.cs   AddInventory()
  tests/Inventory.Tests/
    StockScenarioTests.cs   four scenarios through the dispatcher and the real container
    ArchitectureTests.cs    query handlers depend only on the read model
```

## Process log

| Step | What happened | `dotnet test` result |
|---|---|---|
| 1. Spec | `docs/SPEC.md` written first: the concept, requirements with exact rules and messages, the API, five acceptance criteria. | — |
| 2. Tests first | Five test methods (six cases; one theory has two rows) written against the spec's API, with an empty library. `dotnet build` failed with 5 errors, all CS0234: the namespaces `Inventory.Abstractions`, `.ReadSide` and `.WriteSide` did not exist yet. The compiler stops at the using directives, so the missing types are not listed one by one. | Build only: `Build FAILED.` `0 Warning(s)` `5 Error(s)` |
| 3. Stubs (red) | Every signature from the spec, with bodies that throw `NotImplementedException`. The five scenario cases failed inside the `AddInventory` stub, before any assertion. `QueryHandlers_DependOnlyOnTheReadModel` passed: it checks constructor signatures, which the stubs already had. | `Failed!  - Failed:     5, Passed:     1, Skipped:     0, Total:     6, Duration: 823 ms - Inventory.Tests.dll (net9.0)` |
| 4. First green | Straightforward implementation: query dispatch through `dynamic`, failure messages inline. Green on the first run. | `Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 734 ms - Inventory.Tests.dll (net9.0)` |
| 5. Validate | Temporary changes, each reverted. (a) A query handler given `IProductRepository` made the architecture test fail, so it can catch a violation. (b) A handler implementing `HandleAsync` explicitly broke the `dynamic` dispatcher at runtime with `RuntimeBinderException`. | (a) `Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 90 ms - Inventory.Tests.dll (net9.0)`<br>(b) `Failed!  - Failed:     4, Passed:     2, Skipped:     0, Total:     6, Duration: 233 ms - Inventory.Tests.dll (net9.0)` |
| 6. Refactor | A typed, cached query invoker instead of `dynamic`; failure messages in one place; a comment on the shared read-store registration. Check (b) now passes (`Passed: 6`). Splitting the read store into two singletons, tried temporarily, fails all five scenario cases. Test code unchanged; `dotnet build`: 0 warnings, 0 errors. | `Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 947 ms - Inventory.Tests.dll (net9.0)` |
| 7. OpenSpec baseline | Behaviour recorded as the [`warehouse-stock`](../openspec/specs/warehouse-stock/spec.md) spec through the OpenSpec change `baseline-warehouse-stock`: validated with `openspec validate --strict`, each scenario traced to its test, then archived. No code changed. | `Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 210 ms - Inventory.Tests.dll (net9.0)` |
| 8. Follow-up refactor | The in-memory product store keeps immutable snapshots and hands out detached copies, like a database (OpenSpec change `detach-stored-products`, no spec change; see the follow-up section in `docs/REFACTOR-NOTES.md`). A temporary test proved the old store leaked unsaved changes (`Failed: 1, Passed: 6`) and passed after the change (`Passed: 7`); it was then deleted. Existing tests unchanged; full rebuild: 0 warnings, 0 errors. | `Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 73 ms - Inventory.Tests.dll (net9.0)` |

## Key takeaways

- Commands return only a `CommandResult`; queries return data and change nothing. `RegisterProduct` does not return the product, because the caller chose the SKU and can query it.
- The read side is thin because the projection does the work at write time: `IsLowStock` is stored, not computed per query.
- The dispatcher and its invoker are about 70 lines. Their only reflection recovers a query's runtime type, once per type, so that callers get a typed result from `QueryAsync` without naming it.
- Both models live in one process and are updated together. Separate stores with asynchronous projections, and the eventual consistency that comes with them, are the next step rather than part of the definition, and CQRS is not event sourcing.
