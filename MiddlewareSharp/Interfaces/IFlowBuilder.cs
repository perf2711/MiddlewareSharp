using System;
using System.Linq.Expressions;

namespace MiddlewareSharp.Interfaces
{
    public interface IFlowBuilder<TContext> : IFlowBuilder<Flow<TContext>, TContext>
    {

    }

    public interface IFlowBuilder<TFlow, TContext>
        where TFlow : IFlow<TContext>
    {
        IFlowBuilder<TFlow, TContext> Use(Type middlewareType);
        IFlowBuilder<TFlow, TContext> Use<TMiddleware>() where TMiddleware : IMiddleware<TContext>;
		IFlowBuilder<TFlow, TContext> UseCatch(Type middlewareType);
		IFlowBuilder<TFlow, TContext> UseCatch<TMiddleware>() where TMiddleware : ICatchMiddleware<TContext>;
		TFlow Build();
    }
}
