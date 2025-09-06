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
- ✅ No Scrutor; **native reflection scanning** with duplicate-registration guard.
- ✅ **Deterministic behavior order** (explicit; no accidental closed-type pickup).
- 🚧 No parallel `Publish` option yet (roadmap).
- 🚧 No delegate/pipeline caching yet (roadmap).

---

## Target Framework & Tooling

- .NET **8.0+**
- Nullable enabled, analyzers on (recommended).
- Works great with Rider / VS / VS Code.

---

## Roadmap

- [ ] **Delegate caching** for `Send<T>` (avoid reflection per call)
- [ ] **Pipeline caching** per closed `TReq/TRes`
- [ ] Optional **parallel publish** mode
- [ ] SourceLink + symbol packages polish
- [ ] Additional samples (Minimal API, MVC, Console)
- [ ] Benchmarks doc & regression guard

---

## Benchmarks (local)

> Optional for now.

```bash
# once a benchmarks project is added
dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks
```

Capture results and add a short table to `docs/benchmarks.md`.

---

## Contributing

- Keep public surface minimal (Abstractions) and well-documented (XML).
- Add focused unit tests for behaviors, order, and errors.
- Conventional commits are appreciated (`feat:`, `fix:`, `docs:` …).

---

## License

MIT © Ugur Kap
