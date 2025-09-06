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

        services.AddSingleton<IMediator, MediatR.Mediator>();
        services.AddTransient<IRequestHandler<Ping, int>, PingHandler>();

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var res = await mediator.Send(new Ping(41));
        Assert.Equal(42, res);
    }
}
