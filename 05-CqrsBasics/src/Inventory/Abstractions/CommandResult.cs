namespace Inventory.Abstractions;

/// <summary>
/// The outcome of a command: success, or failure with an error message. It never carries data.
/// </summary>
public sealed record CommandResult
{
    private CommandResult(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public string? Error { get; }

    public static CommandResult Success() => new(isSuccess: true, error: null);

    public static CommandResult Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new(isSuccess: false, error);
    }
}
