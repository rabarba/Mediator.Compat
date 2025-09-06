#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Handles a request of type <typeparamref name="TRequest"/> that does not return a meaningful value.
    /// This is a shorthand for <see cref="IRequestHandler{TRequest, TResponse}"/> with <see cref="Unit"/>.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
        where TRequest : IRequest<Unit>
    {
        // Intentionally empty. The response type is fixed to Unit via the inherited generic interface.
        // Implementors only need to provide:
        // Task<Unit> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
