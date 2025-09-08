
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Compat.Tests;
public sealed class ScopeToken { public Guid Id { get; } = Guid.NewGuid(); }

public sealed record LeakProbe() : IRequest<Unit>;
public sealed class LeakProbeHandler : IRequestHandler<LeakProbe, Unit>
{
    public static WeakReference? LastTokenWeak;
    private readonly ScopeToken _token;

    public LeakProbeHandler(ScopeToken token) => _token = token;

    public Task<Unit> Handle(LeakProbe request, CancellationToken ct)
    {
        LastTokenWeak = new WeakReference(_token);
        return Task.FromResult(Unit.Value);
    }
}

public class NoLeakTests
{
    [Fact]
    public async Task Executor_cache_does_not_capture_scoped_instances()
    {
        LeakProbeHandler.LastTokenWeak = null;

        var sc = new ServiceCollection();
        sc.AddMediatorCompat(typeof(NoLeakTests).Assembly);
        sc.AddScoped<ScopeToken>();
        sc.AddTransient<IRequestHandler<LeakProbe, Unit>, LeakProbeHandler>();

        var sp = sc.BuildServiceProvider();

        // execute in a scope
        using (var scope = sp.CreateScope())
        {
            var m = scope.ServiceProvider.GetRequiredService<IMediator>();
            await m.Send(new LeakProbe());
        }

        // token should be collectible after scope dispose
        for (var i = 0; i < 20; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            if (LeakProbeHandler.LastTokenWeak is { IsAlive: false }) break;
            await Task.Delay(20);
        }

        Assert.NotNull(LeakProbeHandler.LastTokenWeak);
        Assert.False(LeakProbeHandler.LastTokenWeak!.IsAlive);
    }
}
