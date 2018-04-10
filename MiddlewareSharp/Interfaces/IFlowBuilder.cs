using System;

namespace MiddlewareSharp.Interfaces
{
    public interface IFlowBuilder<TContext>
        where TContext : new ()
    {
        IServiceProvider ServiceProvider { get; set; }

        IFlowBuilder<TContext> Use(Type middlewareType);
        IFlowBuilder<TContext> Use<TMiddleware>() where TMiddleware : IMiddleware<TContext>;
        Flow<TContext> Build();
    }
}
