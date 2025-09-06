# Benchmarks — Mediator.Compat

Baseline microbenchmarks for `Send` and `Publish` with/without behaviors/handlers.  
Caching (**delegate + pipeline**) is enabled in these numbers.

> Environment: Release, .NET 8, macOS (latest local run)

---

## How to run

```bash
dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks
# or a single method:
dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks -- --filter '*Send_Ping*'
# faster job:
dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks -- --job Short --filter '*Send_Ping*'
# list available benchmarks:
dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks -- --list flat
```

BenchmarkDotNet emits reports under:
```
BenchmarkDotNet.Artifacts/results/
```

---

## Results (current)

| Case                           | Mean (ns) | Alloc (B) |
|-------------------------------:|----------:|----------:|
| **Send_Ping (0 behaviors)**    | ~66       | 440       |
| **Send_Ping (+2 behaviors)**   | ~86–87    | 656       |
| **Publish_Note (0 handlers)**  | ~21.5–21.7| 24        |
| **Publish_Note (+2 handlers)** | ~38.3–38.4| 144       |

<sub>Notes: Behavior order is registration order (first = outermost). Publish is sequential by design.</sub>

---
