using System.Threading.Tasks;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    internal delegate Task MiddlewareDelegate<TContext>(TContext context, IMiddlewareFactory<TContext> serviceProvider);
}
