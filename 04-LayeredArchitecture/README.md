# 04 — Layered Architecture

Employee expense tracking (submit an expense, get a monthly total per category) as an ASP.NET Core minimal API
built in three strict layers: **Presentation** (HTTP) → **Business** (rules) → **Data Access** (storage). Each
layer depends only on the layer directly below it, owns its own models and maps them at the boundary, and has one
DI entry point: `AddBusinessLayer()` calls `AddDataAccessLayer()`, so the web project never touches data-access
types. Unit tests enforce the dependency rules by reflecting over the compiled assemblies. The spec also contrasts
this classic style with Clean/Onion architecture.

## Artifacts

| Artifact | Where |
|---|---|
| Specification | [docs/SPEC.md](docs/SPEC.md) |
| Unit tests | [tests/Expenses.Tests/](tests/Expenses.Tests/): 5 test methods, 7 test cases |
| Implementation | [src/](src/): [Presentation](src/Expenses.Presentation/), [Business](src/Expenses.Business/), [DataAccess](src/Expenses.DataAccess/) |
| Refactor notes | [docs/REFACTOR-NOTES.md](docs/REFACTOR-NOTES.md) |
| Living spec (OpenSpec) | [openspec/specs/expense-tracking/spec.md](../openspec/specs/expense-tracking/spec.md) |

## How to run

```powershell
dotnet test
dotnet run --project src/Expenses.Presentation
```

The API listens on `http://localhost:5404` (see `Properties/launchSettings.json`). Example requests:

```powershell
$expense = @{ description = 'Train to Mostar'; amount = 120.00; category = 'Travel'; incurredOn = '2026-10-01' }
Invoke-RestMethod -Method Post -Uri http://localhost:5404/expenses -ContentType 'application/json' -Body ($expense | ConvertTo-Json)
Invoke-RestMethod -Uri http://localhost:5404/expenses/summary/2026/10
```

## Project layout

```
04-LayeredArchitecture/
├── LayeredArchitecture.sln
├── docs/                         SPEC.md, REFACTOR-NOTES.md
├── src/
│   ├── Expenses.Presentation/    Program.cs, ExpenseEndpoints, HTTP contracts, ExpenseContractMapping
│   ├── Expenses.Business/        ExpenseService, ExpenseValidator, DTOs, ExpenseRecordMapping, AddBusinessLayer()
│   └── Expenses.DataAccess/      ExpenseRecord, IExpenseRepository, InMemoryExpenseRepository, AddDataAccessLayer()
└── tests/
    └── Expenses.Tests/           ArchitectureTests, ExpenseServiceTests, ExpenseEndpointsTests, hand-written test doubles
```

## Process log

| Step | What happened | `dotnet test` result |
|---|---|---|
| 1. Specify | `docs/SPEC.md` written before any code. | n/a |
| 2. Tests first | Empty projects and the solution created; 5 test methods (7 cases) written against the spec's API. `dotnet build`: `Build FAILED.`, `0 Warning(s)`, `14 Error(s)` (8 × CS0234 missing namespace, 6 × CS0246 missing type). | n/a (does not compile) |
| 3. Stubs (red) | Public API added with `throw new NotImplementedException()` bodies. The 3 behaviour tests fail. The 4 architecture cases already pass, because no dependency exists yet that could break a rule; they are guards against regressions (see step 7). | `Failed!  - Failed:     3, Passed:     4, Skipped:     0, Total:     7, Duration: 418 ms - Expenses.Tests.dll (net9.0)` |
| 4. First green | Simplest implementation: validation, mapping and saving inline in `ExpenseService`; mapping inline in the handlers; routes in `Program.cs`. | `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 32 ms - Expenses.Tests.dll (net9.0)` |
| 5. Run and validate | `dotnet run` plus curl: `201 Created` with `Location`, `400` problem details with camelCase keys, `"Travel, Meals"` rejected, summary ordered by total, month 13 gives `404`. | n/a |
| 6. Refactor | R1–R5: validator extracted, magic values named, one mapping class per boundary, routes moved next to their handlers. Tests run after each step; test code unchanged; the smoke test gave identical responses. Full rebuild: `0 Warning(s)`, `0 Error(s)`. | `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 693 ms - Expenses.Tests.dll (net9.0)` |
| 7. Guard check | In a scratch copy: `AddDataAccessLayer()` in `Program.cs` compiles, but test 1 fails; an upward project reference fails restore with MSB4006 (circular dependency). | `Failed!  - Failed:     1, Passed:     6, Skipped:     0, Total:     7` (deliberately broken copy) |
| 8. OpenSpec baseline | Behaviour recorded as the [`expense-tracking`](../openspec/specs/expense-tracking/spec.md) spec through the OpenSpec change `baseline-expense-tracking`: validated with `openspec validate --strict`, each scenario traced to a test or a manual check, then archived. The API was called again: `201` with `Location`, `400` for `"Travel, Meals"`, `404` for month 13, an empty summary for a month without expenses. No code changed. | `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 157 ms - Expenses.Tests.dll (net9.0)` |

## Key takeaways

- Strict layering is a rule about compiled dependencies. Project references are transitive, so the compiler
  allows a skip-layer call; the architecture test is what catches it.
- A DI entry point per layer keeps the composition root free of data-access types.
- Mapping at every boundary costs code, but it keeps the storage and HTTP formats out of the business rules.
- Business compiling against Data Access is the trade-off Clean/Onion removes; for a service this size it is
  acceptable.
