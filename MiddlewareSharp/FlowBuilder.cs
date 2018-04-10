using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    /// <summary>
    /// Class used to build middleware flow.
    /// </summary>
    /// <typeparam name="TContext">Context used in middlewares as they are invoked.</typeparam>
	public class FlowBuilder<TContext> : IFlowBuilder<TContext>
		where TContext : new()
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
        /// <returns><see cref="IFlowBuilder{TContext}"/> instance for fluent api.</returns>
		public IFlowBuilder<TContext> Use(Type middlewareType)
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
	    /// <returns><see cref="IFlowBuilder{TContext}"/> instance for fluent api.</returns>
	    public IFlowBuilder<TContext> Use<TMiddleware>() where TMiddleware : IMiddleware<TContext>
		{
			return Use(typeof(TMiddleware));
		}

        /// <summary>
        /// Builds the middleware execution tree and creates an instance of <see cref="Flow{TContext}"/> for final usage.
        /// </summary>
        /// <returns><see cref="Flow{TContext}"/> instance.</returns>
		public Flow<TContext> Build()
		{
			var expression = GetMiddlewareTree(_middlewares.GetEnumerator());
			return new Flow<TContext>(expression.Compile());
		}

		private static Expression<MiddlewareDelegate<TContext>> GetMiddlewareTree(IEnumerator<Type> middlewareEnumerator)
		{
		    var contextParam = Expression.Parameter(typeof(TContext), "initialContext");
		    var factoryParam = Expression.Parameter(typeof(IMiddlewareFactory<TContext>), "factory");
		    var faultedParam = Expression.Parameter(typeof(bool), "isFlowFaulted");

		    var invokeTree = GetInvokeTree(middlewareEnumerator, factoryParam, faultedParam);
		    var invoke = Expression.Invoke(invokeTree, contextParam);

		    var block = Expression.Block(new[] {faultedParam},
		        invoke);

		    return Expression.Lambda<MiddlewareDelegate<TContext>>(block, contextParam, factoryParam);
		}

	    private static Expression<RequestDelegate<TContext>> GetInvokeTree(IEnumerator<Type> middlewareEnumerator, Expression factoryParameterExpression, Expression flowFaultedExpression)
	    {
	        var type = middlewareEnumerator.MoveNext()
	            ? middlewareEnumerator.Current
	            : null;

	        var contextParam = Expression.Parameter(typeof(TContext), (type?.Name ?? "unknown") + "_context");
	        if (type == null)
	        {
	            return Expression.Lambda<RequestDelegate<TContext>>(Expression.Constant(Task.CompletedTask), "unkown_scope", new [] {contextParam});
	        }

	        var middlewareParameter = Expression.Parameter(typeof(IMiddleware<TContext>), type.Name);
	        var block = Expression.Block(new[] {middlewareParameter},
	            // $middlewareParameter = ...
	            AssignMiddleware(type, middlewareParameter, factoryParameterExpression),
	            // $ <Task>.ContinueWith(<Release lambda>, <state>)
	            ContinueWith(middlewareEnumerator, middlewareParameter, contextParam, factoryParameterExpression, flowFaultedExpression)
	        );

            return Expression.Lambda<RequestDelegate<TContext>>(block, type.Name + "_scope", new [] {contextParam});
	    }

	    private static Expression AssignMiddleware(Type middlewareType, Expression middlewareExpression, Expression factoryParameterExpression)
	    {
	        var typeConstant = Expression.Constant(middlewareType, typeof(Type));

	        return Expression.Assign(
	            middlewareExpression,

	            // ... $factory.Create($typeConstant)
	            Expression.Call(factoryParameterExpression,
	                typeof(IMiddlewareFactory<TContext>).GetMethod(nameof(IMiddlewareFactory<TContext>.Create)),
	                typeConstant));
	    }

	    private static Expression ContinueWith(IEnumerator<Type> middlewareEnumerator, Expression middlewareExpression, Expression contextExpression, Expression factoryParameterExpression, Expression flowFaultedExpression)
	    {
	        return Expression.Call(
                // $middlewareExpression.InvokeAsync() (returns Task for ContinueWith)
	            InvokeAsync(middlewareEnumerator, middlewareExpression, contextExpression, factoryParameterExpression, flowFaultedExpression),

	            // ContinueWith(Action<Task, object>, object) method
	            typeof(Task).GetMethod(nameof(Task.ContinueWith),
	                new[] {typeof(Action<Task, object>), typeof(object)}),

	            // Pass to ContinueWith - <Release lambda>
	            ContinueWithAction(contextExpression, factoryParameterExpression, flowFaultedExpression),

	            // <state>
	            middlewareExpression
	        );
	    }

	    private static Expression InvokeAsync(IEnumerator<Type> middlewareEnumerator, Expression middlewareExpression, Expression contextExpression, Expression factoryParameterExpression, Expression flowFaultedExpression)
	    {
	        return Expression.Call(middlewareExpression,
	            typeof(IMiddleware<TContext>).GetMethod(nameof(IMiddleware<TContext>.InvokeAsync)), contextExpression,
	            GetInvokeTree(middlewareEnumerator, factoryParameterExpression, flowFaultedExpression));
	    }

	    private static Expression ContinueWithAction(Expression contextExpression, Expression factoryParameterExpression, Expression flowFaultedExpression)
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
                                ThrowMiddlewareException(continueTaskParam, Expression.Convert(stateParam, typeof(IMiddleware<TContext>)), contextExpression)
                            )
                        )
	                )
	            ),
	            continueTaskParam,
	            stateParam
	        );
	    }

	    private static Expression ThrowMiddlewareException(Expression taskExpression, Expression middlewareExpression, Expression contextExpression)
	    {
	        var constructorInfo = typeof(MiddlewareException<TContext>).GetConstructor(new[]
	            {typeof(IMiddleware<TContext>), typeof(TContext), typeof(IEnumerable<Exception>)});

	        return Expression.Throw(
	            Expression.New(
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
	            )
	        );
	    }
	}
}
