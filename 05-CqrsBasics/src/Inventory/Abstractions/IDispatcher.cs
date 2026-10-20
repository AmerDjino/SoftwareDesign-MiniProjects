namespace Inventory.Abstractions;

/// <summary>
/// Routes commands and queries to their handlers, so callers depend on one interface instead of on every handler.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends the command to its single <see cref="ICommandHandler{TCommand}"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No handler is registered for <typeparamref name="TCommand"/>.</exception>
    Task<CommandResult> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>
    /// Sends the query to the single <see cref="IQueryHandler{TQuery, TResult}"/> registered for its runtime type.
    /// </summary>
    /// <exception cref="InvalidOperationException">No handler is registered for the query's type.</exception>
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
