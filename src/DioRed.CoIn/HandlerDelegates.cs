namespace DioRed.CoIn;

internal delegate void SyncHandlerInvoker(object handler, object command);
internal delegate TResult SyncHandlerInvoker<TResult>(object handler, object command);

internal delegate Task AsyncHandlerInvoker(object handler, object command, CancellationToken ct);
internal delegate Task<TResult> AsyncHandlerInvoker<TResult>(object handler, object command, CancellationToken ct);