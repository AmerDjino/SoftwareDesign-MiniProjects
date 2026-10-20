## Purpose

Reminds each patient once, by e-mail, of an appointment that starts within the next 24 hours, and lets the host application choose the clock, the appointment source and the delivery channel.

## ADDED Requirements

### Requirement: Due appointments
A reminder run SHALL read the current time once. An appointment SHALL be due when it starts after that time and no more than 24 hours later; an appointment that starts at that time or earlier SHALL never be due.

#### Scenario: Due within the next 24 hours
- **GIVEN** the time is 2026-10-07 09:00 UTC
- **WHEN** a run finds appointments starting at 2026-10-07 13:00 +02:00 (11:00 UTC) and exactly 24 hours later, at 2026-10-08 09:00 UTC
- **THEN** both appointments are reminded

#### Scenario: Not due
- **GIVEN** the time is 2026-10-07 09:00 UTC
- **WHEN** a run finds appointments starting 24 hours and 1 minute later, exactly now, and 1 hour ago
- **THEN** none of them is reminded

### Requirement: One reminder per due appointment
A run SHALL send exactly one reminder, to the patient's e-mail address, for every due appointment that has not been reminded before, earliest appointment first, and SHALL report how many reminders it sent.

#### Scenario: Two due appointments among five
- **GIVEN** five appointments, two of them due
- **WHEN** a run completes
- **THEN** two reminders are sent, the earlier appointment first
- **AND** the run reports 2

### Requirement: Reminder message
A reminder SHALL be addressed to the patient's e-mail address, with the subject `Appointment reminder: {start}` and the body `Dear {patient}, this is a reminder of your appointment with {clinician} on {start}.`, where `{start}` is the start time converted to UTC and written `yyyy-MM-dd HH:mm UTC`, whatever the machine's regional settings.

#### Scenario: Appointment stored with a local offset
- **GIVEN** Ana Novak (`ana@example.com`) has an appointment with Dr. Selma Kurt at 2026-10-07 13:00 +02:00
- **WHEN** she is reminded
- **THEN** the subject is `Appointment reminder: 2026-10-07 11:00 UTC`
- **AND** the body is `Dear Ana Novak, this is a reminder of your appointment with Dr. Selma Kurt on 2026-10-07 11:00 UTC.`

### Requirement: No repeated reminders
An appointment that has been reminded SHALL NOT be reminded again by a later run in the same process.

#### Scenario: Second run an hour later
- **GIVEN** a run has reminded an appointment
- **WHEN** another run starts one hour later
- **THEN** it sends nothing and reports 0

### Requirement: Delivery failure
If sending a reminder fails, the run SHALL stop with that error. Reminders sent earlier in the run SHALL stay recorded, and the failed and remaining appointments SHALL be reminded by the next run, so every reminder is delivered at least once.

#### Scenario: Sending fails halfway
- **GIVEN** a run with three due appointments, and the second reminder cannot be sent
- **WHEN** the run fails
- **THEN** the first appointment is recorded as reminded
- **AND** the next run reminds the second and third appointments

### Requirement: Default setup
The library's default registration SHALL give a host a complete and valid setup (the system clock, a console sender, an empty appointment source and the reminder job) that passes the dependency-injection container's build-time and scope validation.

#### Scenario: Defaults only
- **WHEN** a host registers only the defaults and builds its service provider with build-time and scope validation turned on
- **THEN** the provider builds
- **AND** a reminder job can be resolved inside a run scope

### Requirement: Shared reminder history
All runs in a process SHALL share one reminder history, and each run SHALL get its own reminder job.

#### Scenario: Two runs
- **WHEN** a host creates two run scopes from the default setup
- **THEN** both runs use the same reminder history
- **AND** each run has its own reminder job, the same instance for every request within that run

### Requirement: Replaceable dependencies
A host SHALL be able to replace the clock, the appointment source and the reminder sender by registering its own implementations, before or after the defaults, without any change to the reminder job.

#### Scenario: Replacements registered after the defaults
- **GIVEN** the defaults, then a fixed clock at 2026-10-07 09:00 UTC, an appointment source with one appointment three hours later and a sender that captures messages
- **WHEN** a run completes
- **THEN** the captured messages contain exactly one reminder, to `ana@example.com`
- **AND** the run reports 1

#### Scenario: Replacement registered before the defaults
- **GIVEN** a host registers its own clock and then the defaults
- **WHEN** the reminder job is resolved
- **THEN** it uses the host's clock
