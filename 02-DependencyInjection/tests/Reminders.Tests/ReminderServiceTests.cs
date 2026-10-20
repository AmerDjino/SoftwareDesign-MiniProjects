namespace Reminders.Tests;

public sealed class ReminderServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SendDueReminders_AppointmentsWithinNext24Hours_SendsOneReminderEach()
    {
        // 13:00 at UTC+2 is 11:00 UTC, two hours from now.
        var dueInTwoHours = AppointmentAt(new DateTimeOffset(2026, 10, 7, 13, 0, 0, TimeSpan.FromHours(2)), "Ana Novak", "ana@example.com");
        var dueInExactly24Hours = AppointmentAt(Now.AddHours(24), "Emir Kasum", "emir@example.com");
        var notDueIn24HoursAndOneMinute = AppointmentAt(Now.AddHours(24).AddMinutes(1), "Lejla Hasan", "lejla@example.com");
        var notDueStartingNow = AppointmentAt(Now, "Mirza Begic", "mirza@example.com");
        var notDueStartedAnHourAgo = AppointmentAt(Now.AddHours(-1), "Sara Dizdar", "sara@example.com");
        var source = new InMemoryAppointmentSource(
            [notDueIn24HoursAndOneMinute, dueInExactly24Hours, notDueStartedAnHourAgo, dueInTwoHours, notDueStartingNow]);
        var sender = new SpyReminderSender();
        var service = new ReminderService(source, sender, new InMemoryReminderLog(), new FakeTimeProvider(Now));

        var sent = await service.SendDueRemindersAsync();

        ReminderMessage[] expected =
        [
            new("ana@example.com", "Appointment reminder: 2026-10-07 11:00 UTC",
                "Dear Ana Novak, this is a reminder of your appointment with Dr. Selma Kurt on 2026-10-07 11:00 UTC."),
            new("emir@example.com", "Appointment reminder: 2026-10-08 09:00 UTC",
                "Dear Emir Kasum, this is a reminder of your appointment with Dr. Selma Kurt on 2026-10-08 09:00 UTC."),
        ];
        Assert.Equal(2, sent);
        Assert.Equal(expected, sender.Sent);
    }

    [Fact]
    public async Task SendDueReminders_RunTwice_DoesNotRemindTheSameAppointmentAgain()
    {
        var source = new InMemoryAppointmentSource([AppointmentAt(Now.AddHours(2), "Ana Novak", "ana@example.com")]);
        var sender = new SpyReminderSender();
        var log = new InMemoryReminderLog();
        var clock = new FakeTimeProvider(Now);
        await new ReminderService(source, sender, log, clock).SendDueRemindersAsync();
        clock.Advance(TimeSpan.FromHours(1));

        var sentBySecondRun = await new ReminderService(source, sender, log, clock).SendDueRemindersAsync();

        Assert.Equal(0, sentBySecondRun);
        Assert.Single(sender.Sent);
    }

    private static Appointment AppointmentAt(DateTimeOffset startsAt, string patientName, string patientEmail) =>
        new(Guid.NewGuid(), patientName, patientEmail, "Dr. Selma Kurt", startsAt);
}
