using System;
using System.Threading.Tasks;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    /// <summary>
    /// Flow built by <see cref="IFlowBuilder{TContext}"/>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public sealed class Flow<TContext> : IFlow<TContext>
        where TContext : new()
    {
        private readonly MiddlewareDelegate<TContext> _start;

        internal Flow(MiddlewareDelegate<TContext> start)
        {
            _start = start;
        }

        /// <summary>
        /// Invokes added <see cref="IMiddleware{TContext}"/> in order they were added by <see cref="IFlowBuilder{TContext}"/>.
        /// Middlewares and <see cref="TContext"/> are resolved by <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> for resolving middlewares and <see cref="TContext"/>.</param>
        /// <returns>Awaitable <see cref="Task{TContext}"/> for asynchronous middleware invocation.</returns>
        public Task<TContext> InvokeAsync(IServiceProvider serviceProvider)
        {
            var context = (TContext) serviceProvider.GetService(typeof(TContext));
            return InvokeAsync(context, serviceProvider);
        }

        /// <summary>
        /// Invokes added <see cref="IMiddleware{TContext}"/> in order they were added by <see cref="IFlowBuilder{TContext}"/>.
        /// Middlewares are resolved by <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> for resolving middlewares.</param>
        /// <param name="context"><see cref="TContext"/> for middleware execution.</param>
        /// <returns>Awaitable <see cref="Task{TContext}"/> for asynchronous middleware invocation.</returns>
        public async Task<TContext> InvokeAsync(TContext context, IServiceProvider serviceProvider)
        {
            var factory = serviceProvider.GetService(typeof(IMiddlewareFactory<TContext>)) as IMiddlewareFactory<TContext>;
            await _start.Invoke(context, factory);
            return context;
        }
    }
}
