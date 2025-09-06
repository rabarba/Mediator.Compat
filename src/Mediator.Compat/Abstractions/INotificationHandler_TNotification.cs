#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Handles a notification of type <typeparamref name="TNotification"/>.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        /// <summary>
        /// Handle the notification.
        /// </summary>
        /// <param name="notification">The notification instance.</param>
        /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
        Task Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
