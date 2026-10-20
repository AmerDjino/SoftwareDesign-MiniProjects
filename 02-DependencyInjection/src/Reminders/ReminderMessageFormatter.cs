using System.Globalization;

namespace Reminders;

/// <summary>
/// Builds the reminder text for an appointment. It is a pure function (no clock, no I/O, no state),
/// so <see cref="ReminderService"/> calls it directly instead of receiving it through dependency injection.
/// </summary>
internal static class ReminderMessageFormatter
{
    public static ReminderMessage Format(Appointment appointment)
    {
        var start = appointment.StartsAt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
        return new ReminderMessage(
            To: appointment.PatientEmail,
            Subject: $"Appointment reminder: {start}",
            Body: $"Dear {appointment.PatientName}, this is a reminder of your appointment with {appointment.ClinicianName} on {start}.");
    }
}
