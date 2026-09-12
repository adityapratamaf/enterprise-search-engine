using FluentValidation;

namespace SearchEngine.Application.Features.Search.Queries.BenchmarkSearch;

public class BenchmarkSearchValidator
    : AbstractValidator<BenchmarkSearchQuery>
{
    /// <summary>
    /// Batas bawaan <c>index.max_result_window</c> Elasticsearch — sama
    /// dengan yang berlaku pada endpoint pencarian.
    /// </summary>
    private const int BatasHasil = 10_000;

    public BenchmarkSearchValidator()
    {
        // Dibatasi karena pengukuran berjalan sinkron di dalam permintaan
        // HTTP. Satu kueri SQL pada ratusan ribu baris dapat memakan
        // beberapa detik, sehingga sepuluh iterasi sudah cukup lama.
        RuleFor(x => x.Request.Iterasi)
            .InclusiveBetween(1, 10)
            .OverridePropertyName("Iterasi");

        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .OverridePropertyName("PageNumber");

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, 100)
            .OverridePropertyName("PageSize");

        RuleFor(x => x.Request)
            .Must(r => (long)r.PageNumber * r.PageSize <= BatasHasil)
            .WithMessage(
                $"PageNumber dikali PageSize tidak boleh melebihi {BatasHasil:N0}.")
            .OverridePropertyName("PageNumber");

        RuleFor(x => x.Request.Search)
            .MaximumLength(200)
            .OverridePropertyName("Search");

        RuleFor(x => x.Request.Lat)
            .InclusiveBetween(-90, 90)
            .When(x => x.Request.Lat.HasValue)
            .OverridePropertyName("Lat");

        RuleFor(x => x.Request.Lon)
            .InclusiveBetween(-180, 180)
            .When(x => x.Request.Lon.HasValue)
            .OverridePropertyName("Lon");

        RuleFor(x => x.Request.RadiusKm)
            .GreaterThan(0)
            .LessThanOrEqualTo(5_000)
            .When(x => x.Request.RadiusKm.HasValue)
            .OverridePropertyName("RadiusKm");

        RuleFor(x => x.Request)
            .Must(r =>
                (r.Lat.HasValue && r.Lon.HasValue && r.RadiusKm.HasValue)
                || (!r.Lat.HasValue && !r.Lon.HasValue && !r.RadiusKm.HasValue))
            .WithMessage(
                "Lat, Lon, dan RadiusKm harus diisi bersamaan atau dikosongkan semua.")
            .OverridePropertyName("RadiusKm");

        // Keempat batas kotak peta harus diisi bersamaan — separuh kotak
        // tidak menjelaskan area apa pun.
        RuleFor(x => x.Request)
            .Must(r =>
            {
                var terisi = new[]
                {
                    r.LatMin.HasValue, r.LonMin.HasValue,
                    r.LatMax.HasValue, r.LonMax.HasValue
                };

                return terisi.All(x => x) || terisi.All(x => !x);
            })
            .WithMessage(
                "LatMin, LonMin, LatMax, dan LonMax harus diisi bersamaan "
                + "atau dikosongkan semua.")
            .OverridePropertyName("LatMin");

        RuleFor(x => x.Request)
            .Must(r =>
                !r.LatMin.HasValue
                || (r.LatMin < r.LatMax && r.LonMin < r.LonMax))
            .WithMessage(
                "LatMin harus lebih kecil dari LatMax, dan LonMin lebih "
                + "kecil dari LonMax.")
            .OverridePropertyName("LatMin");

        RuleFor(x => x.Request.RatingMin)
            .InclusiveBetween(1, 5)
            .When(x => x.Request.RatingMin.HasValue)
            .OverridePropertyName("RatingMin");
    }
}
