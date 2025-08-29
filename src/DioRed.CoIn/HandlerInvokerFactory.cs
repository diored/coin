using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace DioRed.CoIn;

internal static class HandlerInvokerFactory
{
    private static readonly ConcurrentDictionary<string, Delegate> _cache = new();

    public static TDelegate GetInvoker<TDelegate>(
        Type handlerType,
        Type commandType,
        Type? resultType,
        bool isAsync)
        where TDelegate : Delegate
    {
        var delegateType = typeof(TDelegate);

        var key = $"{handlerType.FullName}|{commandType.FullName}|{resultType?.FullName}|{isAsync}";

        return (TDelegate)_cache.GetOrAdd(key, _ =>
        {
            var handlerParam = Expression.Parameter(typeof(object), "handler");
            var commandParam = Expression.Parameter(typeof(object), "command");
            var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

            var typedHandler = Expression.Convert(handlerParam, handlerType);
            var typedCommand = Expression.Convert(commandParam, commandType);

            MethodInfo method = isAsync
                ? handlerType.GetMethod(
                    nameof(IAsyncCommandHandler<>.HandleAsync),
                    [commandType, typeof(CancellationToken)]
                  )!
                : handlerType.GetMethod(
                    nameof(ICommandHandler<>.Handle),
                    [commandType]
                  )!;

            Expression call = isAsync
                ? Expression.Call(typedHandler, method, typedCommand, ctParam)
                : Expression.Call(typedHandler, method, typedCommand);

            return Expression.Lambda(delegateType,
                call,
                isAsync
                    ? [handlerParam, commandParam, ctParam]
                    : [handlerParam, commandParam]
            ).Compile();
        });
    }
}