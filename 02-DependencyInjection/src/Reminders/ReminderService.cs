namespace Reminders;

public sealed class ReminderService(
    IAppointmentSource appointmentSource, IReminderSender reminderSender, IReminderLog reminderLog, TimeProvider timeProvider)
{
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(24);

    private readonly IAppointmentSource _appointmentSource = appointmentSource ?? throw new ArgumentNullException(nameof(appointmentSource));
    private readonly IReminderSender _reminderSender = reminderSender ?? throw new ArgumentNullException(nameof(reminderSender));
    private readonly IReminderLog _reminderLog = reminderLog ?? throw new ArgumentNullException(nameof(reminderLog));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    public async Task<int> SendDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var dueAppointments = await _appointmentSource.GetStartingBetweenAsync(now, now + ReminderWindow, cancellationToken);
        var sent = 0;
        foreach (var appointment in dueAppointments)
        {
            if (_reminderLog.WasSent(appointment.Id))
            {
                continue;
            }

            await _reminderSender.SendAsync(ReminderMessageFormatter.Format(appointment), cancellationToken);
            _reminderLog.MarkSent(appointment.Id);
            sent++;
        }

        return sent;
    }
}
