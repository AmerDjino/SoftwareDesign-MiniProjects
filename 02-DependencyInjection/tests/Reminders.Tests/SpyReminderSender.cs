namespace Reminders.Tests;

internal sealed class SpyReminderSender : IReminderSender
{
    private readonly List<ReminderMessage> _sent = [];

    public IReadOnlyList<ReminderMessage> Sent => _sent;

    public Task SendAsync(ReminderMessage message, CancellationToken cancellationToken)
    {
        _sent.Add(message);
        return Task.CompletedTask;
    }
}
