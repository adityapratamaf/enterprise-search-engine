using SearchEngine.Application.Common.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Infrastructure.Ocr;

public static class DependencyInjection
{
    public static IServiceCollection
        AddOcrInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddOptions<OcrOptions>()
            .Bind(configuration.GetSection(OcrOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Sengaja TIDAK memeriksa keberadaan berkas data bahasa di sini.
        // Aplikasi harus tetap bisa hidup tanpa OCR — seluruh fitur
        // pencarian lain tidak bergantung padanya — dan ketidaktersediaan
        // dilaporkan saat OCR benar-benar dipanggil.
        services.AddScoped<
            IOcrService,
            TesseractOcrService>();

        return services;
    }
}
