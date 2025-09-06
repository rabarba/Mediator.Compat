#nullable enable

namespace MediatR
{
    /// <summary>
    /// Represents a request that does not return a meaningful value.
    /// This is a shorthand for <see cref="IRequest{TResponse}"/> with <see cref="Unit"/>.
    /// </summary>
    /// <remarks>
    /// Prefer the explicit generic form <c>IRequest&lt;Unit&gt;</c> when you want to be crystal clear
    /// about the response type, but this non-generic alias keeps call sites concise.
    /// </remarks>
    public interface IRequest : IRequest<Unit>
    {
        // Intentionally empty. The response type is fixed to Unit via the inherited generic interface.
    }
}
