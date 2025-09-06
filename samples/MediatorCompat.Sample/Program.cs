using MediatR;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register Mediator.Compat: scan this assembly + add a simple logging behavior
builder.Services.AddMediatorCompat(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

var app = builder.Build();

// A tiny endpoint that uses IMediator
app.MapGet("/ping", async (int x, IMediator mediator) =>
{
    var result = await mediator.Send(new Ping(x));
    return Results.Ok(new { input = x, result });
});

app.Run();

// ---------- Sample request/handler/behavior ----------

public sealed record Ping(int X) : IRequest<int>;

public sealed class PingHandler : IRequestHandler<Ping, int>
{
    public Task<int> Handle(Ping request, CancellationToken ct) => Task.FromResult(request.X + 1);
}

public sealed class LoggingBehavior<TReq, TRes>(ILogger<LoggingBehavior<TReq, TRes>> logger)
    : IPipelineBehavior<TReq, TRes>
    where TReq : IRequest<TRes>
{
    public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        logger.LogInformation("Handling {RequestType}", typeof(TReq).Name);
        var res = await next();
        logger.LogInformation("Handled {RequestType}", typeof(TReq).Name);
        return res;
    }
}
