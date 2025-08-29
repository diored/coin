namespace DioRed.CoIn;

/// <summary>
/// Synchronous command handler
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    void Handle(TCommand command);
}

/// <summary>
/// Synchronous command handler with result
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
/// <typeparam name="TResult">Result type</typeparam>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    TResult Handle(TCommand command);
}