using Autofac;
using MiddlewareSharp.Interfaces;
using Polly;

namespace MiddlewareSharp.Polly.Autofac
{
	public static class AutofacFlowDependencyBuilderExtensions
	{
		public static ContainerBuilder RegisterMiddleware<TContext, TMiddleware>(this ContainerBuilder builder, IAsyncPolicy asyncPolicy)
			where TMiddleware : class, IMiddleware<TContext>
		{
			builder.RegisterType<TMiddleware>();
			builder.Register(p => new PollyMiddleware<TContext, TMiddleware>(p.Resolve<TMiddleware>(), asyncPolicy));
			return builder;
		}
	}
}
