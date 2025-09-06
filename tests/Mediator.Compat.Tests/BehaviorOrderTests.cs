using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Compat.Tests;

// Test helper: capture call order
public class OrderTracker
{
    private readonly List<string> _steps = new();
    public void Log(string step) => _steps.Add(step);
    public IReadOnlyList<string> Steps => _steps;
}

// Pipeline behaviors (open generic)
public sealed class B1<TReq, TRes>(OrderTracker t) : IPipelineBehavior<TReq, TRes>
    where TReq : IRequest<TRes>
{
    public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        t.Log("B1:before");
        var res = await next();
        t.Log("B1:after");
        return res;
    }
}

public sealed class B2<TReq, TRes>(OrderTracker t) : IPipelineBehavior<TReq, TRes>
    where TReq : IRequest<TRes>
{
    public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        t.Log("B2:before");
        var res = await next();
        t.Log("B2:after");
        return res;
    }
}

public sealed class ShortCircuitPing(OrderTracker t) : IPipelineBehavior<Ping, int>
{
    public Task<int> Handle(Ping request, RequestHandlerDelegate<int> next, CancellationToken ct)
    {
        t.Log("SHORT:before");
        return Task.FromResult(999);
    }
}

public class BehaviorOrderTests
{
    [Fact]
    public async Task Behaviors_execute_in_registration_order_outer_to_inner()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IMediator, MediatR.Mediator>();
        sc.AddSingleton<OrderTracker>();
        sc.AddTransient(typeof(IPipelineBehavior<,>), typeof(B1<,>)); // outer
        sc.AddTransient(typeof(IPipelineBehavior<,>), typeof(B2<,>)); // inner
        sc.AddTransient<IRequestHandler<Ping, int>, PingHandler>();   // from SendSmokeTests

        var sp = sc.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var tracker = sp.GetRequiredService<OrderTracker>();

        var result = await mediator.Send(new Ping(41));
        Assert.Equal(42, result);

        Assert.Equal(["B1:before", "B2:before", "B2:after", "B1:after"], tracker.Steps);
    }

    [Fact]
    public async Task Behavior_can_short_circuit_and_skip_handler()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IMediator, MediatR.Mediator>();
        sc.AddSingleton<OrderTracker>();
        sc.AddTransient<IPipelineBehavior<Ping, int>, ShortCircuitPing>(); // short-circuit
        sc.AddTransient<IRequestHandler<Ping, int>, PingHandler>();       // would be skipped

        var sp = sc.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var tracker = sp.GetRequiredService<OrderTracker>();

        var result = await mediator.Send(new Ping(0));
        Assert.Equal(999, result);                         // came from behavior, not handler
        Assert.Equal(["SHORT:before"], tracker.Steps);
    }
}
