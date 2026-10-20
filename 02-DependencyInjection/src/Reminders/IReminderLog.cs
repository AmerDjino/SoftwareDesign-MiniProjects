namespace Reminders;

/// <summary>
/// Remembers which appointments have already been reminded. One instance is shared by every run,
/// so implementations must be thread-safe.
/// </summary>
public interface IReminderLog
{
    bool WasSent(Guid appointmentId);

    void MarkSent(Guid appointmentId);
}
