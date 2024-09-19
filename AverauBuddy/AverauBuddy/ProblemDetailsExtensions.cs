using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AverauBuddy
{
    public static class ProblemDetailsExtensions
    {
        public static IServiceCollection AddProblemDetails(this IServiceCollection services, Action<IExceptionToProblemDetailsMapper> configureMapper = null)
        {
            var mapper = new ExceptionToProblemDetailsMapper();
            configureMapper?.Invoke(mapper);
            services.AddSingleton<IExceptionToProblemDetailsMapper>(mapper);
            return services;
        }

        public static IApplicationBuilder UseProblemDetails(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ProblemDetailsMiddleware>();
        }
    }
}