using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Reminders;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ReminderService"/> and a default for each of its dependencies. A default is added only when
    /// the service has no registration yet, so the host can register its own implementations before or after this call.
    /// </summary>
    public static IServiceCollection AddReminders(this IServiceCollection services)
    {
        // Stateless and thread-safe: one clock for the whole process.
        services.TryAddSingleton(TimeProvider.System);

        // Shared by every run (scope); a shorter lifetime would forget which reminders were already sent.
        services.TryAddSingleton<IReminderLog, InMemoryReminderLog>();

        // Stands in for a DbContext-backed source: one unit of work per run. Empty until the host registers real data.
        services.TryAddScoped<IAppointmentSource>(_ => new InMemoryAppointmentSource([]));

        // Stateless, and Console.Out is synchronised, so one instance can serve every run.
        services.TryAddSingleton<IReminderSender>(_ => new ConsoleReminderSender(Console.Out));

        // Scoped because it depends on the scoped source; as a singleton it would hold that source captive.
        services.TryAddScoped<ReminderService>();

        return services;
    }
}
