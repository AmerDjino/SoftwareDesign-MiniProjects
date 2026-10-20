namespace Reminders;

public sealed class InMemoryAppointmentSource(IEnumerable<Appointment> appointments) : IAppointmentSource
{
    private readonly Appointment[] _appointments = [.. appointments];

    public Task<IReadOnlyList<Appointment>> GetStartingBetweenAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        IReadOnlyList<Appointment> startingBetween =
        [
            .. _appointments
                .Where(appointment => appointment.StartsAt > from && appointment.StartsAt <= to)
                .OrderBy(appointment => appointment.StartsAt),
        ];
        return Task.FromResult(startingBetween);
    }
}
