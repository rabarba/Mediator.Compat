using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Compat.Tests;

public sealed record Ping(int X) : IRequest<int>;

public sealed class PingHandler : IRequestHandler<Ping, int>
{
    public Task<int> Handle(Ping request, CancellationToken ct) => Task.FromResult(request.X + 1);
}

public class SendSmokeTests
{
    [Fact]
    public async Task Send_returns_handler_result()
    {
        var services = new ServiceCollection();

        // ⬇️ This registers IMediator + RequestExecutorCache and scans this test assembly
        services.AddMediatorCompat(typeof(SendSmokeTests).Assembly);

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var res = await mediator.Send(new Ping(41));
        Assert.Equal(42, res);
    }
}
