using System.Threading.Tasks;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    public delegate Task MiddlewareDelegate<TContext>(TContext context, IMiddlewareFactory<TContext> serviceProvider);
}
