using System;
using MiddlewareSharp.Interfaces;
using MiddlewareSharp.Tests.Middlewares;

namespace MiddlewareSharp.Tests
{
    public class TestMiddlewareFactory : DefaultMiddlewareFactory<TestContext>
    {
        public TestMiddlewareFactory(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IMiddleware<TestContext> Create(Type middlewareType)
        {
            var middleware = base.Create(middlewareType);
            if (middleware is ITestMiddleware test)
            {
                test.IsCreated = true;
            }
            return middleware;
        }

		public override ICatchMiddleware<TestContext> CreateCatch(Type middlewareType)
		{
			var middleware = base.CreateCatch(middlewareType);
			if (middleware is ITestMiddleware test)
			{
				test.IsCreated = true;
			}
			return middleware;
		}

		public override void Release(IMiddleware<TestContext> middleware)
        {
            if (middleware is ITestMiddleware test)
            {
                test.IsReleased = true;
            }
            base.Release(middleware);
        }
    }
}
