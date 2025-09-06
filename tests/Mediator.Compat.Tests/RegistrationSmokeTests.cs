using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Compat.Tests;

public class RegistrationSmokeTests
{
    [Fact]
    public async Task Scanning_finds_handlers_and_Send_works()
    {
        var sc = new ServiceCollection();
        sc.AddMediatorCompat(typeof(SendSmokeTests).Assembly); // contains Ping + PingHandler

        var sp = sc.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new Ping(41));
        Assert.Equal(42, result);
    }

    [Fact]
    public void Duplicate_scans_do_not_register_duplicates()
    {
        var sc = new ServiceCollection();
        sc.AddMediatorCompat(typeof(SendSmokeTests).Assembly);
        sc.AddMediatorCompat(typeof(SendSmokeTests).Assembly); // scan twice on purpose

        // ensure only one (ServiceType, ImplType) pair exists for Ping handler
        var count = sc.Count(d =>
            d.ServiceType == typeof(IRequestHandler<Ping, int>) &&
            d.ImplementationType == typeof(PingHandler));

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Open_generic_behaviors_can_be_added_in_order()
    {
        var sc = new ServiceCollection();
        sc.AddMediatorCompat(typeof(BehaviorOrderTests).Assembly); // Ping + PingHandler present
        sc.AddSingleton<OrderTracker>();

        // Outer → Inner
        sc.AddOpenBehavior(typeof(B1<,>));
        sc.AddOpenBehavior(typeof(B2<,>));

        var sp = sc.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var tracker  = sp.GetRequiredService<OrderTracker>();

        var res = await mediator.Send(new Ping(41));
        Assert.Equal(42, res);

        Assert.Equal(["B1:before", "B2:before", "B2:after", "B1:after"], tracker.Steps);
    }
}
