namespace Inventory.Abstractions;

/// <summary>
/// A request to change state. It is handled by exactly one <see cref="ICommandHandler{TCommand}"/>
/// and answered with a <see cref="CommandResult"/>, never with data.
/// </summary>
public interface ICommand;
