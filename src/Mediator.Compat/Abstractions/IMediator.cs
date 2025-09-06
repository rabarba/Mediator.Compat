#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Coordinates the sending of requests and publishing of notifications.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Sends a request to its matching handler through the configured pipeline and returns a response.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="request">The request instance.</param>
        /// <param name="cancellationToken">Token to observe while waiting for completion.</param>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes a notification to all matching handlers. Handlers are invoked sequentially by default.
        /// </summary>
        /// <typeparam name="TNotification">The notification type.</typeparam>
        /// <param name="notification">The notification instance.</param>
        /// <param name="cancellationToken">Token to observe while waiting for completion.</param>
        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }
}
