using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AverauBuddy.Test;

public class ProblemDetailsExtensionsTests
{
    [Fact]
    public void AddProblemDetails_RegistersServicesCorrectly()
    {
        var services = new ServiceCollection();

        services.AddProblemDetails(mapper =>
        {
            mapper.RegisterMapping<InvalidOperationException>((ex, ctx) => new ProblemDetails
            {
                Type = "https://example.com/probs/invalid-operation",
                Title = "Invalid Operation",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Instance = ctx.TraceIdentifier
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        var mapper = serviceProvider.GetService<IExceptionToProblemDetailsMapper>();
        mapper.Should().NotBeNull();

        var exception = new InvalidOperationException("Invalid operation occurred.");
        var context = Substitute.For<HttpContext>();
        context.TraceIdentifier.Returns("TRACE123");

        var problemDetails = mapper.Map(exception, context);

        problemDetails.Should().NotBeNull();
        problemDetails.Type.Should().Be("https://example.com/probs/invalid-operation");
        problemDetails.Title.Should().Be("Invalid Operation");
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Be("Invalid operation occurred.");
        problemDetails.Instance.Should().Be("TRACE123");
    }

    [Fact]
    public void UseProblemDetails_AddsMiddlewareToPipeline()
    {
        var services = new ServiceCollection();
        var mockMapper = Substitute.For<IExceptionToProblemDetailsMapper>();
        services.AddSingleton<IExceptionToProblemDetailsMapper>(mockMapper);
        var serviceProvider = services.BuildServiceProvider();

        var appBuilder = Substitute.For<IApplicationBuilder>();
        appBuilder.ApplicationServices.Returns(serviceProvider);

        appBuilder.Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>())
            .Returns(appBuilder);

        appBuilder.UseProblemDetails();

        appBuilder.Received(1).Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
    }
}
