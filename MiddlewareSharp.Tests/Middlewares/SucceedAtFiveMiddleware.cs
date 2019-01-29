using MiddlewareSharp.Interfaces;
using System.Threading.Tasks;

namespace MiddlewareSharp.Tests.Middlewares
{
	public class SucceedAtFiveMiddleware : IMiddleware<TestContext>, ITestMiddleware
	{
		public bool IsCreated { get; set; }
		public bool IsReleased { get; set; }

		public Task InvokeAsync(TestContext context, RequestDelegate<TestContext> next)
		{
			context.N++;
			if (context.N != 5)
			{
				throw new TestException();
			}
			return next?.Invoke(context);
		}
	}
}
