namespace Reminders;

public sealed class InMemoryReminderLog : IReminderLog
{
    private readonly HashSet<Guid> _sent = [];
    private readonly Lock _lock = new();

    public bool WasSent(Guid appointmentId)
    {
        lock (_lock)
        {
            return _sent.Contains(appointmentId);
        }
    }

    public void MarkSent(Guid appointmentId)
    {
        lock (_lock)
        {
            _sent.Add(appointmentId);
        }
    }
}
