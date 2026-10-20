namespace Reminders;

public sealed record Appointment(Guid Id, string PatientName, string PatientEmail, string ClinicianName, DateTimeOffset StartsAt);
