using Microsoft.Extensions.DependencyInjection;
using Reminders;

var now = TimeProvider.System.GetUtcNow();
Appointment[] appointments =
[
    new(Guid.NewGuid(), "Ana Novak", "ana@example.com", "Dr. Selma Kurt", now.AddHours(2)),
    new(Guid.NewGuid(), "Emir Kasum", "emir@example.com", "Dr. Adnan Pasic", now.AddHours(20)),
    new(Guid.NewGuid(), "Lejla Hasan", "lejla@example.com", "Dr. Selma Kurt", now.AddDays(3)),
];

var services = new ServiceCollection();
services.AddReminders();
services.AddScoped<IAppointmentSource>(_ => new InMemoryAppointmentSource(appointments));

using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

for (var run = 1; run <= 2; run++)
{
    using var scope = provider.CreateScope();
    var reminderService = scope.ServiceProvider.GetRequiredService<ReminderService>();
    var sent = await reminderService.SendDueRemindersAsync();
    Console.WriteLine($"Run {run}: {sent} reminder(s) sent.");
}
