using System.Text.Encodings.Web;
using System.Text.Json;
using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Models;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace SearchEngine.WebAPI.Middleware;

/// <summary>
/// Centralized exception-to-HTTP mapping built on the framework-native
/// <see cref="IExceptionHandler"/> pipeline (registered with
/// <c>AddExceptionHandler</c> + <c>UseExceptionHandler</c>).
///
/// It is the single place that turns exceptions into HTTP responses and it
/// preserves the existing <see cref="Result{T}"/> error envelope, so the
/// public error contract is unchanged. Unexpected exceptions are logged with
/// full detail server-side but returned to the client as an opaque message,
/// so no stack trace, database error, connection string, or secret leaks.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

            // Menyamakan penamaan dengan respons sukses, yang diserialisasi
            // MVC dengan camelCase. Tanpa ini amplop Result yang sama
            // terbaca "message" saat berhasil dan "Message" saat gagal,
            // sehingga klien harus menangani dua bentuk untuk satu kontrak.
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validation:
                var errors = validation.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(v => v.ErrorMessage));

                await WriteAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Validation failed",
                    cancellationToken,
                    errors);
                return true;

            case NotFoundException:
                await WriteAsync(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    exception.Message,
                    cancellationToken);
                return true;

            case UnauthorizedException:
                await WriteAsync(
                    httpContext,
                    StatusCodes.Status401Unauthorized,
                    exception.Message,
                    cancellationToken);
                return true;

            case ForbiddenException:
                await WriteAsync(
                    httpContext,
                    StatusCodes.Status403Forbidden,
                    exception.Message,
                    cancellationToken);
                return true;

            case ConflictException:
                await WriteAsync(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    exception.Message,
                    cancellationToken);
                return true;

            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception processing {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);

                await WriteAsync(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    cancellationToken);
                return true;
        }
    }

    private static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string message,
        CancellationToken cancellationToken,
        object? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response =
            errors is null
                ? Result<object>.Failure(message)
                : Result<object>.Failure(message, errors);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                response,
                SerializerOptions),
            cancellationToken);
    }
}
