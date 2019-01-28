using System;
using System.Threading.Tasks;

namespace MiddlewareSharp.Interfaces
{
	/// <summary>
	/// Middleware executed by <see cref="Flow{TContext}"/>, when any of previous middlewares throw an exception.
	/// </summary>
	/// <typeparam name="TContext">Context used by middlewares.</typeparam>
	public interface ICatchMiddleware<TContext>
	{
		/// <summary>
		/// Invokes the middleware.
		/// </summary>
		/// <param name="context">Context used by middleware.</param>
		/// <param name="exception">Exception throwed from last middleware.</param>
		/// <param name="next">Next middleware invocation in queue.</param>
		/// <returns></returns>
		Task InvokeAsync(TContext context, MiddlewareException<TContext> exception, RequestDelegate<TContext> next);
	}
}
