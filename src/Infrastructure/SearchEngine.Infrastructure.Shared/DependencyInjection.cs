using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Infrastructure.Shared.Storage;
using SearchEngine.Infrastructure.Shared.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Infrastructure.Shared;

public static class DependencyInjection
{
    public static IServiceCollection
        AddSharedInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddScoped<
            IFileAttachmentService,
            LocalStorageService>();

        // SMTP
        services.AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection(SmtpSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<
            IEmailService,
            EmailService>();

        services.AddScoped<
            IEmailDispatcher,
            EmailDispatcher>();

        return services;
    }
}
