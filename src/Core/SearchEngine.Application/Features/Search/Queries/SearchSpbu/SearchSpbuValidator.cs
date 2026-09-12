using FluentValidation;

namespace SearchEngine.Application.Features.Search.Queries.SearchSpbu;

public class SearchSpbuValidator
    : AbstractValidator<SearchSpbuQuery>
{
    /// <summary>
    /// Batas bawaan <c>index.max_result_window</c> Elasticsearch. Melewati
    /// angka ini membuat Elasticsearch menolak permintaan, bukan
    /// mengembalikan halaman kosong.
    /// </summary>
    private const int BatasHasil = 10_000;

    private const int MaksPageSize = 100;

    public SearchSpbuValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithName("PageNumber")
            .OverridePropertyName("PageNumber");

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, MaksPageSize)
            .WithName("PageSize")
            .OverridePropertyName("PageSize");

        // Paginasi dalam dibatasi, persis seperti mesin pencari pada
        // umumnya yang tidak mengizinkan menelusuri lewat halaman ke-100.
        // Untuk menjangkau hasil yang jauh, persempit kata kunci atau
        // gunakan penyaring — bukan menelusuri ribuan halaman.
        RuleFor(x => x.Request)
            .Must(r => (long)r.PageNumber * r.PageSize <= BatasHasil)
            .WithMessage(
                $"PageNumber dikali PageSize tidak boleh melebihi {BatasHasil:N0}. "
                + "Persempit kata kunci atau tambahkan penyaring.")
            .OverridePropertyName("PageNumber");

        RuleFor(x => x.Request.Search)
            .MaximumLength(200)
            .WithName("Search")
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

        // Ketiganya hanya bermakna bersama-sama.
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

        RuleFor(x => x.Request.FacetSize)
            .InclusiveBetween(1, 600)
            .OverridePropertyName("FacetSize");

        RuleFor(x => x.Request.RatingMin)
            .InclusiveBetween(1, 5)
            .When(x => x.Request.RatingMin.HasValue)
            .OverridePropertyName("RatingMin");

        RuleFor(x => x.Request.UlasanMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Request.UlasanMin.HasValue)
            .OverridePropertyName("UlasanMin");
    }
}
