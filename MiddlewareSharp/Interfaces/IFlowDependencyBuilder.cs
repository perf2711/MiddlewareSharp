namespace MiddlewareSharp.Interfaces
{
    public interface IFlowDependencyBuilder<TContext>
    {
        /// <summary>
        /// Adds scoped <see cref="DefaultMiddlewareFactory{TContext}"/> factory.
        /// </summary>
        /// <returns><see cref="IFlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        IFlowDependencyBuilder<TContext> WithDefaultMiddlewareFactory();

        /// <summary>
        /// Adds scoped factory.
        /// </summary>
        /// <typeparam name="TFactory">Factory type.</typeparam>
        /// <returns><see cref="IFlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        IFlowDependencyBuilder<TContext> WithMiddlewareFactory<TFactory>() where TFactory : class, IMiddlewareFactory<TContext>;

        /// <summary>
        /// Adds scoped middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Middleware type.</typeparam>
        /// <returns><see cref="IFlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        IFlowDependencyBuilder<TContext> WithMiddleware<TMiddleware>() where TMiddleware : class, IMiddleware<TContext>;
    }
}
