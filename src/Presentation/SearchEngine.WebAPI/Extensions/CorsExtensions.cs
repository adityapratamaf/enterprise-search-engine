namespace SearchEngine.WebAPI.Extensions;

public static class CorsExtensions
{
    private const string PolicyName =
        "SearchEngineCors";

    public static IServiceCollection
        AddSearchEngineCors(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        var allowedOrigins =
            configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>()
            ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(
                PolicyName,
                builder =>
                {
                    builder
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        return services;
    }

    public static IApplicationBuilder
        UseSearchEngineCors(
            this IApplicationBuilder app)
    {
        app.UseCors(PolicyName);

        return app;
    }
}

