namespace MediatR.Internals;

using System;
using System.Threading;
using System.Threading.Tasks;

internal delegate Task<object?> BoxedExecutor(IServiceProvider sp, object request, CancellationToken ct);
