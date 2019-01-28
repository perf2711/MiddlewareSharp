using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MiddlewareSharp.Extensions;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
	/// <summary>
	/// Class used to build middleware flow.
	/// </summary>
	/// <typeparam name="TContext">Context used in middlewares as they are invoked.</typeparam>
	public class FlowBuilder<TContext> : FlowBuilder<Flow<TContext>, TContext>, IFlowBuilder<TContext>
	{
		public FlowBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}
	}

	/// <summary>
	/// Class used to build middleware flow.
	/// </summary>
	/// <typeparam name="TFlow">Returned flow.</typeparam>
	/// <typeparam name="TContext">Context used in middlewares as they are invoked.</typeparam>
	public class FlowBuilder<TFlow, TContext> : IFlowBuilder<TFlow, TContext>
		where TFlow : IFlow<TContext>
	{
		/// <summary>
		/// Gets or sets the <see cref="IServiceProvider"/> for dependency injection.
		/// </summary>
		public IServiceProvider ServiceProvider { get; set; }

		private readonly ICollection<Type> _middlewares;

		/// <summary>
		/// Creates an instance of flow builder with provided <see cref="IServiceProvider"/> for dependency injection.
		/// </summary>
		/// <param name="serviceProvider"><see cref="IServiceProvider"/> for dependency injection.</param>
		public FlowBuilder(IServiceProvider serviceProvider)
		{
			_middlewares = new List<Type>();
			ServiceProvider = serviceProvider;
		}

		/// <summary>
		/// Adds specified type to the middleware queue.
		/// </summary>
		/// <param name="middlewareType">Type of middleware to add. Must be assignable to <see cref="IMiddleware{TContext}"/></param>
		/// <returns><see cref="IFlowBuilder{TFlow, TContext}"/> instance for fluent api.</returns>
		public IFlowBuilder<TFlow, TContext> Use(Type middlewareType)
		{
			if (!typeof(IMiddleware<TContext>).GetTypeInfo().IsAssignableFrom(middlewareType))
			{
				throw new ArgumentException($"Type of the middleware must be assignable from {nameof(IMiddleware<TContext>)}.", nameof(middlewareType));
			}
			_middlewares.Add(middlewareType);
			return this;
		}

		/// <summary>
		/// Adds specified type to the middleware queue.
		/// </summary>
		/// <typeparam name="TMiddleware">Type of middleware to add. Must be assignable to <see cref="IMiddleware{TContext}"/></typeparam>
		/// <returns><see cref="IFlowBuilder{TFlow, TContext}"/> instance for fluent api.</returns>
		public IFlowBuilder<TFlow, TContext> Use<TMiddleware>() where TMiddleware : IMiddleware<TContext>
		{
			return Use(typeof(TMiddleware));
		}

		public IFlowBuilder<TFlow, TContext> UseCatch(Type middlewareType)
		{
			if (!typeof(ICatchMiddleware<TContext>).GetTypeInfo().IsAssignableFrom(middlewareType))
			{
				throw new ArgumentException($"Type of the middleware must be assignable from {nameof(ICatchMiddleware<TContext>)}.", nameof(middlewareType));
			}
			_middlewares.Add(middlewareType);
			return this;
		}

		public IFlowBuilder<TFlow, TContext> UseCatch<TMiddleware>()
			where TMiddleware : ICatchMiddleware<TContext>
		{
			return UseCatch(typeof(TMiddleware));
		}

		/// <summary>
		/// Builds the middleware execution tree and creates an instance of <see cref="Flow{TContext}"/> for final usage.
		/// </summary>
		/// <returns><see cref="Flow{TContext}"/> instance.</returns>
		public TFlow Build()
		{
			var expression = GetMiddlewareTree(_middlewares);
			return (TFlow)Activator.CreateInstance(typeof(TFlow), expression.Compile());
		}

		private static Expression<MiddlewareDelegate<TContext>> GetMiddlewareTree(IEnumerable<Type> middlewares)
		{
			var contextParam = Expression.Parameter(typeof(TContext), "initialContext");
			var factoryParam = Expression.Parameter(typeof(IMiddlewareFactory<TContext>), "factory");
			var faultedParam = Expression.Parameter(typeof(bool), "isFlowFaulted");

			var invokeTree = GetInvokeTree(middlewares, factoryParam, faultedParam);
			var invoke = Expression.Invoke(invokeTree, contextParam);

			var block = Expression.Block(new[] {faultedParam},
				invoke);

			return Expression.Lambda<MiddlewareDelegate<TContext>>(block, contextParam, factoryParam);
		}

		private static Expression<RequestDelegate<TContext>> GetInvokeTree(IEnumerable<Type> middlewares, Expression factoryParameterExpression, Expression flowFaultedExpression)
		{
			var type = middlewares.FirstOrDefault();
			if (typeof(ICatchMiddleware<TContext>).GetTypeInfo().IsAssignableFrom(type))
			{
				return GetInvokeTree(middlewares.Skip(1), factoryParameterExpression, flowFaultedExpression);
			}

			var contextParam = Expression.Parameter(typeof(TContext), (type?.Name ?? "unknown") + "_context");
			if (type == null)
			{
				return Expression.Lambda<RequestDelegate<TContext>>(Expression.Constant(Task.CompletedTask), "unknown_scope", new [] {contextParam});
			}

			var middlewareParameter = Expression.Parameter(typeof(IMiddleware<TContext>), type.Name);
			var block = Expression.Block(new[] {middlewareParameter},
				// $middlewareParameter = ...
				AssignMiddleware(type, middlewareParameter, factoryParameterExpression),
				// $ <Task>.ContinueWith(<Release lambda>, <state>)
				ContinueWith(middlewares, middlewareParameter, contextParam, factoryParameterExpression, flowFaultedExpression)
			);

			return Expression.Lambda<RequestDelegate<TContext>>(block, type.Name + "_scope", new [] {contextParam});
		}

		private static Expression AssignMiddleware(Type middlewareType, Expression middlewareExpression, Expression factoryParameterExpression)
		{
			return Expression.Assign(
				middlewareExpression,

				// ... $factory.Create(middlewareExpression(context))
				Expression.Call(factoryParameterExpression,
					typeof(IMiddlewareFactory<TContext>).GetMethod(nameof(IMiddlewareFactory<TContext>.Create)),
					Expression.Constant(middlewareType, typeof(Type))));
		}

		private static Expression ContinueWith(IEnumerable<Type> middlewares, Expression middlewareExpression, Expression contextExpression, Expression factoryParameterExpression, Expression flowFaultedExpression)
		{
			var call = Expression.Call(
				// $middlewareExpression.InvokeAsync() (returns Task for ContinueWith)
				InvokeAsync(middlewares, middlewareExpression, factoryParameterExpression, flowFaultedExpression, contextExpression),

				// ContinueWith(Action<Task, object>, object) method
				typeof(Task).GetMethod(nameof(Task.ContinueWith),
					new[] { typeof(Action<Task, object>), typeof(object) }),

				// Pass to ContinueWith - <Release lambda>
				ContinueWithAction(middlewares, contextExpression, factoryParameterExpression, flowFaultedExpression),

				// <state>
				middlewareExpression
			);

			var catchMiddlewareType = middlewares.FirstOrDefault(t => typeof(ICatchMiddleware<TContext>).GetTypeInfo().IsAssignableFrom(t));
			if (catchMiddlewareType != null)
			{
				var remainingMiddlewares = middlewares.SkipWhile(t => t != catchMiddlewareType);
				var catchMiddlewareParameter = Expression.Parameter(typeof(ICatchMiddleware<TContext>), catchMiddlewareType.Name);
				var stateParam = Expression.Parameter(typeof(object), "state");
				var continueTaskParam = Expression.Parameter(typeof(Task), "task");

				return Expression.Call(
					call,
					typeof(Task).GetMethod(nameof(Task.ContinueWith),
						new[] { typeof(Action<Task, object>), typeof(object) }),
					Expression.Lambda<Action<Task, object>>(
						Expression.IfThen(
							Expression.Equal(
								Expression.Property(continueTaskParam, nameof(Task.IsFaulted)),
								Expression.Constant(true)),
							Expression.Block(new[] { catchMiddlewareParameter },
								Expression.Assign(flowFaultedExpression, Expression.Constant(false)),
								AssignCatchMiddleware(catchMiddlewareType, catchMiddlewareParameter, factoryParameterExpression),
								ContinueWithCatch(remainingMiddlewares, catchMiddlewareParameter, contextExpression, factoryParameterExpression, flowFaultedExpression, Expression.Convert(Expression.Property(Expression.Property(continueTaskParam, nameof(Task.Exception)), nameof(AggregateException.InnerException)), typeof(MiddlewareException<TContext>)))
							)),
						"catchLambda",
						new[] { continueTaskParam, stateParam }),
					middlewareExpression);
			}

			return call;
		}

		private static Expression InvokeAsync(IEnumerable<Type> middlewares, Expression middlewareExpression, Expression factoryParameterExpression, Expression flowFaultedExpression, Expression contextExpression)
		{
			return Expression.Call(
				middlewareExpression,
				typeof(IMiddleware<TContext>).GetMethod(nameof(IMiddleware<TContext>.InvokeAsync)),
				contextExpression,
				GetInvokeTree(middlewares.Skip(1), factoryParameterExpression, flowFaultedExpression));
		}

		private static Expression AssignCatchMiddleware(Type middlewareType, Expression middlewareExpression, Expression factoryParameterExpression)
		{
			return Expression.Assign(
				middlewareExpression,
				Expression.Call(factoryParameterExpression,
					typeof(IMiddlewareFactory<TContext>).GetMethod(nameof(IMiddlewareFactory<TContext>.CreateCatch)),
					Expression.Constant(middlewareType, typeof(Type))));
		}

		private static Expression ContinueWithCatch(IEnumerable<Type> middlewares, Expression middlewareExpression, Expression contextExpression, Expression factoryParameterExpression, Expression flowFaultedExpression, Expression exceptionExpression)
		{
			return Expression.Call(
				// $middlewareExpression.InvokeAsync() (returns Task for ContinueWith)
				InvokeCatchAsync(middlewares, middlewareExpression, factoryParameterExpression, flowFaultedExpression, contextExpression, exceptionExpression),

				// ContinueWith(Action<Task, object>, object) method
				typeof(Task).GetMethod(nameof(Task.ContinueWith),
					new[] { typeof(Action<Task, object>), typeof(object) }),

				// Pass to ContinueWith - <Release lambda>
				ContinueWithAction(middlewares, contextExpression, factoryParameterExpression, flowFaultedExpression),

				// <state>
				middlewareExpression
			);
		}

		private static Expression InvokeCatchAsync(IEnumerable<Type> middlewares, Expression middlewareExpression, Expression factoryParameterExpression, Expression flowFaultedExpression, Expression contextExpression, Expression exceptionExpression)
		{
			return Expression.Call(
				middlewareExpression,
				typeof(ICatchMiddleware<TContext>).GetMethod(nameof(ICatchMiddleware<TContext>.InvokeAsync)),
				contextExpression,
				exceptionExpression,
				GetInvokeTree(middlewares.Skip(1), factoryParameterExpression, flowFaultedExpression));
		}

		private static Expression ContinueWithAction(IEnumerable<Type> middlewares, Expression contextExpression, Expression factoryParameterExpression, Expression flowFaultedExpression)
		{
			var stateParam = Expression.Parameter(typeof(object));
			var continueTaskParam = Expression.Parameter(typeof(Task));

			return Expression.Lambda<Action<Task, object>>(
				// Call $factory.Release($middlewareParameter) after task completion
				Expression.Block(
					Expression.Call(factoryParameterExpression,
						// Release(IMiddleware<TContext>) method
						typeof(IMiddlewareFactory<TContext>).GetMethod(nameof(IMiddlewareFactory<TContext>.Release)),

						// Pass (IMiddleware<TContext) state cast as parameter
						Expression.Convert(stateParam, typeof(IMiddleware<TContext>))
					),
					// If $task.IsFaulted == true
					Expression.IfThen(
						Expression.Equal(
							Expression.Property(continueTaskParam, nameof(Task.IsFaulted)),
							Expression.Constant(true)
						),
						// If true => If $flowFaultedExpression == true
						Expression.IfThenElse(
							Expression.Equal(
								flowFaultedExpression,
								Expression.Constant(true)
							),
							// If true => Rethrow original exception
							Expression.Throw(Expression.Property(Expression.Property(continueTaskParam, nameof(Task.Exception)), nameof(AggregateException.InnerException))),
							// If false => Create middleware exception and set $flowFaultedExpression to true
							Expression.Block(
								Expression.Assign(flowFaultedExpression, Expression.Constant(true)),
								Expression.Throw(CreateMiddlewareException(continueTaskParam, Expression.Convert(stateParam, typeof(IMiddleware<TContext>)), contextExpression))
							)
						)
					)
				),
				continueTaskParam,
				stateParam
			);
		}

		private static Expression CreateMiddlewareException(Expression taskExpression, Expression middlewareExpression, Expression contextExpression)
		{
			var constructorInfo = typeof(MiddlewareException<TContext>).GetConstructor(new[]
				{typeof(IMiddleware<TContext>), typeof(TContext), typeof(IEnumerable<Exception>)});

			return Expression.New(
				constructorInfo,
				middlewareExpression,
				contextExpression,
				Expression.Property(
					Expression.Property(
						taskExpression,
						nameof(Task.Exception)
					),
					nameof(AggregateException.InnerExceptions)
				)
			);
		}
	}
}
