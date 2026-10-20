using System.Collections.Concurrent;
using Inventory.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Dispatching;

/// <summary>
/// Calls the handler of one query type with full static typing. The dispatcher only sees <see cref="IQuery{TResult}"/>,
/// so reflection is needed once per query type to create the matching invoker; every call after that is an ordinary
/// interface call. Invokers hold no state, which is why one cached instance can serve every service provider.
/// </summary>
internal abstract class QueryHandlerInvoker<TResult>
{
    private static readonly ConcurrentDictionary<Type, QueryHandlerInvoker<TResult>> Invokers = new();

    public static QueryHandlerInvoker<TResult> For(Type queryType) => Invokers.GetOrAdd(queryType, Create);

    public abstract Task<TResult> InvokeAsync(
        IQuery<TResult> query,
        IServiceProvider services,
        CancellationToken cancellationToken);

    private static QueryHandlerInvoker<TResult> Create(Type queryType)
    {
        var invokerType = typeof(QueryHandlerInvoker<,>).MakeGenericType(queryType, typeof(TResult));
        return (QueryHandlerInvoker<TResult>)Activator.CreateInstance(invokerType)!;
    }
}

internal sealed class QueryHandlerInvoker<TQuery, TResult> : QueryHandlerInvoker<TResult>
    where TQuery : IQuery<TResult>
{
    public override Task<TResult> InvokeAsync(
        IQuery<TResult> query,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var handler = services.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return handler.HandleAsync((TQuery)query, cancellationToken);
    }
}
