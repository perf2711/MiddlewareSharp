using Microsoft.Extensions.DependencyInjection;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    public class FlowDependencyBuilder<TContext> : IFlowDependencyBuilder<TContext>
    {
        public IServiceCollection Collection { get; }

        internal FlowDependencyBuilder(IServiceCollection collection)
        {
            Collection = collection;
        }

        public IFlowDependencyBuilder<TContext> WithDefaultMiddlewareFactory()
        {
            Collection.AddScoped<IMiddlewareFactory<TContext>, DefaultMiddlewareFactory<TContext>>();
            return this;
        }

        public IFlowDependencyBuilder<TContext> WithMiddlewareFactory<TFactory>() where TFactory : class, IMiddlewareFactory<TContext>
        {
            Collection.AddScoped<IMiddlewareFactory<TContext>, TFactory>();
            return this;
        }

        public IFlowDependencyBuilder<TContext> WithMiddleware<TMiddleware>() where TMiddleware : class, IMiddleware<TContext>
        {
            Collection.AddScoped<TMiddleware>();
            return this;
        }
    }
}
