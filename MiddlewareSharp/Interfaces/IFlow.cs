using System;
using System.Threading.Tasks;

namespace MiddlewareSharp.Interfaces
{
    public interface IFlow<TContext>
        where TContext : new()
    {
        Task<TContext> InvokeAsync(IServiceProvider serviceProvider);
        Task<TContext> InvokeAsync(TContext context, IServiceProvider serviceProvider);
    }
}