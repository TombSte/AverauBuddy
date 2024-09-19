using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AverauBuddy
{
    public class ProblemDetailsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IExceptionToProblemDetailsMapper _mapper;
        private readonly JsonSerializerSettings _jsonSettings;

        public ProblemDetailsMiddleware(RequestDelegate next, IExceptionToProblemDetailsMapper mapper)
        {
            _next = next;
            _mapper = mapper;

            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var problemDetails = _mapper.Map(ex, context);
                context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var json = JsonConvert.SerializeObject(problemDetails, _jsonSettings);
                await context.Response.WriteAsync(json);
            }
        }
    }
}