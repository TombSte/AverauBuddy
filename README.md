
# AverauBuddy

AverauBuddy is a robust ASP.NET Core middleware library designed to handle exceptions gracefully by mapping them to standardized Problem Details responses as per RFC 7807. This ensures consistent and informative error responses across your APIs, enhancing both developer experience and client-side error handling.

## Table of Contents
- Features
- Installation
- Usage
  - Adding the Middleware
  - Configuring Exception Mappings
- Examples
- Testing
- Contributing
- License
- Contact

## Features
- **Standardized Error Responses**: Automatically maps exceptions to Problem Details format.
- **Custom Exception Mappings**: Easily register custom mappings for specific exception types.
- **Flexible Configuration**: Configure the middleware to suit your application's needs.
- **Seamless Integration**: Simple setup with ASP.NET Core's middleware pipeline.
- **Extensive Testing**: Comprehensive unit tests ensure reliability and stability.
- **Continuous Deployment**: Automated GitHub Actions pipeline for building and publishing to NuGet.

## Installation
Install the AverauBuddy package from NuGet:

```bash
dotnet add package AverauBuddy --version 1.0.0
```

Or via the NuGet Package Manager:

```xml
<PackageReference Include="AverauBuddy" Version="1.0.0" />
```

## Usage

### Adding the Middleware
To integrate AverauBuddy into your ASP.NET Core application, follow these steps:

#### Register the Middleware Services

In your `Startup.cs` or `Program.cs` (for .NET 6 and above), add the AverauBuddy services to the dependency injection container:

```csharp
using AverauBuddy;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProblemDetails(mapper =>
        {
            // Register custom exception mappings here
        });
        
        // Other service registrations
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseProblemDetails();

        // Other middleware registrations
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

#### Configure Exception Mappings
You can register custom mappings for specific exceptions to tailor the ProblemDetails responses:

```csharp
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

    mapper.RegisterMapping<CannotParsePageException>((ex, ctx) => new ProblemDetails
    {
        Type = "https://yourdomain.com/probs/cannot-parse-page",
        Title = "Cannot Parse Page",
        Status = StatusCodes.Status422UnprocessableEntity,
        Detail = ex.Message,
        Instance = ctx.TraceIdentifier
    });
});
```

### Configuring Exception Mappings
The `ExceptionToProblemDetailsMapper` allows you to define how specific exceptions are transformed into ProblemDetails responses. This is useful for providing meaningful error information to API consumers.

#### Example: Custom Exception Mapping
Suppose you have a custom exception `CannotParsePageException`. You can map it as follows:

```csharp
public class CannotParsePageException : Exception
{
    public CannotParsePageException(string message) : base(message)
    {
    }

    // Additional properties or methods if needed
}

// In Startup.cs or Program.cs
services.AddProblemDetails(mapper =>
{
    mapper.RegisterMapping<CannotParsePageException>((ex, ctx) => new ProblemDetails
    {
        Type = "https://yourdomain.com/probs/cannot-parse-page",
        Title = "Cannot Parse Page",
        Status = StatusCodes.Status422UnprocessableEntity,
        Detail = ex.Message,
        Instance = ctx.TraceIdentifier
    });
});
```

## Examples

### Example: Handling a Generic Exception
When an unhandled exception occurs, AverauBuddy will automatically convert it into a standardized ProblemDetails response.

#### Controller Action:

```csharp
[ApiController]
[Route("[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet("error")]
    public IActionResult GetError()
    {
        throw new InvalidOperationException("An unexpected error occurred.");
    }
}
```

#### Response:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred.",
  "instance": "TRACE123"
}
```

### Example: Handling a Custom Exception
#### Controller Action:

```csharp
[HttpGet("parse-error")]
public IActionResult GetParseError()
{
    throw new CannotParsePageException("Cannot parse page with ID 999.");
}
```

#### Response:

```json
{
  "type": "https://yourdomain.com/probs/cannot-parse-page",
  "title": "Cannot Parse Page",
  "status": 422,
  "detail": "Cannot parse page with ID 999.",
  "instance": "TRACE123"
}
```

## Testing
AverauBuddy includes comprehensive unit tests to ensure reliability and stability. The test suite covers various scenarios, including:

- Handling of generic exceptions.
- Custom exception mappings.
- Fallback mappings for unmapped exceptions.
- Integration of middleware within the ASP.NET Core pipeline.

### Running Tests
To run the tests, navigate to the test project directory and execute:

```bash
dotnet test
```

Ensure that all tests pass before publishing or deploying your application.

## Contributing
Contributions are welcome! To contribute to AverauBuddy, follow these steps:

### Fork the Repository

### Create a Feature Branch

```bash
git checkout -b feature/YourFeature
```

### Commit Your Changes

```bash
git commit -m "Add new feature"
```

### Push to the Branch

```bash
git push origin feature/YourFeature
```

### Open a Pull Request

Provide a clear description of your changes and any relevant context or screenshots.

#### Guidelines
- **Code Quality**: Ensure your code follows consistent styling and best practices.
- **Testing**: Add or update tests to cover your changes.
- **Documentation**: Update the README.md or other documentation as needed.
- **Issue Reporting**: Report bugs or propose enhancements via GitHub Issues.

## License
This project is licensed under the MIT License. See the LICENSE file for details.