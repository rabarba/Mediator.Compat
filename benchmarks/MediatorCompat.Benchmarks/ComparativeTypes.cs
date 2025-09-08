// -------------- COMPAT --------------
namespace Bench.Messages.Compat
{
    using Mediator.Compat;

    public sealed record Ping(int X) : IRequest<int>;
    public sealed record VoidCmd() : IRequest<Unit>;
    public sealed record Note(string Message) : INotification;

    public sealed class PingHandler : IRequestHandler<Ping, int>
    {
        public Task<int> Handle(Ping req, CancellationToken ct) => Task.FromResult(req.X + 1);
    }

    public sealed class VoidHandler : IRequestHandler<VoidCmd, Unit>
    {
        public Task<Unit> Handle(VoidCmd req, CancellationToken ct) => Task.FromResult(Unit.Value);
    }

    public sealed class NoteHandler1 : INotificationHandler<Note>
    {
        public Task Handle(Note n, CancellationToken ct) => Task.CompletedTask;
    }
    public sealed class NoteHandler2 : INotificationHandler<Note>
    {
        public Task Handle(Note n, CancellationToken ct) => Task.CompletedTask;
    }

    public sealed class BenchmarkBehavior<TReq,TRes> : IPipelineBehavior<TReq,TRes>
        where TReq : IRequest<TRes>
    {
        public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
        {
            // minimal pass-through
            return await next();
        }
    }
}

// -------------- OFFICIAL --------------
namespace Bench.Messages.Official
{
    using MediatR;

    public sealed record Ping(int X) : IRequest<int>;
    public sealed record VoidCmd() : IRequest<Unit>;
    public sealed record Note(string Message) : INotification;

    public sealed class PingHandler : IRequestHandler<Ping, int>
    {
        public Task<int> Handle(Ping req, CancellationToken ct) => Task.FromResult(req.X + 1);
    }

    public sealed class VoidHandler : IRequestHandler<VoidCmd, Unit>
    {
        public Task<Unit> Handle(VoidCmd req, CancellationToken ct) => Task.FromResult(Unit.Value);
    }

    public sealed class NoteHandler1 : INotificationHandler<Note>
    {
        public Task Handle(Note n, CancellationToken ct) => Task.CompletedTask;
    }
    public sealed class NoteHandler2 : INotificationHandler<Note>
    {
        public Task Handle(Note n, CancellationToken ct) => Task.CompletedTask;
    }

    public sealed class BenchmarkBehavior<TReq,TRes> : IPipelineBehavior<TReq,TRes>
        where TReq : IRequest<TRes>
    {
        public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
        {
            return await next();
        }
    }
}
