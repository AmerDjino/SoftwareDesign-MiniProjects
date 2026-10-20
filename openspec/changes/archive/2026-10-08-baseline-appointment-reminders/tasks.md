# Tasks

## 1. Specify the existing behaviour

- [x] 1.1 Turn FR-1 to FR-9 of `02-DependencyInjection/docs/SPEC.md` into requirements and scenarios in `specs/appointment-reminders/spec.md` (FR-10, the console demo, is a demonstration and not specified); verify with `openspec validate baseline-appointment-reminders --strict`

## 2. Trace every scenario to a test

- [x] 2.1 "Due within the next 24 hours", "Not due", "Two due appointments among five" and "Appointment stored with a local offset": verified by `ReminderServiceTests.SendDueReminders_AppointmentsWithinNext24Hours_SendsOneReminderEach`
- [x] 2.2 "Second run an hour later": verified by `ReminderServiceTests.SendDueReminders_RunTwice_DoesNotRemindTheSameAppointmentAgain`
- [x] 2.3 "Defaults only": verified by `ServiceCollectionExtensionsTests.AddReminders_BuildsAValidObjectGraph`
- [x] 2.4 "Two runs": verified by `ServiceCollectionExtensionsTests.AddReminders_RegistersExpectedLifetimes`
- [x] 2.5 "Replacements registered after the defaults": verified by `ServiceCollectionExtensionsTests.AddReminders_ReplacingAnImplementation_ChangesBehaviourWithoutTouchingConsumers`
- [x] 2.6 Record the scenarios without an automated test ("Sending fails halfway", "Replacement registered before the defaults") in design.md; verify by reading its Risks section
- [x] 2.7 Run `dotnet test 02-DependencyInjection/DependencyInjection.sln` and verify that all 5 test cases pass

## 3. Point readers to the living spec

- [x] 3.1 Link `openspec/specs/appointment-reminders/spec.md` from `02-DependencyInjection/README.md` and from the header of `02-DependencyInjection/docs/SPEC.md`; verify that every link points to the spec path that archiving creates
