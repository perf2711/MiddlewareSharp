using System.Threading.Tasks;

namespace MiddlewareSharp
{
    public delegate Task RequestDelegate<in TContext>(TContext context);
}
