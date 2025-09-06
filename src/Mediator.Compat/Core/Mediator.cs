#nullable enable

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR
{
    /// <summary>
    /// Minimal mediator implementation. Resolves handlers/behaviors from the IServiceProvider,
    /// composes the pipeline (outer → inner) and invokes the handler.
    /// Notifications are published sequentially.
    /// </summary>
    internal sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _provider;

        public Mediator(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            // Close the generic SendCore<TRequest, TResponse> using the runtime request type.
            var requestType = request.GetType();
            var method = typeof(Mediator)
                .GetMethod(nameof(SendCore), BindingFlags.Instance | BindingFlags.NonPublic)!
                .MakeGenericMethod(requestType, typeof(TResponse));

            return (Task<TResponse>)method.Invoke(this, new object[] { request, cancellationToken })!;
        }

        /// <summary>
        /// Closed-generic worker for Send. Builds the pipeline and invokes the handler.
        /// </summary>
        private Task<TResponse> SendCore<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
        {
            // Resolve the single request handler
            var handler = _provider.GetService<IRequestHandler<TRequest, TResponse>>();
            if (handler is null)
                throw new InvalidOperationException(
                    $"No IRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> is registered.");

            // Resolve behaviors in registration order (outer → inner)
            var behaviors = _provider.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToList();

            // Terminal delegate = call the handler
            RequestHandlerDelegate<TResponse> terminal = () => handler.Handle(request, cancellationToken);

            // Compose behaviors from inner to outer
            for (int i = behaviors.Count - 1; i >= 0; i--)
            {
                var next = terminal;
                var behavior = behaviors[i];
                terminal = () => behavior.Handle(request, next, cancellationToken);
            }

            return terminal();
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
