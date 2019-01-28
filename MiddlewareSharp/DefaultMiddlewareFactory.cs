using System;
using Microsoft.Extensions.DependencyInjection;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    /// <inheritdoc />
    /// <summary>
    /// Default implementation of the <see cref="IMiddlewareFactory{TContext}" />.
    /// </summary>
    /// <typeparam name="TContext">Context used in middlewares.</typeparam>
    public class DefaultMiddlewareFactory<TContext> : IMiddlewareFactory<TContext>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates an instance of factory with provided <see cref="IServiceProvider"/> for dependency injection.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> for dependency injection.></param>
        public DefaultMiddlewareFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public virtual IMiddleware<TContext> Create(Type middlewareType)
        {
            return _serviceProvider.GetRequiredService(middlewareType) as IMiddleware<TContext>;
        }

		/// <inheritdoc />
		public virtual ICatchMiddleware<TContext> CreateCatch(Type middlewareType)
		{
			return _serviceProvider.GetRequiredService(middlewareType) as ICatchMiddleware<TContext>;
		}

		/// <inheritdoc />
		public virtual void Release(IMiddleware<TContext> middleware)
        {
        }
    }
}
