#nullable enable

namespace MediatR
{
    /// <summary>
    /// Represents a request that is handled by a single handler and returns a response of type <typeparamref name="TResponse"/>.
    /// Marker interface only: it carries the request/response type pairing but no members.
    /// </summary>
    /// <typeparam name="TResponse">The response type produced by the matching handler.</typeparam>
    public interface IRequest<out TResponse>
    {
        // Intentionally empty.
        // The presence of TResponse on this marker interface ties the request to its handler's return type.
    }
}
