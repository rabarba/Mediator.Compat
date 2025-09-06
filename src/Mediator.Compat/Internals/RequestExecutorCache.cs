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
        return static async (sp, requestObj, ct) =>
        {
            var handler = sp.GetService<IRequestHandler<TReq, TRes>>();
            if (handler is null)
            {
                throw new InvalidOperationException(
                    $"No IRequestHandler<{typeof(TReq).FullName}, {typeof(TRes).FullName}> is registered. " +
                    "Make sure the handler is registered with the DI container (AddMediatorCompat or manual registration).");
            }

            var req = (TReq)requestObj;

            var behaviorsEnum = sp.GetServices<IPipelineBehavior<TReq, TRes>>();
            IPipelineBehavior<TReq, TRes>[] behaviors;
            if (behaviorsEnum is IPipelineBehavior<TReq, TRes>[] arr)
            {
                behaviors = arr;
            }
            else
            {
                var list = new System.Collections.Generic.List<IPipelineBehavior<TReq, TRes>>(4);
                list.AddRange(behaviorsEnum);
                behaviors = list.ToArray();
            }

            var result = await BehaviorChain.Invoke(behaviors, handler, req, ct).ConfigureAwait(false);
            return (object?)result;
        };
    }
}
