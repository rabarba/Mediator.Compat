#nullable enable

using Mediator.Compat.Internals;
using Microsoft.Extensions.DependencyInjection;


namespace Mediator.Compat
{
    /// <summary>
    /// Minimal mediator implementation. Resolves handlers/behaviors from the IServiceProvider,
    /// composes the pipeline (outer → inner) and invokes the handler.
    /// Notifications are published sequentially.
    /// </summary>
    internal sealed class Mediator(IServiceProvider provider, RequestExecutorCache executorCache) : IMediator
    {
        private readonly IServiceProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        /// <inheritdoc />
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var exec = executorCache.GetOrAdd(request.GetType(), typeof(TResponse));
            var boxed = await exec(_provider, request, cancellationToken).ConfigureAwait(false);
            return (TResponse)boxed!;
        }

        /// <inheritdoc />
        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            if (notification is null) throw new ArgumentNullException(nameof(notification));

            // Resolve all notification handlers and invoke sequentially
            var handlers = _provider.GetServices<INotificationHandler<TNotification>>();
            foreach (var handler in handlers)
            {
                await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
