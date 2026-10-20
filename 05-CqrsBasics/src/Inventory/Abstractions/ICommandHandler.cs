namespace Inventory.Abstractions;

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Executes the command. A failure result means nothing was changed.
    /// </summary>
    Task<CommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
