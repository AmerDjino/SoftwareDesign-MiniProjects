namespace Reminders;

public sealed class ConsoleReminderSender(TextWriter writer) : IReminderSender
{
    private readonly TextWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));

    public Task SendAsync(ReminderMessage message, CancellationToken cancellationToken) =>
        _writer.WriteLineAsync(
            $"To: {message.To} | Subject: {message.Subject} | Body: {message.Body}".AsMemory(), cancellationToken);
}
