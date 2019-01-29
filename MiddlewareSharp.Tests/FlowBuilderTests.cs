using Autofac;
using Autofac.Extensions.DependencyInjection;
using MiddlewareSharp.Autofac;
using MiddlewareSharp.Interfaces;
using MiddlewareSharp.Tests.Middlewares;
using NUnit.Framework;
using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;
using MiddlewareSharp.Polly.Autofac;
using MiddlewareSharp.Polly;

namespace MiddlewareSharp.Tests
{
	[TestFixture]
	public class FlowBuilderTests
	{
		private IServiceProvider ServiceProvider { get; set; }

		[Test]
		public async Task IncrementThenMultiplyMiddlewareTest()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use<IncrementByOneMiddleware>();
			flowBuilder.Use(typeof(MultiplyByFiveMiddleware));

			var flow = flowBuilder.Build();

			var context = await flow.InvokeAsync(ServiceProvider);
			Assert.AreEqual((0 + 1) * 5, context.N);

			VerifyCreated(true, typeof(MultiplyByFiveMiddleware), typeof(IncrementByOneMiddleware));
			VerifyReleased(true, typeof(MultiplyByFiveMiddleware), typeof(IncrementByOneMiddleware));
		}


		[Test]
		public async Task MultiplyThenIncrementMiddlewareTest()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use(typeof(MultiplyByFiveMiddleware));
			flowBuilder.Use<IncrementByOneMiddleware>();

			var flow = flowBuilder.Build();

			var context = new TestContext { N = 5 };
			await flow.InvokeAsync(context, ServiceProvider);

			Assert.AreEqual(5 * 5 + 1, context.N);

