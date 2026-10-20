using Inventory.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Dispatching;

public sealed class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _services;

    public Dispatcher(IServiceProvider services)
    {
        _services = services;
    }

    public Task<CommandResult> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);
        var handler = _services.GetRequiredService<ICommandHandler<TCommand>>();
        return handler.HandleAsync(command, cancellationToken);
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var invoker = QueryHandlerInvoker<TResult>.For(query.GetType());
        return invoker.InvokeAsync(query, _services, cancellationToken);
    }
}
