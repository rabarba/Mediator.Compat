# Mediator.Compat

[![NuGet (pre)](https://img.shields.io/nuget/vpre/Mediator.Compat.svg?logo=nuget)](https://www.nuget.org/packages/Mediator.Compat)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Mediator.Compat.svg?logo=nuget)](https://www.nuget.org/packages/Mediator.Compat)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4?logo=.net)](#target-framework--tooling)

Lightweight, **drop-in replacement** for MediatR’s core API  

(`IRequest`, `IMediator`, `INotification`, `IRequestHandler<>`, `IPipelineBehavior<>`, `Unit`)  

with **native reflection-based scanning**, **deterministic pipeline order**, and **caching for hot paths**.

---

## Install

```bash
dotnet add package Mediator.Compat
```
---

## Quick Start

```csharp
using Mediator.Compat;

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

## Comparative Benchmarks (vs MediatR 12.x)

> **Environment:** macOS (Apple M4 Pro), .NET 8, BenchmarkDotNet *DefaultJob*.  
> **Matrix:** `BehaviorCount ∈ {0, 2}`, `NotificationHandlerCount ∈ {0, 2}`.  
> Full raw output is captured in `docs/benchmarks.md` (CSV/table).

### Headline results

- **Send (Ping, 0 behaviors)**  
  - `Mediator.Compat`: **66.96 ns / 440 B**  
  - `MediatR`: **61.25 ns / 336 B**  
  → MediatR is ~**9%** faster; Compat allocates **+104 B** more. (Baseline “no-pipeline” fast path is our next optimization target.)

- **Send (Ping, 2 behaviors)**  
  - `Mediator.Compat`: **~105 ns / 800 B**  
  - `MediatR`: **~125 ns / 816 B**  
  → Compat is ~**16–17%** faster; **−16 B** less allocation. (Single-closure pipeline pays off.)

- **Send (Void, 0 behaviors)**  
  - `Mediator.Compat`: **59.31 ns / 296 B**  
  - `MediatR`: **59.98 ns / 264 B**  
  → Essentially a tie on time; Compat is **+32 B**.

- **Send (Void, 2 behaviors)**  
  - `Mediator.Compat`: **~88–91 ns / 512 B**  
  - `MediatR`: **~116 ns / 600 B**  
  → Compat is ~**22–24%** faster; **−88 B** less allocation.

- **Publish (Note, 0 handlers)**  
  - `Mediator.Compat`: **21.45 ns / 24 B**  
  - `MediatR`: **35.05 ns / 88 B**  
  → Compat is ~**39%** faster; **−64 B** allocation.

- **Publish (Note, 2 handlers)**  
  - `Mediator.Compat`: **~36.9–40.2 ns / 144 B**  
  - `MediatR`: **~78.2–79.6 ns / 464 B**  
  → Compat is **~2× faster** with **~3× less** allocation.

> **Run locally**
>
> ```bash
> dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks
> ```

### Takeaways

- With **pipeline behaviors enabled (≥1)**, `Mediator.Compat` is consistently **faster** and allocates **less**.  
- For **Publish**, Compat leads both in time and allocation (sequential publish mode).  
- In the **baseline “no-behavior”** Send case, MediatR is slightly ahead; we plan a dedicated **no-pipeline fast path** to close the gap.

### Next optimization targets

- Introduce a **no-behavior fast path** (skip pipeline construction entirely).  
- Cache handler factories (`Func<IServiceProvider, THandler>`) to shave a few more nanoseconds off baseline.  
- Continue allocation dieting in baseline to approach **≤336 B**.

See **[/docs/benchmarks.md](https://github.com/rabarba/Mediator.Compat/blob/main/docs/benchmarks.md)** for how to run.

---

## Differences vs MediatR (at a glance)

- ✅ same **core interfaces** → easy swap.
- ✅ **native reflection scanning** with duplicate-registration guard.
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
