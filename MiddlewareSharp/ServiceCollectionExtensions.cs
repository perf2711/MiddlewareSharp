using Microsoft.Extensions.DependencyInjection;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds scoped flow builder with <see cref="FlowBuilder{TContext}"/>.
        /// </summary>
        /// <typeparam name="TContext">Context to use for middleware flow.</typeparam>
        /// <param name="collection">Dependency injection service collection.</param>
        /// <returns><see cref="FlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> AddFlowBuilder<TContext>(this IServiceCollection collection)
        {
            return collection.AddFlowBuilder<TContext, FlowBuilder<TContext>>();
        }

        /// <summary>
        /// Adds scoped flow builder.
        /// </summary>
        /// <typeparam name="TContext">Context type to use for middleware flow.</typeparam>
        /// <typeparam name="TFlowBuilder">Flow type builder to use for middleware flow.</typeparam>
        /// <param name="collection">Dependency injection service collection.</param>
        /// <returns><see cref="FlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> AddFlowBuilder<TContext, TFlowBuilder>(this IServiceCollection collection)
            where TFlowBuilder : class, IFlowBuilder<TContext>
        {
            collection.AddScoped<IFlowBuilder<TContext>, TFlowBuilder>();
            return new FlowDependencyBuilder<TContext>(collection);
        }

        /// <summary>
        /// Adds scoped flow builder.
        /// </summary>
        /// <typeparam name="TFlow">Middleware flow.</typeparam>
        /// <typeparam name="TContext">Context type to use for middleware flow.</typeparam>
        /// <typeparam name="TFlowBuilder">Flow type builder to use for middleware flow.</typeparam>
        /// <param name="collection">Dependency injection service collection.</param>
        /// <returns><see cref="FlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> AddFlowBuilder<TContext, TFlowBuilder, TFlow>(this IServiceCollection collection)
            where TFlow : IFlow<TContext>
            where TFlowBuilder : class, IFlowBuilder<TFlow, TContext>
        {
            collection.AddScoped<IFlowBuilder<TFlow, TContext>, TFlowBuilder>();
            return new FlowDependencyBuilder<TContext>(collection);
        }

        /// <summary>
        /// Adds scoped context.
        /// </summary>
        /// <typeparam name="TContext">Context type to use for middleware flow.</typeparam>
        /// <param name="collection">Dependency injection service collection.</param>
        /// <returns><see cref="FlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> AddContext<TContext>(this IServiceCollection collection)
            where TContext : class, new()
        {
            collection.AddScoped<TContext>();
            return new FlowDependencyBuilder<TContext>(collection);
        }
    }
}
