using MiddlewareSharp.Interfaces;
using System;

namespace MiddlewareSharp.Polly
{
	public static class PollyFlowBuilderExtensions
	{
		public static IFlowBuilder<Flow<TContext>, TContext> UseWithPolicy<TContext, TMiddleware>(this FlowBuilder<TContext> flowBuilder)
			where TMiddleware : IMiddleware<TContext>
		{
			return flowBuilder.Use<PollyMiddleware<TContext, TMiddleware>>();
		}

		public static IFlowBuilder<Flow<TContext>, TContext> UseWithPolicy<TContext>(this FlowBuilder<TContext> flowBuilder, Type type)
		{
			var pollyType = typeof(PollyMiddleware<,>).MakeGenericType(typeof(TContext), type);
			return flowBuilder.Use(pollyType);
		}
	}
}
