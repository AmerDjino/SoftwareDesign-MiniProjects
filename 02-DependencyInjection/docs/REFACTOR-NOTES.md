# Refactor notes — 02 Dependency Injection

How the first passing implementation was improved without changing its tested behaviour. Every "before" snippet is copied from the first-green code. Claims about runtime behaviour that the five tests do not cover were checked in a throwaway console app outside the repository.

## Starting point

The first green implementation was the most direct code that satisfied the five tests:

- `ReminderService` used its primary-constructor parameters directly and built the reminder text inline, in the middle of the send loop.
- The 24-hour window was the literal `now.AddHours(24)`.
- `AddReminders()` used plain `Add*` registrations with no explanation of the lifetimes.

Test result: `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 54 ms - Reminders.Tests.dll (net9.0)`

The console demo printed two reminders, then `Run 1: 2 reminder(s) sent.` and `Run 2: 0 reminder(s) sent.`

## Refactorings

`dotnet test` ran after each step. All five tests stayed green throughout, and no test code changed.

### 1. Message formatting extracted into a pure function, deliberately not injected

**Smell.** `SendDueRemindersAsync` did two jobs: orchestrating the volatile dependencies (clock, source, log, sender) and composing the e-mail text (UTC conversion, format string, culture, wording). Changing one word in the reminder meant editing the job's control flow, and FR-3 had no single home.

**Principle.** Inject *volatile* dependencies; call *stable* ones directly. Formatting is deterministic, does no I/O and has exactly one correct behaviour, so there is nothing to swap. An `IReminderMessageFormatter` would add an interface, a registration and a constructor parameter, and it would let a test replace the very output that AC-1 is meant to verify.

**Change.** New `internal static class ReminderMessageFormatter` with `Format(Appointment)`; `internal` because it is not an extension point. The `+ " UTC"` concatenation became a quoted literal in the format string.

Before (inside the loop of `ReminderService`):

```csharp
var start = appointment.StartsAt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " UTC";
var message = new ReminderMessage(
    appointment.PatientEmail,
    $"Appointment reminder: {start}",
    $"Dear {appointment.PatientName}, this is a reminder of your appointment with {appointment.ClinicianName} on {start}.");
await reminderSender.SendAsync(message, cancellationToken);
```

After:

```csharp
// ReminderService
await _reminderSender.SendAsync(ReminderMessageFormatter.Format(appointment), cancellationToken);

// ReminderMessageFormatter.Format
var start = appointment.StartsAt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
return new ReminderMessage(
    To: appointment.PatientEmail,
    Subject: $"Appointment reminder: {start}",
    Body: $"Dear {appointment.PatientName}, this is a reminder of your appointment with {appointment.ClinicianName} on {start}.");
```

### 2. The 24-hour rule got a name

**Smell.** The central business rule (FR-1) was a bare number, and the variable `appointments` did not say that it holds the appointments inside the reminder window.

```csharp
// Before
var appointments = await appointmentSource.GetStartingBetweenAsync(now, now.AddHours(24), cancellationToken);

// After
private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(24);

var dueAppointments = await _appointmentSource.GetStartingBetweenAsync(now, now + ReminderWindow, cancellationToken);
```

### 3. Guard clauses and `readonly` fields: constructor injection done fully

**Problem.** The primary-constructor parameters were used directly: never checked, and captured as mutable state. With hand wiring (pure DI, as in the AC-1 and AC-2 tests), a `null` dependency would only surface as a `NullReferenceException` when the job first ran (for a periodic job, possibly long after startup), and the exception would not say which dependency was missing.

**Principle.** In the Constructor Injection pattern the constructor guarantees a complete object: it guards each dependency, stores it in a `readonly` field and does nothing else.

**Change.** Applied to `ReminderService` and `ConsoleReminderSender`, the two classes that receive injected dependencies. `InMemoryAppointmentSource` was left alone: it copies its seed in the constructor, so a `null` already fails there.

```csharp
// Before: parameter used directly in the method body
var now = timeProvider.GetUtcNow();

// After
private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

var now = _timeProvider.GetUtcNow();
```

Strictly, this changes behaviour for invalid input (`null`), which the spec does not cover. Checked outside the repository: `new ReminderService(null!, ...)` now throws `ArgumentNullException` with `ParamName = appointmentSource`.

### 4. `TryAdd*` registrations, each with the reason for its lifetime

**Problem.** `Add*` always appends, and for a single service the container resolves the last registration. As a result:

- a host that registered its own `TimeProvider` *before* calling `AddReminders()` silently got `TimeProvider.System` instead;
- calling `AddReminders()` twice registered everything twice;
- the lifetimes, the main design decision in this project, had no explanation where a maintainer would change them.

**Principle.** A library's `Add*` method supplies defaults, and the application's composition root has the final say whatever the call order. `TryAdd*` registers a service only if it has no registration yet.

