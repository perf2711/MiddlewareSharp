using System;

namespace MiddlewareSharp.Interfaces
{
    public interface IFlowBuilder<TContext> : IFlowBuilder<Flow<TContext>, TContext>
    {

    }

    public interface IFlowBuilder<TFlow, TContext>
        where TFlow : IFlow<TContext>
    {
        IServiceProvider ServiceProvider { get; set; }

        IFlowBuilder<TFlow, TContext> Use(Type middlewareType);
        IFlowBuilder<TFlow, TContext> Use<TMiddleware>() where TMiddleware : IMiddleware<TContext>;
        TFlow Build();
    }
}
