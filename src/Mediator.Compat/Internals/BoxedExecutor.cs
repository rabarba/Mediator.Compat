namespace Mediator.Compat.Internals;

internal delegate Task<object?> BoxedExecutor(IServiceProvider sp, object request, CancellationToken ct);
