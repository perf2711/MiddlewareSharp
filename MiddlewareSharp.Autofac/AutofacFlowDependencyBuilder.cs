using Autofac;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp.Autofac
{
    public class AutofacFlowDependencyBuilder<TContext> : IFlowDependencyBuilder<TContext>
    {
        public ContainerBuilder Builder { get; }

        internal AutofacFlowDependencyBuilder(ContainerBuilder builder)
        {
            Builder = builder;
        }

        public IFlowDependencyBuilder<TContext> WithDefaultMiddlewareFactory()
        {
            Builder.RegisterType<DefaultMiddlewareFactory<TContext>>().As<IMiddlewareFactory<TContext>>().InstancePerLifetimeScope();
            return this;
        }

        public IFlowDependencyBuilder<TContext> WithMiddlewareFactory<TFactory>() where TFactory : class, IMiddlewareFactory<TContext>
        {
            Builder.RegisterType<TFactory>().As<IMiddlewareFactory<TContext>>().InstancePerLifetimeScope();
            return this;
        }

        public IFlowDependencyBuilder<TContext> WithMiddleware<TMiddleware>() where TMiddleware : class, IMiddleware<TContext>
        {
            Builder.RegisterType<TMiddleware>().InstancePerLifetimeScope();
            return this;
        }

		public IFlowDependencyBuilder<TContext> WithCatchMiddleware<TMiddleware>() where TMiddleware : class, ICatchMiddleware<TContext>
		{
			Builder.RegisterType<TMiddleware>().InstancePerLifetimeScope();
			return this;
		}
	}
}
