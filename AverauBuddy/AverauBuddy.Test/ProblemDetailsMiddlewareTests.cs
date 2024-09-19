using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;

namespace AverauBuddy.Test;

class CannotParsePageException : Exception
{
    public CannotParsePageException(string message) : base(message)
    {
    }
}

public class ProblemDetailsMiddlewareTests
{
    private readonly IExceptionToProblemDetailsMapper _mapper;
    private readonly RequestDelegate _next;

    public ProblemDetailsMiddlewareTests()
    {
        _mapper = Substitute.For<IExceptionToProblemDetailsMapper>();
        _next = Substitute.For<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextMiddleware()
    {
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var middleware = new ProblemDetailsMiddleware(_next, _mapper);

        await middleware.InvokeAsync(context);

        await _next.Received(1).Invoke(context);
        _mapper.DidNotReceiveWithAnyArgs().Map(default, default);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_HandlesException()
    {
        var exception = new InvalidOperationException("Test exception");
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message,
            Instance = context.TraceIdentifier
        };

        _mapper.Map(exception, context).Returns(problemDetails);
        _next.Invoke(context).Returns<Task>(x => throw exception);

        var middleware = new ProblemDetailsMiddleware(_next, _mapper);

        await middleware.InvokeAsync(context);

        _mapper.Received(1).Map(exception, context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var responseText = await reader.ReadToEndAsync();

        var expectedJson = JsonConvert.SerializeObject(problemDetails, new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        });

        responseText.Should().Be(expectedJson);
    }

    [Fact]
    public async Task InvokeAsync_CustomException_UsesCustomMapping()
    {
        var exception = new CannotParsePageException("Cannot parse page with ID 999.");
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var problemDetails = new ProblemDetails
        {
            Type = "https://yourdomain.com/probs/cannot-parse-page",
            Title = "Cannot Parse Page",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = exception.Message,
            Instance = context.TraceIdentifier
        };

        _mapper.Map(exception, context).Returns(problemDetails);
        _next.Invoke(context).Returns<Task>(x => throw exception);

        var middleware = new ProblemDetailsMiddleware(_next, _mapper);

        await middleware.InvokeAsync(context);

        _mapper.Received(1).Map(exception, context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        context.Response.ContentType.Should().Be("application/problem+json");

        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var responseText = await reader.ReadToEndAsync();

        var expectedJson = JsonConvert.SerializeObject(problemDetails, new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        });

        responseText.Should().Be(expectedJson);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionNotMapped_UsesFallbackMapping()
    {
        var exception = new NotSupportedException("Operation not supported.");
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var fallbackProblemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message,
            Instance = context.TraceIdentifier
        };

        _mapper.Map(exception, context).Returns(fallbackProblemDetails);
        _next.Invoke(context).Returns<Task>(x => throw exception);

        var middleware = new ProblemDetailsMiddleware(_next, _mapper);

        await middleware.InvokeAsync(context);

        _mapper.Received(1).Map(exception, context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var responseText = await reader.ReadToEndAsync();

        var expectedJson = JsonConvert.SerializeObject(fallbackProblemDetails, new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        });

        responseText.Should().Be(expectedJson);
    }
}