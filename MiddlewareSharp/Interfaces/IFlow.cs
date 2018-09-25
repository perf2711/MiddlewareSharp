using System;
using System.Threading.Tasks;

namespace MiddlewareSharp.Interfaces
{
    public interface IFlow<TContext>
    {
        Task<TContext> InvokeAsync(IServiceProvider serviceProvider);
        Task<TContext> InvokeAsync(TContext context, IServiceProvider serviceProvider);
    }
}