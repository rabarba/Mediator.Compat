using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Internals;
internal sealed class RequestExecutorCache
{
    private readonly ConcurrentDictionary<TypeKey, BoxedExecutor> _map = new();
    private static readonly MethodInfo SBuild =
        typeof(RequestExecutorCache).GetMethod(nameof(BuildExecutor), BindingFlags.NonPublic | BindingFlags.Static)!;

    public BoxedExecutor GetOrAdd(Type requestType, Type responseType)
    {
        var key = new TypeKey(requestType, responseType);
        return _map.GetOrAdd(key, _ => (BoxedExecutor)SBuild.MakeGenericMethod(requestType, responseType).Invoke(null, null)!);
    }

    // One-time generic close; no reflection per call after this.
    private static BoxedExecutor BuildExecutor<TReq, TRes>() where TReq : IRequest<TRes>
    {
        return async (sp, requestObj, ct) =>
        {
            var handler = sp.GetService<IRequestHandler<TReq, TRes>>();
            if (handler is null)
            {
                // Keep your existing meaningful message pattern
                throw new InvalidOperationException(
                    $"No IRequestHandler<{typeof(TReq).FullName}, {typeof(TRes).FullName}> is registered. " +
                    "Make sure the handler is registered with the DI container (AddMediatorCompat or manual registration).");
            }

            var req = (TReq)requestObj;
            var behaviors = sp.GetServices<IPipelineBehavior<TReq, TRes>>();

            RequestHandlerDelegate<TRes> next = () => handler.Handle(req, ct);

            // registration order = execution order (first = outermost)
            // DI returns in registration order → wrap in reverse
            foreach (var b in behaviors.Reverse())
            {
                var current = next;
                next = () => b.Handle(req, current, ct);
            }

            var result = await next().ConfigureAwait(false);
            return (object?)result;
        };
    }
}
