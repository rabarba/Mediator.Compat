# Mediator.Compat

[![NuGet (pre)](https://img.shields.io/nuget/vpre/Mediator.Compat.svg?logo=nuget)](https://www.nuget.org/packages/Mediator.Compat)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Mediator.Compat.svg?logo=nuget)](https://www.nuget.org/packages/Mediator.Compat)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4?logo=.net)](#target-framework--tooling)

Lightweight, **drop-in replacement** for MediatR’s core API  

(`IRequest`, `IMediator`, `INotIFICATION`, `IRequestHandler<>`, `IPipelineBehavior<>`, `Unit`)  

with **native reflection-based scanning**, **deterministic pipeline order**, and **caching for hot paths**.

> **Status:** beta. Core API stable. **Delegate caching** and **single-closure pipeline caching** implemented.

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


public sealed class LoggingBehavior<TReq,TRes> : IPipelineBehavior<TReq,TRes> where TReq : IRequest<TRes>
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

- `AddMediatorCompat(...)` registers **IMediator** and **core internals** and **scans** your assemblies for:
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

## Performance (current baseline)

> Release build, .NET 8, macOS (latest local run). Caching is **enabled** (delegate + pipeline).

- **Send (0 behaviors):** ~**66 ns**, **440 B**
- **Send (+2 behaviors):** ~**86–87 ns**, **656 B**
- **Publish (0 handlers):** ~**21.5–21.7 ns**, **24 B**
- **Publish (+2 handlers):** ~**38.3–38.4 ns**, **144 B**

See **[docs/benchmarks.md](docs/benchmarks.md)** for how to run.

---

## Differences vs MediatR (at a glance)

- ✅ Same **namespace** (`MediatR`) & same **core interfaces** → easy swap.
- ✅ No Scrutor; **native reflection scanning** with duplicate-registration guard.
- ✅ **Deterministic behavior order** (explicit; no accidental closed-type pickup).
- ✅ **Delegate caching** per `(TReq,TRes)` for `Send`.
- ✅ **Single-closure pipeline** to minimize per-call allocations.
- ✅ Auto-registration for open generic INotificationHandler<> and IRequestHandler<,>
- 🚧 `Publish` is sequential-only (parallel mode — backlog/idea).

---

## Target Framework & Tooling

- .NET **8.0+**
- Nullable + analyzers recommended.
- Works great with Rider / VS / VS Code.

---

## Links

- NuGet: https://www.nuget.org/packages/Mediator.Compat  
- GitHub: https://github.com/rabarba/Mediator.Compat

---

## License

MIT © Ugur Kap
