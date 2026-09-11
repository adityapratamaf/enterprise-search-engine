namespace SearchEngine.WebAPI.Extensions;

public static class ConfigurationValidationExtensions
{
    public static IServiceCollection
        ValidateConnectionStrings(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        Validate(configuration, "BusinessConnection");
        Validate(configuration, "IdentityConnection");
        Validate(configuration, "HangfireConnection");

        return services;
    }

    private static void Validate(
        IConfiguration configuration,
        string name)
    {
        if (string.IsNullOrWhiteSpace(
                configuration.GetConnectionString(name)))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' is missing.");
        }
    }
}
