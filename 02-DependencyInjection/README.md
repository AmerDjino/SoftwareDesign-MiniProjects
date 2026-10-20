# 02 — Dependency Injection

A clinic reminder job that demonstrates constructor injection, the composition root and service lifetimes in .NET 9. Each run sends one reminder for every appointment starting within the next 24 hours that has not been reminded yet. `ReminderService` receives its four volatile dependencies (clock, appointment source, sender, reminder log) through its constructor, so the tests can wire it by hand with fakes. `AddReminders()` and a small console app assemble the same object graph with `Microsoft.Extensions.DependencyInjection`, using deliberate lifetimes and build-time validation.

## Artifacts

| Artifact | Location |
|---|---|
| Specification | [docs/SPEC.md](docs/SPEC.md) |
| Unit tests | [tests/Reminders.Tests](tests/Reminders.Tests) |
| Implementation | [src/Reminders](src/Reminders) (library), [src/Reminders.ConsoleApp](src/Reminders.ConsoleApp) (composition root demo) |
| Refactor notes | [docs/REFACTOR-NOTES.md](docs/REFACTOR-NOTES.md) |
| Living spec (OpenSpec) | [openspec/specs/appointment-reminders/spec.md](../openspec/specs/appointment-reminders/spec.md) |

## How to run

From this folder:

```powershell
dotnet test
dotnet run --project src/Reminders.ConsoleApp
```

The demo seeds three appointments (two of them due) and runs the job twice, each run in its own scope. The times depend on when you run it:

```text
To: ana@example.com | Subject: Appointment reminder: 2026-10-07 14:19 UTC | Body: Dear Ana Novak, this is a reminder ...
To: emir@example.com | Subject: Appointment reminder: 2026-10-08 08:19 UTC | Body: Dear Emir Kasum, this is a reminder ...
Run 1: 2 reminder(s) sent.
Run 2: 0 reminder(s) sent.
```

## Project layout

```text
02-DependencyInjection/
  DependencyInjection.sln
  docs/                      SPEC.md, REFACTOR-NOTES.md
  src/
    Reminders/               class library
      ReminderService.cs               the job: constructor injection only
      ServiceCollectionExtensions.cs   AddReminders(): defaults and lifetimes
      ReminderMessageFormatter.cs      pure function, deliberately not injected
      IAppointmentSource.cs, IReminderSender.cs, IReminderLog.cs
      InMemoryAppointmentSource.cs, ConsoleReminderSender.cs, InMemoryReminderLog.cs
      Appointment.cs, ReminderMessage.cs
    Reminders.ConsoleApp/    Program.cs: the composition root
  tests/
    Reminders.Tests/
      ReminderServiceTests.cs               AC-1, AC-2: wired by hand, no container
      ServiceCollectionExtensionsTests.cs   AC-3 to AC-5: real container
      FakeTimeProvider.cs, SpyReminderSender.cs   hand-written test doubles
```

## Process log

| Step | What happened | `dotnet test` result |
|---|---|---|
| Specification | Wrote [docs/SPEC.md](docs/SPEC.md): the concept, the due rule and message format, the API, the lifetimes and five acceptance criteria. | — |
| Tests first | Created the library and test projects and wrote the five tests. `dotnet build` failed as expected: `0 Warning(s)`, `5 Error(s)`, all CS0246 (`Appointment`, `IReminderSender`, `ReminderMessage` not found). The compiler stops at declaration errors, so the method bodies were not even bound. | — |
| Red | Added API stubs: records, interfaces, and classes whose members throw `NotImplementedException`. All five tests failed with that exception. | `Failed!  - Failed:     5, Passed:     0, Skipped:     0, Total:     5, Duration: 162 ms - Reminders.Tests.dll (net9.0)` |
| First green | Simplest implementation, plus the console app as the composition root. The demo printed two reminders, then 0 on the second run. | `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 54 ms - Reminders.Tests.dll (net9.0)` |
| Refactor | Extracted the formatter as a pure function (not injected), named the 24-hour window, added constructor guard clauses, switched to `TryAdd*` with a reason per lifetime. Green after every step; test code unchanged. An experiment on container validation led to one corrected sentence in the spec. | `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 64 ms - Reminders.Tests.dll (net9.0)` |
| Build check | `dotnet build DependencyInjection.sln --no-incremental`: `0 Warning(s)`, `0 Error(s)`. | — |
| OpenSpec baseline | Behaviour recorded as the [`appointment-reminders`](../openspec/specs/appointment-reminders/spec.md) spec through the OpenSpec change `baseline-appointment-reminders`: validated with `openspec validate --strict`, each scenario traced to its test, then archived. No code changed. | `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 206 ms - Reminders.Tests.dll (net9.0)` |

## Key takeaways

- DI is a technique, not a container: `ReminderService` is tested by passing four arguments to its constructor.
- Inject what is volatile (time, data, delivery, shared state); call pure logic such as message formatting directly.
- Lifetimes follow the dependencies: the log must be a singleton for correctness, and `ReminderService` must not be one, because its source is scoped.
- Turn on both `ValidateOnBuild` and `ValidateScopes`: with either one alone, a captive dependency is not caught at startup.