			VerifyCreated(true, typeof(MultiplyByFiveMiddleware), typeof(IncrementByOneMiddleware));
			VerifyReleased(true, typeof(MultiplyByFiveMiddleware), typeof(IncrementByOneMiddleware));
		}

		[Test]
		public async Task StoppingMiddlewareTest()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use(typeof(MultiplyByFiveMiddleware));
			flowBuilder.Use<StopMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();

			var flow = flowBuilder.Build();

			var context = new TestContext { N = 5 };
			await flow.InvokeAsync(context, ServiceProvider);

			Assert.AreEqual(5 * 5, context.N);

			VerifyCreated(true, typeof(MultiplyByFiveMiddleware), typeof(StopMiddleware));
			VerifyReleased(true, typeof(MultiplyByFiveMiddleware), typeof(StopMiddleware));

			VerifyCreated(false, typeof(IncrementByOneMiddleware));
			VerifyReleased(false, typeof(IncrementByOneMiddleware));
		}

		[Test]
		public void ThrowingMiddlewareTest()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();
			flowBuilder.Use<ThrowingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();

			var flow = flowBuilder.Build();

			var context = new TestContext { N = 5 };
			var exception = Assert.ThrowsAsync<MiddlewareException<TestContext>>(() => flow.InvokeAsync(context, ServiceProvider));

			Assert.AreEqual(ServiceProvider.GetService(typeof(ThrowingMiddleware)), exception.Middleware);
			Assert.AreSame(context, exception.Context);
			Assert.AreEqual(5 * 5, context.N);

			VerifyCreated(true, typeof(MultiplyByFiveMiddleware), typeof(ThrowingMiddleware));
			VerifyReleased(true, typeof(MultiplyByFiveMiddleware), typeof(ThrowingMiddleware));

			VerifyCreated(false, typeof(IncrementByOneMiddleware));
			VerifyReleased(false, typeof(IncrementByOneMiddleware));
		}

		[Test]
		public async Task CatchingMiddleware_WhenMiddlewareThrows_FlowDoesntThrow()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();
			flowBuilder.Use<ThrowingMiddleware>();
			flowBuilder.UseCatch<CatchingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();

			var flow = flowBuilder.Build();
			var context = new TestContext { N = 5 };
			await flow.InvokeAsync(context, ServiceProvider);
		}

		[Test]
		public async Task CatchingMiddleware_WhenMiddlewareThrows_FlowContinues()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();
			flowBuilder.Use<ThrowingMiddleware>();
			flowBuilder.UseCatch<CatchingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();

			var flow = flowBuilder.Build();
			var context = new TestContext { N = 5 };
			await flow.InvokeAsync(context, ServiceProvider);

			Assert.AreEqual(5 * 5 + 1, context.N);
		}

		[Test]
		public async Task CatchingMiddleware_WhenMiddlewareThrows_FlowSkipsToCatch()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();
			flowBuilder.Use<ThrowingMiddleware>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();
			flowBuilder.UseCatch<CatchingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();

			var flow = flowBuilder.Build();
			var context = new TestContext { N = 5 };
			await flow.InvokeAsync(context, ServiceProvider);

			Assert.AreEqual(5 * 5 + 1, context.N);
		}

		[Test]
		public async Task CatchingMiddleware_WhenMiddlewareDoesntThrow_CatchIsNotExecuted()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();
			flowBuilder.UseCatch<CatchingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();

			var flow = flowBuilder.Build();
			var context = new TestContext { N = 5 };
			await flow.InvokeAsync(context, ServiceProvider);

			Assert.IsNull(context.CatchedException);
		}

		[Test]
		public async Task CatchingMiddleware_WhenMiddlewareThrows_CatchIsExecutedOnlyOnce()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use<ThrowingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();
			flowBuilder.Use<ThrowingMiddleware>();
			flowBuilder.UseCatch<CatchingMiddleware>();

			var flow = flowBuilder.Build();
			var context = new TestContext { N = 5 };
			await flow.InvokeAsync(context, ServiceProvider);

			Assert.AreEqual(5, context.CatchN);
		}

		[Test]
		public async Task CatchingMiddleware_WhenTwoCatchesAreUsed_ClosestOneIsExecuted()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.Use<IncrementByOneMiddleware>();
			flowBuilder.Use<ThrowingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();
			flowBuilder.UseCatch<CatchingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();
			flowBuilder.Use<ThrowingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();
			flowBuilder.UseCatch<CatchingMiddleware>();
			flowBuilder.Use<IncrementByOneMiddleware>();

			var flow = flowBuilder.Build();
			var context = new TestContext { N = 5 };
			await flow.InvokeAsync(context, ServiceProvider);

			Assert.AreEqual(6 + 7, context.CatchN);
		}


		[Test]
		public void InvalidMiddlewareUseTest()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			Assert.Throws<ArgumentException>(() => flowBuilder.Use(typeof(int)));
		}

		[Test]
		public async Task Flow_WhenRetryPolicyIsProvided_ShouldSucceedAfterRetrying()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.UseWithPolicy<TestContext, SucceedAtFiveMiddleware>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();

			var flow = flowBuilder.Build();
			var context = new TestContext { N = 1 };
			await flow.InvokeAsync(context, ServiceProvider);

			Assert.AreEqual(5 * 5, context.N);
		}

		[Test]
		public void Flow_WhenRetryPolicyIsProvided_ShouldFailAfterTooManyRetries()
		{
			ServiceProvider = SetupServices();

			var flowBuilder = new FlowBuilder<TestContext>();
			flowBuilder.UseWithPolicy<TestContext, SucceedAtFiveMiddleware>();
			flowBuilder.Use<MultiplyByFiveMiddleware>();

			var flow = flowBuilder.Build();
			var context = new TestContext { N = -2 };
			Assert.ThrowsAsync<MiddlewareException<TestContext>>(() => flow.InvokeAsync(context, ServiceProvider));
		}

		private static IServiceProvider SetupServices()
		{
			var builder = new ContainerBuilder();

			builder.RegisterFlowBuilder<TestContext>()
				.WithMiddlewareFactory<TestMiddlewareFactory>()
				.WithMiddleware<IncrementByOneMiddleware>()
				.WithMiddleware<MultiplyByFiveMiddleware>()
				.WithMiddleware<StopMiddleware>()
				.WithMiddleware<ThrowingMiddleware>()
				.WithMiddleware<SucceedAtFiveMiddleware>()
				.WithCatchMiddleware<CatchingMiddleware>();

			builder.RegisterMiddleware<TestContext, SucceedAtFiveMiddleware>(Policy.Handle<TestException>().RetryAsync(5));

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
