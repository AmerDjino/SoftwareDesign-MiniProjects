namespace Inventory.Abstractions;

/// <summary>
/// A request for data that must not change state. The type parameter names the answer's type,
/// which lets <see cref="IDispatcher.QueryAsync{TResult}"/> infer it at the call site.
/// </summary>
/// <typeparam name="TResult">The type of data the query returns.</typeparam>
public interface IQuery<TResult>;
