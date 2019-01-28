using System.Threading.Tasks;

namespace MiddlewareSharp.Interfaces
{
    /// <summary>
    /// Middleware executed by <see cref="Flow{TContext}"/>.
    /// </summary>
    /// <typeparam name="TContext">Context used by middlewares.</typeparam>
    public interface IMiddleware<TContext>
	{
        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">Context used by middleware.</param>
        /// <param name="next">Next middleware invocation in queue.</param>
        /// <returns></returns>
        Task InvokeAsync(TContext context, RequestDelegate<TContext> next);
    }
}
