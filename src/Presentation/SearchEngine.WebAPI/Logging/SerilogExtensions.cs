using Serilog;

namespace SearchEngine.WebAPI.Logging;

public static class SerilogExtensions
{
    public static WebApplicationBuilder
        AddSearchEngineLogging(
            this WebApplicationBuilder builder)
    {
        Log.Logger =
            new LoggerConfiguration()

            .ReadFrom.Configuration(
                builder.Configuration)

            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}
