using Autofac;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp.Autofac
{
    public static class BuilderExtensions
    {
        /// <summary>
        /// Registers scoped flow builder with <see cref="FlowBuilder{TContext}"/> and scoped context.
        /// </summary>
        /// <typeparam name="TContext">Context to use for middleware flow.</typeparam>
        /// <param name="builder">Dependency injection builder.</param>
        /// <returns><see cref="AutofacFlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> RegisterFlowBuilder<TContext>(this ContainerBuilder builder)
        {
            return builder.RegisterFlowBuilder<TContext, FlowBuilder<TContext>>();
        }

        /// <summary>
        /// Registers scoped flow builder and scoped context.
        /// </summary>
        /// <typeparam name="TContext">Context type to use for middleware flow.</typeparam>
        /// <typeparam name="TFlowBuilder">Flow builder type to use for middleware flow.</typeparam>
        /// <param name="builder">Dependency injection builder.</param>
        /// <returns><see cref="AutofacFlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> RegisterFlowBuilder<TContext, TFlowBuilder>(this ContainerBuilder builder)
            where TFlowBuilder : IFlowBuilder<TContext>
        {
            builder.RegisterType<TFlowBuilder>().As<IFlowBuilder<TContext>>();
            builder.RegisterType<TContext>();
            return new AutofacFlowDependencyBuilder<TContext>(builder);
        }

        /// <summary>
        /// Registers scoped flow builder and scoped context.
        /// </summary>
        /// <typeparam name="TContext">Context type to use for middleware flow.</typeparam>
        /// <typeparam name="TFlowBuilder">Flow builder type to use for middleware flow.</typeparam>
        /// <param name="builder">Dependency injection builder.</param>
        /// <returns><see cref="AutofacFlowDependencyBuilder{TContext}"/> for further configuration.</returns>
        public static IFlowDependencyBuilder<TContext> RegisterFlowBuilder<TContext, TFlowBuilder, TFlow>(this ContainerBuilder builder)
            where TFlowBuilder : IFlowBuilder<TContext>
            where TFlow : IFlow<TContext>
        {
            builder.RegisterType<TFlowBuilder>().As<IFlowBuilder<TFlow, TContext>>();
            builder.RegisterType<TContext>();
            return new AutofacFlowDependencyBuilder<TContext>(builder);
        }
    }
}
