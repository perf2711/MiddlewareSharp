namespace MiddlewareSharp.Tests.Middlewares
{
    public interface ITestMiddleware
    {
        bool IsCreated { get; set; }
        bool IsReleased { get; set; }
    }
}
