using System;

namespace MiddlewareSharp.Interfaces
{
    /// <summary>
    /// Middleware factory interface used to instantiate and dispose middlewares.
    /// </summary>
    /// <typeparam name="TContext">Context used in middlewares.</typeparam>
    public interface IMiddlewareFactory<TContext>
    {
        /// <summary>
        /// Creates an instance of <see cref="IMiddleware{TContext}"/>.
        /// </summary>
        /// <param name="middlewareType">Type of middleware to instantiate.</param>
        /// <returns>Instantiated middleware.</returns>
        IMiddleware<TContext> Create(Type middlewareType);

		/// <summary>
		/// Creates an instance of <see cref="ICatchMiddleware{TContext}"/>.
		/// </summary>
		/// <param name="middlewareType">Type of middleware to instantiate.</param>
		/// <returns>Instantiated middleware.</returns>
		ICatchMiddleware<TContext> CreateCatch(Type middlewareType);

		/// <summary>
		/// Releases an instance of <see cref="IMiddleware{TContext}"/>
		/// </summary>
		/// <param name="middleware">Middleware to dispose.</param>
		void Release(IMiddleware<TContext> middleware);
	}
}
