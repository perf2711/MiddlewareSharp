using Microsoft.Extensions.DependencyInjection;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds scoped flow builder with <see cref="FlowBuilder{TContext}"/> and scoped context.
        /// </summary>
        /// <typeparam name="TContext">Context to use for middleware flow.</typeparam>
        /// <param name="collection">Dependency injection service collection.</param>
        /// <returns><see cref="FlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> AddFlowBuilder<TContext>(this IServiceCollection collection)
            where TContext : class, new ()
        {
            return collection.AddFlowBuilder<TContext, FlowBuilder<TContext>>();
        }

        /// <summary>
        /// Adds scoped flow builder and scoped context.
        /// </summary>
        /// <typeparam name="TContext">Context type to use for middleware flow.</typeparam>
        /// <typeparam name="TFlowBuilder">Flow type builder to use for middleware flow.</typeparam>
        /// <param name="collection">Dependency injection service collection.</param>
        /// <returns><see cref="FlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> AddFlowBuilder<TContext, TFlowBuilder>(this IServiceCollection collection)
            where TContext : class, new ()
            where TFlowBuilder : class, IFlowBuilder<TContext>
        {
            collection.AddScoped<IFlowBuilder<TContext>, TFlowBuilder>();
            collection.AddScoped<TContext>();
            return new FlowDependencyBuilder<TContext>(collection);
        }
    }
}
