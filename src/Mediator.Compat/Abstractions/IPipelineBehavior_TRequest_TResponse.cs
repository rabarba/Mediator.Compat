#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines a cross-cutting behavior that runs around the handling of a request.
    /// Registration order == execution order (first registered is the outermost).
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public interface IPipelineBehavior<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Executes the behavior logic. Call <paramref name="next"/> to invoke the remaining pipeline and the handler.
        /// Behaviors may short-circuit by not calling <paramref name="next"/>, but this should be used sparingly.
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <param name="next">Continuation delegate to the rest of the pipeline and ultimately the handler.</param>
        /// <param name="cancellationToken">Token to observe while awaiting completion.</param>
        Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }
}
