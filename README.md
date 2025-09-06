# Mediator.Compat

Lightweight, **drop-in replacement** for MediatR’s core API  
(`IRequest`, `IMediator`, `INotification`, `IRequestHandler<>`, `IPipelineBehavior<>`, `Unit`)  
with **reflection-based scanning** (no Scrutor), **predictable pipeline order**, and **clear errors**.

> **Status:** active WIP. Public contracts are stable; core mediator and DI scanning are implemented.

---

## Install

```bash
dotnet add package Mediator.Compat
```

From source:
1) Add a project reference to `src/Mediator.Compat/Mediator.Compat.csproj`.  
2) Keep `using MediatR;` in your code (same namespace as MediatR).

---

## Quick Start

```csharp
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatorCompat(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); // scan handlers/notifications
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));            // pipeline (outer → inner)
});

var app = builder.Build();

app.MapGet("/ping", async (int x, IMediator mediator) =>
{
    var result = await mediator.Send(new Ping(x));
    return Results.Ok(result);
});

app.Run();

// ---- app code ----
public sealed record Ping(int X) : IRequest<int>;

public sealed class PingHandler : IRequestHandler<Ping, int>
{
    public Task<int> Handle(Ping req, CancellationToken ct) => Task.FromResult(req.X + 1);
}

public sealed class LoggingBehavior<TReq,TRes> : IPipelineBehavior<TReq,TRes>
    where TReq : IRequest<TRes>
{
    private readonly ILogger<LoggingBehavior<TReq, TRes>> _log;
    public LoggingBehavior(ILogger<LoggingBehavior<TReq, TRes>> log) => _log = log;

    public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        _log.LogInformation("Handling {Request}", typeof(TReq).Name);
        var res = await next();
        _log.LogInformation("Handled {Request}", typeof(TReq).Name);
        return res;
    }
}
```

---

## How DI registration works

- `AddMediatorCompat(...)` registers `IMediator` and **scans** your assemblies for:
  - `IRequestHandler<TReq,TRes>`
  - `INotificationHandler<TNote>`
- **Behaviors are not auto-registered**. Add them **explicitly** to control order:
  - `services.AddOpenBehavior(typeof(ValidationBehavior<,>));`
- **Order matters**: registration order == execution order (**first = outermost**).

---

## Behavior pipeline (mental model)

```
Caller → [Behavior #1] → [Behavior #2] → … → [Handler] → Result
               ↑ outer                          ↑ inner
```

- Behaviors may short-circuit (don’t call `next()`), but use sparingly.
- `CancellationToken` flows through every step.

---

## Error semantics

- Missing request handler:
  - Throws `InvalidOperationException` with concrete generic types in the message.
- `Publish` is **sequential** and **no-op** if there are no handlers.
- Exceptions bubble up unchanged (no hidden wrapping).

---

## Differences vs MediatR (at a glance)

- ✅ Same **namespace** (`MediatR`) & same **core interfaces** → easy swap.
- ✅ **Native reflection scanning** with duplicate-registration guard.
- ✅ **Deterministic behavior order** (explicit; no accidental closed-type pickup).
- 🚧 No delegate/pipeline caching yet (roadmap).

---

## Benchmarks

- 📊 See detailed results and how to run them in **[docs/benchmarks.md](docs/benchmarks.md)**.
- TL;DR of current baseline (Release):  
  - `Send` ~**215–222 ns** (no behaviors), +**~20–27 ns/behavior**  
  - `Publish` ~**22 ns** (0 handlers), +**~8–9 ns/handler**

---

## Roadmap

- [ ] **Delegate caching** for `Send<T>` (avoid reflection per call)
- [ ] **Pipeline caching** per closed `TReq/TRes`

---

## License

MIT © Ugur Kap
