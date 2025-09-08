namespace Mediator.Compat.Internals;

internal static class BehaviorChain
{
    // O(1) closure: index/req/ct/handler/behaviors tek bir closure içinde tutulur.
    public static Task<TRes> Invoke<TReq, TRes>(
        IPipelineBehavior<TReq, TRes>[] behaviors,
        IRequestHandler<TReq, TRes> handler,
        TReq req,
        CancellationToken ct)
        where TReq : IRequest<TRes>
    {
        var index = -1;

        return Next();

        Task<TRes> Next()
        {
            index++;
            return (uint)index >= (uint)behaviors.Length ? handler.Handle(req, ct) : behaviors[index].Handle(req, Next, ct);
        }
    }
}