```csharp
// Before
services.AddSingleton(TimeProvider.System);
services.AddSingleton<IReminderLog, InMemoryReminderLog>();
// ...
services.AddScoped<ReminderService>();

// After
// Stateless and thread-safe: one clock for the whole process.
services.TryAddSingleton(TimeProvider.System);

// Shared by every run (scope); a shorter lifetime would forget which reminders were already sent.
services.TryAddSingleton<IReminderLog, InMemoryReminderLog>();
// ...
// Scoped because it depends on the scoped source; as a singleton it would hold that source captive.
services.TryAddScoped<ReminderService>();
```

Checked outside the repository, with the first-green registrations copied into the throwaway app:

| Scenario | First green (`Add*`) | Refactored (`TryAdd*`) |
|---|---|---|
| Host registers its own `TimeProvider` before `AddReminders()` | `TimeProvider.System` resolved; the host's clock is lost | The host's clock is resolved |
| `AddReminders()` called twice | 10 service descriptors | 5 service descriptors |

Registering *after* `AddReminders()`, which is what the AC-5 test and `Program.cs` do, works the same in both versions. That is why no test noticed the problem.

### Considered and not changed

- **Reading the clock once per run.** First green already did this (`var now = ...` feeds both ends of the window), so there was nothing to fix.
- **`Program.cs`.** It registers the seeded source after `AddReminders()`, which works in both versions and stays the simplest way to show a host overriding a default.

## Experiment: which validation catches a captive dependency

Before refactoring, the throwaway app registered `ReminderService` as a singleton (after `AddReminders()`) and resolved it in a scope under each combination of options:

| Options | Result |
|---|---|
| None (plain `BuildServiceProvider()`) | No error: the singleton silently captures the scoped source. |
| `ValidateOnBuild` only | No error either. |
| `ValidateScopes` only | Fails on first resolution: `Cannot consume scoped service 'Reminders.IAppointmentSource' from singleton 'Reminders.ReminderService'.` |
| Both | Fails inside `BuildServiceProvider`: `AggregateException: Some services are not able to be constructed (...)` wrapping the same message. |

The spec had said that `ValidateScopes` makes the container refuse to build. That sentence in [SPEC.md](SPEC.md) section 5.3 was corrected; the tests and `Program.cs` already enabled both options.

## Result

- After the refactor: `Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 64 ms - Reminders.Tests.dll (net9.0)`
- `dotnet build DependencyInjection.sln --no-incremental`: `0 Warning(s)`, `0 Error(s)`.
- Public API: unchanged. One new internal type, `ReminderMessageFormatter`. Observable differences appear only for misuse: `null` dependencies now fail at construction, and `AddReminders()` no longer overrides registrations the host made earlier.
- Test code: unchanged.

## Not done — ideas for a further manual pass

1. **Pin down the `TryAdd*` behaviour in a test.** Turn the AC-5 test into a `[Theory]` with one row that registers the test doubles after `AddReminders()` and one that registers them before. It stays one test method, and the order independence stops depending on a throwaway experiment.
2. **Make `IReminderLog` asynchronous** (`Task<bool> WasSentAsync(Guid, CancellationToken)`, `Task MarkSentAsync(Guid, CancellationToken)`), as a database-backed log would need. Follow the compiler errors and notice that only the abstraction, its implementation and its single consumer change.
3. **Move the run loop out of `Program.cs`** into a small `ReminderJob` that receives `IServiceScopeFactory` and creates one scope per run. Infrastructure that owns scopes is a legitimate user of the container, unlike a business class that calls `GetService` (service locator); writing both side by side makes the difference concrete.
4. **Remove the default `IAppointmentSource` registration** and run the tests. The AC-3 test should now fail inside `BuildServiceProvider`, because `ValidateOnBuild` reports the missing registration. Then decide which contract is better, "the host must register a source" or "an empty default", and update the spec and the test to match.

## Lessons learned

- **Constructor injection pays off before any container exists.** The two `ReminderService` tests build the object graph with `new` and four arguments: no container, no mocking library. The container matters only in the composition root.
- **`ValidateOnBuild` alone does not catch captive dependencies.** Only `ValidateOnBuild` together with `ValidateScopes` fails at startup. The first version of the spec got this wrong, so claims about framework behaviour are worth testing.
- **Not everything belongs behind an interface.** The formatter is simpler as a static pure function; injecting it would have added ceremony and let tests fake away the behaviour they check.
- **Lifetimes follow dependencies and correctness.** `ReminderService` is scoped because its source is scoped; the log is a singleton because FR-4 depends on its state outliving each run. A one-line reason next to the registration keeps the next person from "simplifying" it.
- **Registration code is part of the public contract.** `Add*` versus `TryAdd*` decides whether the host or the library wins, and the tests could not see the difference because they only register overrides afterwards.
