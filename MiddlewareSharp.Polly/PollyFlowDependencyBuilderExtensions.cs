﻿using Microsoft.Extensions.DependencyInjection;
using MiddlewareSharp.Interfaces;
using Polly;

namespace MiddlewareSharp.Polly
{
	public static class PollyFlowDependencyBuilderExtensions
	{
		public static IServiceCollection WithMiddleware<TContext, TMiddleware>(this IServiceCollection collection, IAsyncPolicy asyncPolicy)
			where TMiddleware : IMiddleware<TContext>
		{
			collection.AddScoped(p => new PollyMiddleware<TContext, TMiddleware>(p.GetService<TMiddleware>(), asyncPolicy));
			return collection;
		}
	}
}
