using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AverauBuddy
{
    public class ExceptionMapping
    {
        public Type ExceptionType { get; set; }
        public Func<Exception, HttpContext, ProblemDetails> ProblemDetailsFactory { get; set; }
    }
}