using System.Threading.Tasks;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp.Tests.Middlewares
{
    public class ThrowingMiddleware : IMiddleware<TestContext>, ITestMiddleware
    {
        public async Task InvokeAsync(TestContext context, RequestDelegate<TestContext> next)
        {
            ThrowingMethod();
        }

        public void ThrowingMethod()
        {
            throw new TestException();
        }

        public bool IsCreated { get; set; }
        public bool IsReleased { get; set; }
    }
}
