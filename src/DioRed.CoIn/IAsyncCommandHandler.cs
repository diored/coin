namespace DioRed.CoIn;

/// <summary>
/// Asynchronous command handler
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
public interface IAsyncCommandHandler<TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Asynchronous command handler with result
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
/// <typeparam name="TResult">Result type</typeparam>
public interface IAsyncCommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}