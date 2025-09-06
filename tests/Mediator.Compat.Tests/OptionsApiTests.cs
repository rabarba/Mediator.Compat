using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Compat.Tests;

public class OptionsApiTests
{
    [Fact]
    public async Task Options_registration_scans_and_respects_behavior_order()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<OrderTracker>();

        sc.AddMediatorCompat(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(BehaviorOrderTests).Assembly);
            cfg.AddOpenBehavior(typeof(B1<,>)); // outer
            cfg.AddOpenBehavior(typeof(B2<,>)); // inner
        });

        var sp = sc.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var tracker  = sp.GetRequiredService<OrderTracker>();

        var res = await mediator.Send(new Ping(41));
        Assert.Equal(42, res);

        Assert.Equal(["B1:before", "B2:before", "B2:after", "B1:after"], tracker.Steps);
    }
}
