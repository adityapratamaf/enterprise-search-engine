using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Infrastructure.Persistence.Context;
using SearchEngine.Infrastructure.Persistence.Interceptors;
using SearchEngine.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection
        AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Claims
        services.AddScoped<
            ICurrentUserService,
            CurrentUserService>();

        // Auditable
        services.AddScoped<
            AuditableEntityInterceptor>();

        // Cascade cleanup file attachment saat record induk dihapus
        services.AddScoped<
            IFileAttachmentCleaner,
            FileAttachmentCleaner>();

        services.AddScoped<
            IApplicationBusinessDbContext>(
            provider =>
                provider.GetRequiredService<
                    ApplicationBusinessDbContext>());

        services.AddDbContext<ApplicationBusinessDbContext>(
            (sp, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString(
                        "BusinessConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });

                options.AddInterceptors(
                    sp.GetRequiredService<
                        AuditableEntityInterceptor>());
            });

        return services;
    }
}
