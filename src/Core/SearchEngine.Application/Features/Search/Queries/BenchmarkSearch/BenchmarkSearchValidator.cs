using FluentValidation;

namespace SearchEngine.Application.Features.Search.Queries.BenchmarkSearch;

public class BenchmarkSearchValidator
    : AbstractValidator<BenchmarkSearchQuery>
{
    public BenchmarkSearchValidator()
    {
        // Dibatasi karena pengukuran berjalan sinkron di dalam permintaan
        // HTTP. Satu kueri SQL pada ratusan ribu baris dapat memakan
        // beberapa detik, sehingga sepuluh iterasi sudah cukup lama.
        RuleFor(x => x.Request.Iterasi)
            .InclusiveBetween(1, 10)
            .OverridePropertyName("Iterasi");

        RuleFor(x => x.Request.Contoh)
            .InclusiveBetween(0, 10)
            .OverridePropertyName("Contoh");

        RuleFor(x => x.Request.Search)
            .MaximumLength(200)
            .OverridePropertyName("Search");
    }
}
