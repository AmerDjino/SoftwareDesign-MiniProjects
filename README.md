# Software Design Mini-Projects

Six small .NET 9 projects, one per software-design concept. Each one is built test-first: a written
specification, then 3–5 unit tests that fail, then the implementation, then a refactor with notes.
The domains are small business problems on purpose, so the concept is the interesting part, not the domain.
The current behaviour of every project is kept as a living [OpenSpec](https://github.com/Fission-AI/OpenSpec) spec in [`openspec/`](openspec/).

| # | Concept | Mini-app | Living spec | Tests (methods / cases) |
|---|---------|----------|-------------|-------------------------|
| 01 | [SOLID principles](01-SolidPrinciples/) | Checkout pricing with pluggable discount rules | [checkout-pricing](openspec/specs/checkout-pricing/spec.md) | 5 / 21 |
| 02 | [Dependency Injection](02-DependencyInjection/) | Appointment-reminder job wired through a composition root | [appointment-reminders](openspec/specs/appointment-reminders/spec.md) | 5 / 5 |
| 03 | [Repository Pattern](03-RepositoryPattern/) | Invoices (overdue list, mark as paid) with in-memory and EF Core + SQLite repositories | [invoice-payments](openspec/specs/invoice-payments/spec.md), [invoice-storage](openspec/specs/invoice-storage/spec.md) | 5 / 9 |
| 04 | [Layered Architecture](04-LayeredArchitecture/) | Expense-tracking Web API: Presentation → Business → Data Access | [expense-tracking](openspec/specs/expense-tracking/spec.md) | 5 / 7 |
| 05 | [CQRS basics](05-CqrsBasics/) | Warehouse stock: commands, queries, write model and read model | [warehouse-stock](openspec/specs/warehouse-stock/spec.md) | 5 / 6 |
| 06 | [Separation of Concerns](06-SeparationOfConcerns/) | Timesheet CSV import → overtime report | [timesheet-report](openspec/specs/timesheet-report/spec.md) | 5 / 5 |

A test method that is an xUnit `[Theory]` runs once per data row, so one method can produce several test cases.

## What every mini-project contains

| Artifact | Where |
|----------|-------|
| Specification | `docs/SPEC.md`: the concept in plain words, scope, requirements, design, acceptance criteria mapped to test names. Written before the code and kept as a record of that step |
| Living spec | `openspec/specs/<capability>/spec.md` at the repo root: the current behaviour as OpenSpec requirements and scenarios |
| Unit tests | `tests/<Project>.Tests/`: 3–5 xUnit test methods, written before the implementation |
| Implementation | `src/<Project>/` |
| Refactor notes | `docs/REFACTOR-NOTES.md`: what the first passing version looked like, what was refactored and why (with before/after code), ideas for a further manual pass |

Each project's `README.md` has a process log with the actual `dotnet test` output of every step.

## How each project was built

1. **Specify.** Write `docs/SPEC.md` first: the problem, the rules with exact numbers, the public API, and acceptance criteria.
2. **Test first.** Write 3–5 tests against the API in the spec. The build fails because the code doesn't exist yet.
   Add API stubs that only throw `NotImplementedException` and run the tests to see them fail (red).
3. **Implement.** Write the simplest implementation that makes the tests pass (green).
4. **Refactor.** Improve the design without changing behaviour, keep the tests green, and write down what changed and why.
5. **Record the living spec.** Once all six projects were done, each project's behaviour was recorded as an OpenSpec spec
   through one change per project, archived in `openspec/changes/archive/2026-10-08-baseline-*`. Each change has a proposal,
   a spec delta, a design note and tasks; it was validated with `openspec validate --strict` before archiving, and its
   `tasks.md` names the test behind every scenario and lists the scenarios that no automated test covers.

## Specs with OpenSpec

OpenSpec keeps specs next to the code. `openspec/specs/` holds the current behaviour, and every change to that behaviour
starts as a proposal in `openspec/changes/<change>/` (why, a spec delta, design and tasks) that is archived when the work
is done. The repo uses one `openspec/` folder at the root, the layout OpenSpec recommends for monorepos, with one or two
capabilities per mini-project. `openspec/config.yaml` holds the rules every change follows: work test-first, keep each
project's README process log and refactor notes up to date, and leave `docs/SPEC.md` unchanged.

Specs describe what a caller can observe. The concept a project demonstrates is design, so it stays in `docs/SPEC.md`
and in each change's `design.md`. For example, the layering rules of 04 and the read-side rule of 05 are guarded by
architecture tests rather than written as spec scenarios.

With the OpenSpec CLI installed (`npm install -g @fission-ai/openspec@latest`):

```bash
openspec list --specs                  # the seven capabilities
openspec show checkout-pricing --type spec
openspec validate --all --strict
openspec view                          # dashboard of specs and changes
```

To change a project's behaviour, start a change. In Claude Code use `/opsx:propose <change-name>`, then `/opsx:apply` and
`/opsx:archive` (the commands live in `.claude/`). By hand: `openspec new change <change-name>`, write the artifacts, then
`openspec validate <change-name> --strict` and `openspec archive <change-name>`. The "Not done" ideas at the end of each
`docs/REFACTOR-NOTES.md` are ready-made candidates.

## Running

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

```bash
dotnet test SoftwareDesign-MiniProjects.sln
```

Every mini-project folder also has its own solution, so you can open or test a single concept, e.g.
`dotnet test 03-RepositoryPattern/RepositoryPattern.sln`.

Three projects also have something to run:

```bash
dotnet run --project 02-DependencyInjection/src/Reminders.ConsoleApp      # composition-root demo
dotnet run --project 04-LayeredArchitecture/src/Expenses.Presentation     # Web API on http://localhost:5404
dotnet run --project 06-SeparationOfConcerns/src/Timesheets.ConsoleApp    # overtime report for the sample CSV
```

## Repository layout

```
SoftwareDesign-MiniProjects/
├── SoftwareDesign-MiniProjects.sln   all projects, one solution folder per mini-project
├── Directory.Build.props             shared settings: net9.0, nullable, warnings as errors
├── Directory.Packages.props          central NuGet package versions
├── .editorconfig / .gitignore / .gitattributes
├── openspec/                         OpenSpec: config.yaml, specs/ (living specs), changes/ (proposals and archive)
├── .claude/                          OpenSpec's /opsx commands and skills for Claude Code
├── 01-SolidPrinciples/
├── 02-DependencyInjection/
├── 03-RepositoryPattern/
├── 04-LayeredArchitecture/
├── 05-CqrsBasics/
└── 06-SeparationOfConcerns/
    ├── README.md
    ├── docs/SPEC.md, docs/REFACTOR-NOTES.md
    ├── src/...
    └── tests/...
```

## Conventions

- Tests use plain xUnit `Assert` and hand-written test doubles (fakes, stubs, spies) with no mocking library,
  so the tests double as examples of the concepts.
- Tests are deterministic: no real clock, network or file system (the repository project uses SQLite in-memory).
- Numbers and dates are parsed and formatted with `CultureInfo.InvariantCulture`.
- Builds treat warnings as errors.
