using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AverauBuddy
{
    public interface IExceptionToProblemDetailsMapper
    {
        void RegisterMapping<TException>(Func<TException, HttpContext, ProblemDetails> factory) where TException : Exception;
        ProblemDetails Map(Exception exception, HttpContext context);
    }
    
    public class ExceptionToProblemDetailsMapper : IExceptionToProblemDetailsMapper
    {
        private readonly List<ExceptionMapping> _mappings = new List<ExceptionMapping>();

        public void RegisterMapping<TException>(Func<TException, HttpContext, ProblemDetails> factory) where TException : Exception
        {
            _mappings.Add(new ExceptionMapping
            {
                ExceptionType = typeof(TException),
                ProblemDetailsFactory = (ex, ctx) => factory((TException)ex, ctx)
            });
        }

        public ProblemDetails Map(Exception exception, HttpContext context)
        {
            foreach (var mapping in _mappings)
            {
                if (mapping.ExceptionType.IsAssignableFrom(exception.GetType()))
                {
                    return mapping.ProblemDetailsFactory(exception, context);
                }
            }

            // Fallback
            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.Message,
                Instance = context.TraceIdentifier
            };
        }
    }
}