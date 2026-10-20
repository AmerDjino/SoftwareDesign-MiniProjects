namespace Reminders;

public interface IAppointmentSource
{
    /// <summary>
    /// Returns the appointments that start after <paramref name="from"/> and no later than <paramref name="to"/>,
    /// ordered by start time, earliest first.
    /// </summary>
    Task<IReadOnlyList<Appointment>> GetStartingBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}
