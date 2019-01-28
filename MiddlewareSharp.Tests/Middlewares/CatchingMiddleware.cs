using MiddlewareSharp.Interfaces;
using System;
using System.Threading.Tasks;

namespace MiddlewareSharp.Tests.Middlewares
{
	public class CatchingMiddleware : ICatchMiddleware<TestContext>, ITestMiddleware
	{
		public bool IsCreated { get; set; }
		public bool IsReleased { get; set; }

		public Task InvokeAsync(TestContext context, MiddlewareException<TestContext> exception, RequestDelegate<TestContext> next)
		{
			context.CatchN += context.N;
			context.CatchedException = exception;
			return next(context);
		}
	}
}
