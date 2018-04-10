using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MiddlewareSharp.Interfaces;

namespace MiddlewareSharp
{
    public class MiddlewareException<TContext> : AggregateException
    {
        /// <summary>
        /// Middleware which threw the exception.
        /// </summary>
        public IMiddleware<TContext> Middleware { get; }

        /// <summary>
        /// Current context state.
        /// </summary>
        public TContext Context { get; }

        public MiddlewareException(IMiddleware<TContext> middleware, TContext context)
        {
            Middleware = middleware;
            Context = context;
        }

        public MiddlewareException(IMiddleware<TContext> middleware, TContext context, IEnumerable<Exception> innerExceptions) : base(innerExceptions)
        {
            Middleware = middleware;
            Context = context;
        }

        public MiddlewareException(IMiddleware<TContext> middleware, TContext context, params Exception[] innerExceptions) : base(innerExceptions)
        {
            Middleware = middleware;
            Context = context;
        }

        public MiddlewareException(IMiddleware<TContext> middleware, TContext context, string message) : base(message)
        {
            Middleware = middleware;
            Context = context;
        }

        public MiddlewareException(IMiddleware<TContext> middleware, TContext context, string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
        {
            Middleware = middleware;
            Context = context;
        }

        public MiddlewareException(IMiddleware<TContext> middleware, TContext context, string message, Exception innerException) : base(message, innerException)
        {
            Middleware = middleware;
            Context = context;
        }

        public MiddlewareException(IMiddleware<TContext> middleware, TContext context, string message, params Exception[] innerExceptions) : base(message, innerExceptions)
        {
            Middleware = middleware;
            Context = context;
        }

        protected MiddlewareException(IMiddleware<TContext> middleware, TContext context, SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext)
        {
            Middleware = middleware;
            Context = context;
        }
    }
}
