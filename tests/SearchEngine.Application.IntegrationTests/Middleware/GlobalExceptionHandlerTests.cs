using System.Net;
using System.Text;
using SearchEngine.Application.Common.Exceptions;
using SearchEngine.WebAPI.Middleware;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace SearchEngine.Application.IntegrationTests.Middleware;

/// <summary>
/// A-016: verifies the centralized <see cref="GlobalExceptionHandler"/>
/// (IExceptionHandler) maps each exception type to the correct HTTP status
/// and never leaks internal detail for unexpected errors.
/// </summary>
public class GlobalExceptionHandlerTests
{
    private static async Task<(int Status, string Body)> HandleAsync(
        Exception exception)
    {
        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/test";

        using var stream = new MemoryStream();
        context.Response.Body = stream;

        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        handled.Should().BeTrue();

        var body = Encoding.UTF8.GetString(stream.ToArray());
        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task Validation_Maps_To_400_With_Errors()
    {
        var exception = new ValidationException(new[]
        {
            new ValidationFailure("Name", "Name is required")
        });

        var (status, body) = await HandleAsync(exception);

        status.Should().Be((int)HttpStatusCode.BadRequest);
        body.Should().Contain("Validation failed");
        body.Should().Contain("Name is required");
    }

    [Fact]
    public async Task Unauthorized_Maps_To_401()
    {
        var (status, _) = await HandleAsync(
            new UnauthorizedException("no"));

        status.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Forbidden_Maps_To_403()
    {
        var (status, _) = await HandleAsync(
            new ForbiddenException("no"));

        status.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NotFound_Maps_To_404()
    {
        var (status, _) = await HandleAsync(
            new NotFoundException("missing"));

        status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Conflict_Maps_To_409()
    {
        var (status, _) = await HandleAsync(
            new ConflictException("dup"));

        status.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Unexpected_Maps_To_500_And_Does_Not_Leak_Detail()
    {
        var secret =
            "Password=SuperSecret123; at SearchEngine.Foo.Bar()";

        var (status, body) = await HandleAsync(
            new InvalidOperationException(secret));

        status.Should().Be((int)HttpStatusCode.InternalServerError);
        body.Should().Contain("Internal server error");

        // No exception message, stack frame, or secret leaks to the client.
        body.Should().NotContain("SuperSecret123");
        body.Should().NotContain("InvalidOperationException");
        body.Should().NotContain("at SearchEngine.Foo.Bar");
    }
}
