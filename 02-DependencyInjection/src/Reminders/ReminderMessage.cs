namespace Reminders;

public sealed record ReminderMessage(string To, string Subject, string Body);
