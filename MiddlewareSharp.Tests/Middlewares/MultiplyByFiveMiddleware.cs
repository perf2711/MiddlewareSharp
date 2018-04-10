using System.Threading.Tasks;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp.Tests.Middlewares
{
    public class MultiplyByFiveMiddleware : IMiddleware<TestContext>, ITestMiddleware
    {
        public bool IsReleased { get; set; }
        public bool IsCreated { get; set; }

        public Task InvokeAsync(TestContext context, RequestDelegate<TestContext> next)
        {
            context.N *= 5;
            return next?.Invoke(context);
        }
    }
}
