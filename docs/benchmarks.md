# Benchmarks — Mediator.Compat

This page captures a first pass of microbenchmarks for `Send` and `Publish` with different pipeline sizes.

> ✅ Goal: establish a clean baseline **before** delegate/pipeline caching optimizations.

---

## How to run

```bash
dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks
# or a single method:
dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks -- --filter *Send_Ping*
# faster job:
dotnet run -c Release -p benchmarks/MediatorCompat.Benchmarks -- --job Short --filter *Send_Ping*
```

BenchmarkDotNet will emit markdown reports under:
```
BenchmarkDotNet.Artifacts/results/
```

---

## Results (current baseline)

> Machine/environment details are omitted here for brevity. Keep using `-c Release`.  
> All numbers below are from your latest run.

| Method        | BehaviorCount | NotificationHandlerCount | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------------|---------------:|--------------------------:|-----------:|---------:|---------:|-------:|----------:|
| Send_Ping     | 0             | 0                        | 221.56 ns | 1.196 ns | 1.118 ns | 0.0648 | 544 B     |
| Send_Void     | 0             | 0                        | 215.33 ns | 1.307 ns | 1.092 ns | 0.0563 | 472 B     |
| Publish_Note  | 0             | 0                        | 21.70 ns  | 0.102 ns | 0.090 ns | 0.0029 | 24 B      |
| Send_Ping     | 0             | 2                        | 218.43 ns | 1.242 ns | 1.162 ns | 0.0648 | 544 B     |
| Send_Void     | 0             | 2                        | 216.15 ns | 1.899 ns | 1.776 ns | 0.0563 | 472 B     |
| Publish_Note  | 0             | 2                        | 38.46 ns  | 0.183 ns | 0.162 ns | 0.0172 | 144 B     |
| Send_Ping     | 2             | 0                        | 259.89 ns | 1.897 ns | 1.775 ns | 0.1049 | 880 B     |
| Send_Void     | 2             | 0                        | 268.46 ns | 1.787 ns | 1.672 ns | 0.0963 | 808 B     |
| Publish_Note  | 2             | 0                        | 35.80 ns  | 0.214 ns | 0.200 ns | 0.0029 | 24 B      |
| Send_Ping     | 2             | 2                        | 260.53 ns | 2.129 ns | 1.992 ns | 0.1049 | 880 B     |
| Send_Void     | 2             | 2                        | 257.23 ns | 1.426 ns | 1.334 ns | 0.0963 | 808 B     |
| Publish_Note  | 2             | 2                        | 38.57 ns  | 0.148 ns | 0.138 ns | 0.0172 | 144 B     |

---

## Quick takeaways

- **Send (no behaviors):** ~215–222 ns, **472–544 B** alloc.
- **Send (+2 behaviors):** ~257–269 ns, adds ~**20–27 ns per behavior**, ~**168 B per behavior**.
- **Publish:** ~22 ns with 0 handlers; with 2 handlers ~38–39 ns (**~8–9 ns / handler**), allocation grows linearly.

These numbers are healthy for an MVP that:
- closes generics via reflection on each `Send<T>`,
- builds the behavior chain per call,
- resolves handler/behaviors from DI each time.

---

## Next optimizations (roadmap impact)

- **Delegate caching:** pre-close and cache an executor per `(TRequest,TResponse)` to avoid reflection per call.
- **Pipeline caching:** pre-compose behavior chains per `(TRequest,TResponse)`; at call time just resolve instances and invoke.
- **Optional parallel publish:** keep sequential as default; measure impact separately.

When these land, re-run and add a second table for A/B comparison.

---
