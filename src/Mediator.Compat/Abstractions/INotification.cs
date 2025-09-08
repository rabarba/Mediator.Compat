#nullable enable

namespace Mediator.Compat
{
    /// <summary>
    /// Represents a one-way notification (event) that does not produce a response.
    /// </summary>
    /// <remarks>
    /// Notifications are published via
    /// <see cref="IMediator.Publish{TNotification}(TNotification, System.Threading.CancellationToken)"/>
    /// and may be handled by zero or more <see cref="INotificationHandler{TNotification}"/> implementations.
    /// </remarks>
    public interface INotification
    {
        // Marker interface: intentionally empty.
    }
}
