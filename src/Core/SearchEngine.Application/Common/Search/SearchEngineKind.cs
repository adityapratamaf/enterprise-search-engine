using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace SearchEngine.Application.Common.Search;

/// <summary>
/// Mesin yang melayani sebuah permintaan pencarian.
///
/// Pilihan ini ada untuk keperluan perbandingan: <see cref="Sql"/> mewakili
/// pencarian apa adanya dengan <c>LIKE</c> di basis data, sehingga selisih
/// kemampuan dan kecepatannya terhadap <see cref="Elasticsearch"/> dapat
/// dilihat langsung. Bawaannya selalu Elasticsearch.
///
/// Hanya dua nilai yang dideklarasikan supaya daftar pilihan pada
/// dokumentasi API tetap ringkas. Penulisan singkat seperti <c>es</c>
/// tetap diterima, ditangani <see cref="SearchEngineKindConverter"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SearchEngineKind>))]
[TypeConverter(typeof(SearchEngineKindConverter))]
public enum SearchEngineKind
{
    Elasticsearch = 1,

    Sql = 2
}

/// <summary>
/// Menerima penulisan ringkas pada query string tanpa menambahkan anggota
/// enum baru — anggota tambahan akan ikut muncul sebagai pilihan terpisah
/// di dokumentasi API dan membingungkan pembacanya.
///
/// Hanya memengaruhi pengikatan masukan; keluaran tetap diserialisasi
/// sebagai nama resminya.
/// </summary>
public sealed class SearchEngineKindConverter
    : EnumConverter
{
    public SearchEngineKindConverter()
        : base(typeof(SearchEngineKind))
    {
    }

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string teks)
        {
            switch (teks.Trim().ToLowerInvariant())
            {
                case "es":
                case "elastic":
                case "elasticsearch":
                    return SearchEngineKind.Elasticsearch;

                case "sql":
                case "sqlserver":
                case "sql-server":
                    return SearchEngineKind.Sql;
            }
        }

        return base.ConvertFrom(context, culture, value);
    }
}
