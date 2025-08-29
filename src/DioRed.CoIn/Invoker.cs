namespace DioRed.CoIn;

public interface IInvoker
{
    void Invoke(ICommand command);
    TResult Invoke<TResult>(ICommand<TResult> command);

    Task InvokeAsync(ICommand command, CancellationToken ct = default);
    Task<TResult> InvokeAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}

public class Invoker(IServiceProvider provider) : IInvoker
{
    public void Invoke(ICommand command)
    {
        if (TryInvokeSync(command))
        {
            return;
        }

        InvokeAsync(command).GetAwaiter().GetResult();
    }

    public TResult Invoke<TResult>(ICommand<TResult> command)
    {
        if (TryInvokeSync(command, out TResult result))
        {
            return result;
        }

        return InvokeAsync(command).GetAwaiter().GetResult();
    }

    public Task InvokeAsync(ICommand command, CancellationToken ct = default)
    {
        if (TryInvokeAsync(command, ct, out var task))
        {
            return task;
        }

        Invoke(command);
        return Task.CompletedTask;
    }

    public Task<TResult> InvokeAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        if (TryInvokeAsync(command, ct, out Task<TResult>? task))
        {
            return task;
        }

        var result = Invoke(command);
        return Task.FromResult(result);
    }

    #region Private helpers

    private bool TryInvokeSync(ICommand command)
    {
        var cmdType = command.GetType();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(cmdType);
        var handler = provider.GetService(handlerType);

        if (handler is null)
        {
            return false;
        }

        var invoker = HandlerInvokerFactory.GetInvoker<SyncHandlerInvoker>(
            handlerType,
            cmdType,
            resultType: null,
            isAsync: false
        );

        invoker(handler, command);

        return true;
    }

    private bool TryInvokeSync<TResult>(ICommand<TResult> command, out TResult result)
    {
        var cmdType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(cmdType, typeof(TResult));
        var handler = provider.GetService(handlerType);

        if (handler is null)
        {
            result = default!;
            return false;
        }

        var invoker = HandlerInvokerFactory.GetInvoker<SyncHandlerInvoker<TResult>>(
            handlerType,
            cmdType,
            resultType: typeof(TResult),
            isAsync: false
        );

        result = invoker(handler, command);

        return true;
    }

    private bool TryInvokeAsync(ICommand command, CancellationToken ct, out Task task)
    {
        var cmdType = command.GetType();
        var handlerType = typeof(IAsyncCommandHandler<>).MakeGenericType(cmdType);
        var handler = provider.GetService(handlerType);

        if (handler is null)
        {
            task = null!;
            return false;
        }

        var invoker = HandlerInvokerFactory.GetInvoker<AsyncHandlerInvoker>(
            handlerType,
            cmdType,
            resultType: null,
            isAsync: true
        );

        task = invoker(handler, command, ct);

        return true;
    }

    private bool TryInvokeAsync<TResult>(ICommand<TResult> command, CancellationToken ct, out Task<TResult> task)
    {
        var cmdType = command.GetType();
        var handlerType = typeof(IAsyncCommandHandler<,>).MakeGenericType(cmdType, typeof(TResult));
        var handler = provider.GetService(handlerType);

        if (handler is null)
        {
            task = null!;
            return false;
        }

        var invoker = HandlerInvokerFactory.GetInvoker<AsyncHandlerInvoker<TResult>>(
            handlerType,
            cmdType,
            resultType: typeof(TResult),
            isAsync: true
        );

        task = invoker(handler, command, ct);

        return true;
    }

    #endregion
}