namespace SearchEngine.WebAPI.Middleware;

public class CorrelationIdMiddleware
{
    private const string
        HeaderName =
            "X-Correlation-Id";

    private const int
        MaxCorrelationIdLength =
            128;

    private readonly RequestDelegate
        _next;

    public CorrelationIdMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(
        HttpContext context)
    {
        var correlationId =
            context.Request.Headers[
                HeaderName]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId)
            || correlationId.Length >
            MaxCorrelationIdLength)
        {
            correlationId =
                Guid.NewGuid()
                    .ToString();
        }

        context.Response.Headers[
            HeaderName]
            = correlationId;

        using (
            Serilog.Context.LogContext
                .PushProperty(
                    "CorrelationId",
                    correlationId))
        {
            await _next(context);
        }
    }
}
