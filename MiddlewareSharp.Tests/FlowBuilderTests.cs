using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MiddlewareSharp.Autofac;
using MiddlewareSharp.Interfaces;
using MiddlewareSharp.Tests.Middlewares;
using Xunit;
using Xunit.Abstractions;

namespace MiddlewareSharp.Tests
{
    public class FlowBuilderTests
    {
        private IServiceProvider ServiceProvider { get; set; }

        public FlowBuilderTests(ITestOutputHelper output)
        {
            Trace.Listeners.Add(new TestTraceListener(output));
        }

        [Fact]
        public async Task IncrementThenMultiplyMiddlewareTest()
        {
            ServiceProvider = SetupServices();

            var flowBuilder = new FlowBuilder<TestContext>(ServiceProvider);
            flowBuilder.Use<IncrementByOneMiddleware>();
            flowBuilder.Use(typeof(MultiplyByFiveMiddleware));

            var flow = flowBuilder.Build();

            var context = await flow.InvokeAsync(ServiceProvider);
            Assert.Equal((0 + 1) * 5, context.N);

            VerifyCreated(true, typeof(MultiplyByFiveMiddleware), typeof(IncrementByOneMiddleware));
            VerifyReleased(true, typeof(MultiplyByFiveMiddleware), typeof(IncrementByOneMiddleware));
        }


        [Fact]
        public async Task MultiplyThenIncrementMiddlewareTest()
        {
            ServiceProvider = SetupServices();

            var flowBuilder = new FlowBuilder<TestContext>(ServiceProvider);
            flowBuilder.Use(typeof(MultiplyByFiveMiddleware));
            flowBuilder.Use<IncrementByOneMiddleware>();

            var flow = flowBuilder.Build();

            var context = new TestContext {N = 5};
            await flow.InvokeAsync(context, ServiceProvider);

            Assert.Equal(5 * 5 + 1, context.N);

            VerifyCreated(true, typeof(MultiplyByFiveMiddleware), typeof(IncrementByOneMiddleware));
            VerifyReleased(true, typeof(MultiplyByFiveMiddleware), typeof(IncrementByOneMiddleware));
        }

        [Fact]
        public async Task StoppingMiddlewareTest()
        {
            ServiceProvider = SetupServices();

            var flowBuilder = new FlowBuilder<TestContext>(ServiceProvider);
            flowBuilder.Use(typeof(MultiplyByFiveMiddleware));
            flowBuilder.Use<StopMiddleware>();
            flowBuilder.Use<IncrementByOneMiddleware>();

            var flow = flowBuilder.Build();

            var context = new TestContext {N = 5};
            await flow.InvokeAsync(context, ServiceProvider);

            Assert.Equal(5 * 5, context.N);

            VerifyCreated(true, typeof(MultiplyByFiveMiddleware), typeof(StopMiddleware));
            VerifyReleased(true, typeof(MultiplyByFiveMiddleware), typeof(StopMiddleware));

            VerifyCreated(false, typeof(IncrementByOneMiddleware));
            VerifyReleased(false, typeof(IncrementByOneMiddleware));
        }

        [Fact]
        public async Task ThrowingMiddlewareTest()
        {
            ServiceProvider = SetupServices();

            var flowBuilder = new FlowBuilder<TestContext>(ServiceProvider);
            flowBuilder.Use(typeof(MultiplyByFiveMiddleware));
            flowBuilder.Use<ThrowingMiddleware>();
            flowBuilder.Use<IncrementByOneMiddleware>();

            var flow = flowBuilder.Build();

            var context = new TestContext {N = 5};
            var exception = await Assert.ThrowsAsync<MiddlewareException<TestContext>>(() => flow.InvokeAsync(context, ServiceProvider));

            Assert.Equal(ServiceProvider.GetService(typeof(ThrowingMiddleware)), exception.Middleware);
            Assert.Same(context, exception.Context);
            Assert.Equal(5 * 5, context.N);

            VerifyCreated(true, typeof(MultiplyByFiveMiddleware), typeof(ThrowingMiddleware));
            VerifyReleased(true, typeof(MultiplyByFiveMiddleware), typeof(ThrowingMiddleware));

            VerifyCreated(false, typeof(IncrementByOneMiddleware));
            VerifyReleased(false, typeof(IncrementByOneMiddleware));
        }

        [Fact]
        public void InvalidMiddlewareUseTest()
        {
            ServiceProvider = SetupServices();

            var flowBuilder = new FlowBuilder<TestContext>(ServiceProvider);
            Assert.Throws<ArgumentException>(() => flowBuilder.Use(typeof(int)));
        }

        private static IServiceProvider SetupServices()
        {
            var builder = new ContainerBuilder();

            builder.RegisterFlowBuilder<TestContext>()
                .WithMiddlewareFactory<TestMiddlewareFactory>()
                .WithMiddleware<IncrementByOneMiddleware>()
                .WithMiddleware<MultiplyByFiveMiddleware>()
                .WithMiddleware<StopMiddleware>()
                .WithMiddleware<ThrowingMiddleware>();

            builder.RegisterType<TestMiddlewareFactory>().As<IMiddlewareFactory<TestContext>>();
            builder.RegisterType<AutofacServiceProvider>().As<IServiceProvider>();

            return new AutofacServiceProvider(builder.Build());
        }

        private void VerifyCreated(bool created, params Type[] middlewares)
        {
            Assert.True(middlewares
                .Select(m => ServiceProvider.GetService(m) as ITestMiddleware)
                .Where(m => m != null)
                .All(m => m.IsCreated == created),
                $"One of {string.Join(", ", middlewares.Select(m => m.Name))} has {nameof(ITestMiddleware.IsCreated)} set to {!created}");
        }

        private void VerifyReleased(bool released, params Type[] middlewares)
        {
            Assert.True(middlewares
                .Select(m => ServiceProvider.GetService(m) as ITestMiddleware)
                .Where(m => m != null)
                .All(m => m.IsReleased == released),
                $"One of {string.Join(", ", middlewares.Select(m => m.Name))} has {nameof(ITestMiddleware.IsReleased)} set to {!released}");
        }
    }
}
