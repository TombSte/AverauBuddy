using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace AverauBuddy.Test;

public class ExceptionToProblemDetailsMapperTests
{
    private readonly ExceptionToProblemDetailsMapper _mapper;

    public ExceptionToProblemDetailsMapperTests()
    {
        _mapper = new ExceptionToProblemDetailsMapper();
    }

    [Fact]
    public void RegisterMapping_RegistersMappingCorrectly()
    {
        var exception = new InvalidOperationException("Test exception");
        var context = Substitute.For<HttpContext>();
        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/test",
            Title = "Test Exception",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Detailed message",
            Instance = context.TraceIdentifier
        };

        _mapper.RegisterMapping<InvalidOperationException>((ex, ctx) => problemDetails);

        var result = _mapper.Map(exception, context);

        result.Should().Be(problemDetails);
    }

    [Fact]
    public void Map_UnmappedException_ReturnsFallbackProblemDetails()
    {
        var exception = new NotSupportedException("Operation not supported");
        var context = Substitute.For<HttpContext>();

        var result = _mapper.Map(exception, context);

        result.Should().NotBeNull();
        result.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
        result.Title.Should().Be("Internal Server Error");
        result.Status.Should().Be(StatusCodes.Status500InternalServerError);
        result.Detail.Should().Be(exception.Message);
        result.Instance.Should().Be(context.TraceIdentifier);
    }

    [Fact]
    public void Map_MappedException_ReturnsMappedProblemDetails()
    {
        var exception = new ArgumentNullException("param");
        var context = Substitute.For<HttpContext>();
        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/argument-null",
            Title = "Argument Null",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Parameter cannot be null.",
            Instance = context.TraceIdentifier
        };

        _mapper.RegisterMapping<ArgumentNullException>((ex, ctx) => problemDetails);

        var result = _mapper.Map(exception, context);

        result.Should().Be(problemDetails);
    }

    [Fact]
    public void RegisterMapping_MultipleMappings_WorkCorrectly()
    {
        var exception1 = new InvalidOperationException("Invalid operation");
        var exception2 = new ArgumentNullException("param");
        var context = Substitute.For<HttpContext>();

        var problemDetails1 = new ProblemDetails
        {
            Type = "https://example.com/probs/invalid-operation",
            Title = "Invalid Operation",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Operation is invalid.",
            Instance = context.TraceIdentifier
        };

        var problemDetails2 = new ProblemDetails
        {
            Type = "https://example.com/probs/argument-null",
            Title = "Argument Null",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Parameter cannot be null.",
            Instance = context.TraceIdentifier
        };

        _mapper.RegisterMapping<InvalidOperationException>((ex, ctx) => problemDetails1);
        _mapper.RegisterMapping<ArgumentNullException>((ex, ctx) => problemDetails2);

        var result1 = _mapper.Map(exception1, context);
        var result2 = _mapper.Map(exception2, context);

        result1.Should().Be(problemDetails1);
        result2.Should().Be(problemDetails2);
    }

    [Fact]
    public void Map_DerivedException_UsesBaseMapping()
    {
        var baseException = new BaseCustomException("Base exception");
        var derivedException = new DerivedCustomException("Derived exception");
        var context = Substitute.For<HttpContext>();

        var baseProblemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/base-exception",
            Title = "Base Exception",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Base exception occurred.",
            Instance = context.TraceIdentifier
        };

        _mapper.RegisterMapping<BaseCustomException>((ex, ctx) => baseProblemDetails);

        var result = _mapper.Map(derivedException, context);

        result.Should().Be(baseProblemDetails);
    }

    public class BaseCustomException : Exception
    {
        public BaseCustomException(string message) : base(message) { }
    }

    public class DerivedCustomException : BaseCustomException
    {
        public DerivedCustomException(string message) : base(message) { }
    }
}