#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Handles a request of type <typeparamref name="TRequest"/> and produces a response of type <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handle the request and return a response.
        /// </summary>
        /// <param name="request">The request instance.</param>
        /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
