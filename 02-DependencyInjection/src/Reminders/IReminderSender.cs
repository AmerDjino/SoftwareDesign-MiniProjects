namespace Reminders;

public interface IReminderSender
{
    /// <summary>
    /// Delivers one reminder. Completes once the message has been handed over for delivery; throws if it could not be.
    /// </summary>
    Task SendAsync(ReminderMessage message, CancellationToken cancellationToken);
}
