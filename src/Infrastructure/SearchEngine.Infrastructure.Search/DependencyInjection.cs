using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Infrastructure.Search.Indexing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Infrastructure.Search;

public static class DependencyInjection
{
    public static IServiceCollection
        AddSearchInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddOptions<ElasticsearchOptions>()
            .Bind(configuration.GetSection(
                ElasticsearchOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options =
            configuration
                .GetSection(ElasticsearchOptions.SectionName)
                .Get<ElasticsearchOptions>()
            ?? throw new InvalidOperationException(
                "Bagian konfigurasi 'Elasticsearch' tidak ditemukan.");

        // Klien bersifat thread-safe dan memelihara kumpulan koneksinya
        // sendiri, sehingga didaftarkan sekali sebagai singleton.
        services.AddSingleton(_ =>
        {
            var settings =
                new ElasticsearchClientSettings(
                        new Uri(options.Uri))
                    .Authentication(
                        new BasicAuthentication(
                            options.Username,
                            options.Password))
                    .RequestTimeout(
                        TimeSpan.FromSeconds(
                            options.RequestTimeoutSeconds))
                    .MaximumRetries(options.MaxRetries);

            if (options.EnableDebugMode)
            {
                settings = settings.EnableDebugMode();
            }

            return new ElasticsearchClient(settings);
        });

        // Scoped: indexer membaca basis data lewat DbContext yang juga
        // scoped. Hangfire membuat scope tersendiri untuk setiap job.
        services.AddScoped<
            ISpbuIndexer,
            SpbuIndexer>();

        return services;
    }
}
