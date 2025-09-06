#nullable enable

using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Continuation delegate for the remaining pipeline and ultimately the request handler.
    /// </summary>
    /// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
}
