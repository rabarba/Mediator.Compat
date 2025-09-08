```

BenchmarkDotNet v0.15.2, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M4 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 8.0.413
  [Host]     : .NET 8.0.19 (8.0.1925.36514), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.19 (8.0.1925.36514), Arm64 RyuJIT AdvSIMD


```
| Method       | Library | BehaviorCount | NotificationHandlerCount | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------------- |-------- |-------------- |------------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| **Send_Ping**    | **Compat**  | **0**             | **0**                        |  **66.96 ns** | **0.214 ns** | **0.200 ns** | **0.0526** |      **-** |     **440 B** |
| Send_Void    | Compat  | 0             | 0                        |  59.31 ns | 0.181 ns | 0.170 ns | 0.0353 |      - |     296 B |
| Publish_Note | Compat  | 0             | 0                        |  21.45 ns | 0.038 ns | 0.035 ns | 0.0029 |      - |      24 B |
| **Send_Ping**    | **Compat**  | **0**             | **2**                        |  **67.60 ns** | **0.171 ns** | **0.151 ns** | **0.0526** |      **-** |     **440 B** |
| Send_Void    | Compat  | 0             | 2                        |  59.71 ns | 0.152 ns | 0.134 ns | 0.0353 |      - |     296 B |
| Publish_Note | Compat  | 0             | 2                        |  40.15 ns | 0.040 ns | 0.037 ns | 0.0172 |      - |     144 B |
| **Send_Ping**    | **Compat**  | **2**             | **0**                        | **105.53 ns** | **0.385 ns** | **0.360 ns** | **0.0956** |      **-** |     **800 B** |
| Send_Void    | Compat  | 2             | 0                        |  90.85 ns | 0.216 ns | 0.202 ns | 0.0612 |      - |     512 B |
| Publish_Note | Compat  | 2             | 0                        |  21.48 ns | 0.045 ns | 0.042 ns | 0.0029 |      - |      24 B |
| **Send_Ping**    | **Compat**  | **2**             | **2**                        | **104.48 ns** | **0.382 ns** | **0.319 ns** | **0.0956** |      **-** |     **800 B** |
| Send_Void    | Compat  | 2             | 2                        |  88.33 ns | 0.228 ns | 0.213 ns | 0.0612 |      - |     512 B |
| Publish_Note | Compat  | 2             | 2                        |  36.88 ns | 0.158 ns | 0.147 ns | 0.0172 |      - |     144 B |
| **Send_Ping**    | **MediatR** | **0**             | **0**                        |  **61.25 ns** | **0.286 ns** | **0.267 ns** | **0.0401** | **0.0001** |     **336 B** |
| Send_Void    | MediatR | 0             | 0                        |  59.98 ns | 0.317 ns | 0.297 ns | 0.0315 | 0.0001 |     264 B |
| Publish_Note | MediatR | 0             | 0                        |  35.05 ns | 0.091 ns | 0.085 ns | 0.0105 | 0.0001 |      88 B |
| **Send_Ping**    | **MediatR** | **0**             | **2**                        |  **65.44 ns** | **0.402 ns** | **0.376 ns** | **0.0401** | **0.0001** |     **336 B** |
| Send_Void    | MediatR | 0             | 2                        |  57.97 ns | 0.259 ns | 0.243 ns | 0.0315 | 0.0001 |     264 B |
| Publish_Note | MediatR | 0             | 2                        |  78.15 ns | 0.351 ns | 0.328 ns | 0.0554 | 0.0001 |     464 B |
| **Send_Ping**    | **MediatR** | **2**             | **0**                        | **124.69 ns** | **0.562 ns** | **0.525 ns** | **0.0975** | **0.0002** |     **816 B** |
| Send_Void    | MediatR | 2             | 0                        | 116.71 ns | 0.575 ns | 0.538 ns | 0.0716 |      - |     600 B |
| Publish_Note | MediatR | 2             | 0                        |  33.77 ns | 0.151 ns | 0.134 ns | 0.0105 | 0.0001 |      88 B |
| **Send_Ping**    | **MediatR** | **2**             | **2**                        | **125.64 ns** | **0.576 ns** | **0.510 ns** | **0.0975** | **0.0002** |     **816 B** |
| Send_Void    | MediatR | 2             | 2                        | 115.83 ns | 0.257 ns | 0.215 ns | 0.0716 |      - |     600 B |
| Publish_Note | MediatR | 2             | 2                        |  79.64 ns | 0.360 ns | 0.337 ns | 0.0554 | 0.0001 |     464 B |
