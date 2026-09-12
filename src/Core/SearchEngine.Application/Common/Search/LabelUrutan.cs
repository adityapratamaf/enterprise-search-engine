namespace SearchEngine.Application.Common.Search;

/// <summary>
/// Menyusun keterangan dasar pengurutan yang benar-benar dipakai sebuah
/// mesin, untuk ditampilkan pada kendali "Urutkan" di antarmuka.
///
/// Dibuat terpisah karena untuk permintaan yang sama kedua mesin dapat
/// menghasilkan label berbeda: tanpa kolom urut yang eksplisit,
/// Elasticsearch memeringkat menurut relevansi sedangkan SQL hanya mampu
/// mengurutkan menurut abjad. Klien tidak perlu menebak yang mana.
/// </summary>
public static class LabelUrutan
{
    public static string Susun(
        SearchEngineKind engine,
        string? sortBy,
        bool isDescending,
        bool adaKeyword,
        bool adaKoordinat)
    {
        var arah = isDescending ? "Z - A" : "A - Z";

        switch (sortBy?.Trim().ToLowerInvariant())
        {
            case "nama":
                return $"Nama {arah}";

            case "kode":
                return isDescending
                    ? "Kode menurun"
                    : "Kode menaik";

            case "nozzle":
                return isDescending
                    ? "Nozzle terbanyak"
                    : "Nozzle tersedikit";

            case "rating":
                return isDescending
                    ? "Rating tertinggi"
                    : "Rating terendah";

            case "jarak"
                when engine == SearchEngineKind.Elasticsearch
                    && adaKoordinat:
                return "Jarak terdekat";
        }

        if (engine == SearchEngineKind.Elasticsearch && adaKeyword)
        {
            return "Relevansi";
        }

        return $"Nama {arah}";
    }
}
