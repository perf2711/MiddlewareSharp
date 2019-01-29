using MiddlewareSharp.Interfaces;
using Polly;
using System;
using System.Threading.Tasks;

namespace MiddlewareSharp.Polly
{
	public class PollyMiddleware<TContext, TMiddleware> : IMiddleware<TContext>
		where TMiddleware : IMiddleware<TContext>
	{
		private readonly TMiddleware _middleware;
		private readonly IAsyncPolicy _policy;

		public PollyMiddleware(TMiddleware middleware, IAsyncPolicy policy)
		{
			_middleware = middleware;
			_policy = policy;
		}

		public async Task InvokeAsync(TContext context, RequestDelegate<TContext> next)
		{
			var executeNext = false;

			var result = await _policy.ExecuteAndCaptureAsync(() => _middleware.InvokeAsync(context, (c) =>
			{
				executeNext = true;
				return Task.CompletedTask;
			}));

			if (result.FinalException != null)
			{
				throw result.FinalException;
			}

			if (executeNext)
			{
				await next(context);
			}
		}
	}
}
