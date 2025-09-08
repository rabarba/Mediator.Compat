using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Mediator.Compat;

namespace MediatorCompat.Benchmarks;

// Keep it short to finish fast; later you can switch to MediumRun/Default
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 6, iterationCount: 15, launchCount: 1)]
[MemoryDiagnoser]
public class Scenarios
{
    // Tunable parameters
    [Params(0, 2)]
    public int BehaviorCount { get; set; }

    [Params(0, 2)]
    public int NotificationHandlerCount { get; set; }

    private IServiceProvider _sp = null!;
    private IMediator _mediator = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Avoid scanning our benchmark assembly to keep full control over what gets registered.
        // We pass a "blank" assembly (mscorlib) so scanner finds nothing, then register manually.
        services.AddMediatorCompat(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(object).Assembly); // blank
            if (BehaviorCount >= 1) cfg.AddOpenBehavior(typeof(B1<,>));
            if (BehaviorCount >= 2) cfg.AddOpenBehavior(typeof(B2<,>));
        });

        // Request handlers (manual)
        services.AddTransient<IRequestHandler<Ping, int>, PingHandler>();
        services.AddTransient<IRequestHandler<VoidCmd, Unit>, VoidHandler>();

        // Notification handlers (manual; control count by param)
        if (NotificationHandlerCount >= 1)
            services.AddTransient<INotificationHandler<Note>, NoteHandler1>();
        if (NotificationHandlerCount >= 2)
            services.AddTransient<INotificationHandler<Note>, NoteHandler2>();

        // Logging for behavior ctor (no-op logger)
        services.AddSingleton(typeof(NullLogger<>));

        _sp = services.BuildServiceProvider();
        _mediator = _sp.GetRequiredService<IMediator>();
    }

    // --- Benchmarks ---

    [Benchmark]
    public Task<int> Send_Ping() => _mediator.Send(new Ping(41));

    [Benchmark]
    public Task<Unit> Send_Void() => _mediator.Send(new VoidCmd());

    [Benchmark]
    public Task Publish_Note() => _mediator.Publish(new Note("n"));

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_sp is IDisposable d) d.Dispose();
    }
}

// --- Requests / Notifications ---

public sealed record Ping(int X) : IRequest<int>;
public sealed record VoidCmd() : IRequest<Unit>;
public sealed record Note(string Message) : INotification;

public sealed class PingHandler : IRequestHandler<Ping, int>
{
    public Task<int> Handle(Ping request, CancellationToken ct) => Task.FromResult(request.X + 1);
}

public sealed class VoidHandler : IRequestHandler<VoidCmd, Unit>
{
    public Task<Unit> Handle(VoidCmd request, CancellationToken ct) => Task.FromResult(Unit.Value);
}

public sealed class NoteHandler1 : INotificationHandler<Note>
{
    public Task Handle(Note notification, CancellationToken ct) => Task.CompletedTask;
}

public sealed class NoteHandler2 : INotificationHandler<Note>
{
    public Task Handle(Note notification, CancellationToken ct) => Task.CompletedTask;
}

// --- Behaviors (no-op with tiny overhead) ---

public sealed class B1<TReq, TRes> : IPipelineBehavior<TReq, TRes> where TReq : IRequest<TRes>
{
    public Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct) => next();
}

public sealed class B2<TReq, TRes> : IPipelineBehavior<TReq, TRes> where TReq : IRequest<TRes>
{
    public Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct) => next();
}
