using System.Threading.Tasks;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp.Tests.Middlewares
{
    public class IncrementByOneMiddleware : IMiddleware<TestContext>, ITestMiddleware
    {
        public bool IsCreated { get; set; }
        public bool IsReleased { get; set; }

        public Task InvokeAsync(TestContext context, RequestDelegate<TestContext> next)
        {
            context.N++;
            return next?.Invoke(context);
        }
    }
}
