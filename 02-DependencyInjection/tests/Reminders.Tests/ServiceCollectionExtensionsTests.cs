using Microsoft.Extensions.DependencyInjection;

namespace Reminders.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddReminders_BuildsAValidObjectGraph()
    {
        var services = new ServiceCollection().AddReminders();

        using var provider = BuildValidatedProvider(services);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ReminderService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void AddReminders_RegistersExpectedLifetimes()
    {
        using var provider = BuildValidatedProvider(new ServiceCollection().AddReminders());
        using var firstRun = provider.CreateScope();
        using var secondRun = provider.CreateScope();

        var serviceInFirstRun = firstRun.ServiceProvider.GetRequiredService<ReminderService>();
        var serviceAgainInFirstRun = firstRun.ServiceProvider.GetRequiredService<ReminderService>();
        var serviceInSecondRun = secondRun.ServiceProvider.GetRequiredService<ReminderService>();
        var logInFirstRun = firstRun.ServiceProvider.GetRequiredService<IReminderLog>();
        var logInSecondRun = secondRun.ServiceProvider.GetRequiredService<IReminderLog>();

        Assert.Same(serviceInFirstRun, serviceAgainInFirstRun);
        Assert.NotSame(serviceInFirstRun, serviceInSecondRun);
        Assert.Same(logInFirstRun, logInSecondRun);
    }

    [Fact]
    public async Task AddReminders_ReplacingAnImplementation_ChangesBehaviourWithoutTouchingConsumers()
    {
        var now = new DateTimeOffset(2026, 10, 7, 9, 0, 0, TimeSpan.Zero);
        var appointment = new Appointment(Guid.NewGuid(), "Ana Novak", "ana@example.com", "Dr. Selma Kurt", now.AddHours(3));
        var sender = new SpyReminderSender();
        var services = new ServiceCollection().AddReminders();
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(now));
        services.AddScoped<IAppointmentSource>(_ => new InMemoryAppointmentSource([appointment]));
        services.AddSingleton<IReminderSender>(sender);
        using var provider = BuildValidatedProvider(services);
        using var scope = provider.CreateScope();

        var sent = await scope.ServiceProvider.GetRequiredService<ReminderService>().SendDueRemindersAsync();

        Assert.Equal(1, sent);
        Assert.Equal("ana@example.com", Assert.Single(sender.Sent).To);
    }

    private static ServiceProvider BuildValidatedProvider(IServiceCollection services) =>
        services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
}
