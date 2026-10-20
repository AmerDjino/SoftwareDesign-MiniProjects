namespace Inventory.Abstractions;

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Answers the query without changing any state.
    /// </summary>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
