using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Compat.Tests;

public sealed class ScopedTracker : IDisposable
{
    public static int DisposeCount;
    public void Dispose() => Interlocked.Increment(ref DisposeCount);
}

public sealed record UsesScoped() : IRequest<Unit>;

public sealed class UsesScopedHandler(ScopedTracker tracker) : IRequestHandler<UsesScoped, Unit>
{
    public Task<Unit> Handle(UsesScoped request, CancellationToken ct)
    {
        GC.KeepAlive(tracker);
        return Task.FromResult(Unit.Value);
    }
}

public class ScopedLifetimeTests
{
    [Fact]
    public async Task Scoped_dependencies_are_disposed_when_scope_ends()
    {
        ScopedTracker.DisposeCount = 0;

        var sc = new ServiceCollection();
        sc.AddMediatorCompat(typeof(ScopedLifetimeTests).Assembly);
        sc.AddScoped<ScopedTracker>();
        sc.AddTransient<IRequestHandler<UsesScoped, Unit>, UsesScopedHandler>();

        var sp = sc.BuildServiceProvider();
        using (var scope = sp.CreateScope())
        {
            var m = scope.ServiceProvider.GetRequiredService<IMediator>();
            await m.Send(new UsesScoped());
        }

        Assert.Equal(1, ScopedTracker.DisposeCount);
    }
}
