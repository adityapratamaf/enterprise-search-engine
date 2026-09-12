using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using SearchEngine.Infrastructure.Search.Internal;

using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Search;
using SearchEngine.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HttpMethod = Elastic.Transport.HttpMethod;

namespace SearchEngine.Infrastructure.Search.Indexing;

/// <summary>
/// Membangun index pencarian SPBU dari SQL Server.
///
/// Seluruh permintaan dikirim lewat lapisan transport sebagai JSON mentah,
/// bukan lewat API berjenjang klien. Pilihan ini disengaja: berkas mapping
/// yang dipakai adalah berkas yang sama persis dengan yang sudah diuji
/// langsung di Kibana Dev Tools, sehingga tidak ada risiko perilaku berubah
/// karena kesalahan penerjemahan ke bentuk lain.
/// </summary>
public sealed class SpbuIndexer
    : ISpbuIndexer
{
    private const string MappingResource =
        "SearchEngine.Infrastructure.Search.Indexing.Mappings.spbu-index.json";

    private const string ContentTypeJson =
        ElasticsearchGateway.JsonContentType;

    private const string ContentTypeNdJson =
        ElasticsearchGateway.NdJsonContentType;

    private static readonly JsonSerializerOptions DocumentJson =
        new()
        {
            DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

    private readonly ElasticsearchGateway _gateway;

    private readonly IApplicationBusinessDbContext _context;

    private readonly ElasticsearchOptions _options;

    private readonly ILogger<SpbuIndexer> _logger;

    public SpbuIndexer(
        ElasticsearchGateway gateway,
        IApplicationBusinessDbContext context,
        IOptions<ElasticsearchOptions> options,
        ILogger<SpbuIndexer> logger)
    {
        _gateway = gateway;
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    private string Alias =>
        SearchIndexNames.SpbuAlias(_options.IndexPrefix);

    // =====================================================================
    // INDEXING ULANG PENUH
    // =====================================================================

    public async Task<SpbuReindexResult> ReindexAllAsync(
        CancellationToken cancellationToken = default)
    {
        var jam = Stopwatch.StartNew();

        var indexBaru =
            SearchIndexNames.NewSpbuIndex(
                _options.IndexPrefix,
                DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "Indexing ulang dimulai ke {Index}.",
            indexBaru);

        await BuatIndexAsync(indexBaru, cancellationToken);

        var (berhasil, gagal) =
            await IsiIndexAsync(indexBaru, cancellationToken);

        await SelesaikanIndexAsync(indexBaru, cancellationToken);

        // Alias hanya dipindahkan bila seluruh dokumen diterima. Bila ada
        // yang ditolak, index lama tetap melayani pencarian dan index baru
        // ditinggalkan apa adanya untuk diperiksa.
        if (gagal > 0)
        {
            _logger.LogError(
                "Indexing ulang dibatalkan: {Gagal} dokumen ditolak. "
                + "Alias tetap menunjuk index lama; {Index} dibiarkan untuk diperiksa.",
                gagal,
                indexBaru);

            return new SpbuReindexResult(
                indexBaru,
                berhasil,
                gagal,
                [],
                jam.Elapsed);
        }

        var indexLama =
            await PindahkanAliasAsync(indexBaru, cancellationToken);

        foreach (var lama in indexLama)
        {
            await _gateway.SendAsync(
                HttpMethod.DELETE,
                lama,
                null,
                ContentTypeJson,
                cancellationToken);
        }

        jam.Stop();

        _logger.LogInformation(
            "Indexing ulang selesai dalam {Durasi:0.0}s: {Jumlah} dokumen, "
            + "alias {Alias} menunjuk {Index}, {Dihapus} index lama dibersihkan.",
            jam.Elapsed.TotalSeconds,
            berhasil,
            Alias,
            indexBaru,
            indexLama.Count);

        return new SpbuReindexResult(
            indexBaru,
            berhasil,
            0,
            indexLama,
            jam.Elapsed);
    }

    // =====================================================================
    // PENULISAN SEBAGIAN (dipakai tahap berikutnya lewat outbox)
    // =====================================================================

    public async Task<int> IndexAsync(
        IReadOnlyCollection<Guid> spbuIds,
        CancellationToken cancellationToken = default)
    {
        if (spbuIds.Count == 0)
        {
            return 0;
        }

        var dokumen =
            await BacaDokumenAsync(
                spbuIds,
                cancellationToken);

        if (dokumen.Count == 0)
        {
            return 0;
        }

        var (berhasil, _) =
            await KirimBulkAsync(
                Alias,
                dokumen,
                cancellationToken);

        return berhasil;
    }

    public async Task<int> RemoveAsync(
        IReadOnlyCollection<Guid> spbuIds,
        CancellationToken cancellationToken = default)
    {
        if (spbuIds.Count == 0)
        {
            return 0;
        }

        var baris =
            spbuIds
                .Select(id =>
                    JsonSerializer.Serialize(
                        new
                        {
                            delete = new
                            {
                                _id = id.ToString()
                            }
                        }))
                .ToList();

        var respons =
            await KirimBulkMentahAsync(
                Alias,
                baris,
                cancellationToken);

        return HitungBerhasil(respons).Berhasil;
    }

    // =====================================================================
    // LANGKAH-LANGKAH INDEXING ULANG
    // =====================================================================

    private async Task BuatIndexAsync(
        string index,
        CancellationToken cancellationToken)
    {
        var mapping =
            JsonNode.Parse(BacaMapping())!
            as JsonObject
            ?? throw new InvalidOperationException(
                "Berkas mapping tidak berbentuk objek JSON.");

        var settings =
            mapping["settings"]!.AsObject();

        settings["number_of_shards"] = _options.DefaultShards;

        // Selama pembangunan, replika dimatikan dan penyegaran ditunda:
        // keduanya pekerjaan sia-sia untuk index yang belum dipakai, dan
        // menghilangkannya mempercepat penulisan massal secara signifikan.
        settings["number_of_replicas"] = 0;
        settings["refresh_interval"] = "-1";

        await _gateway.SendAsync(
            HttpMethod.PUT,
            index,
            mapping.ToJsonString(),
            ContentTypeJson,
            cancellationToken);
    }

    private async Task<(int Berhasil, int Gagal)> IsiIndexAsync(
        string index,
        CancellationToken cancellationToken)
    {
        var totalBerhasil = 0;
        var totalGagal = 0;

        Guid? kursor = null;

        while (true)
        {
            // Paginasi berbasis kunci, bukan Skip/Take: biayanya tetap
            // datar berapa pun dalamnya, dan tidak melewatkan baris bila
            // ada perubahan data saat proses berjalan.
            var query =
                _context.Spbus
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .AsQueryable();

            if (kursor is not null)
            {
                query = query.Where(x => x.Id > kursor.Value);
            }

            var batch =
                await ProyeksiSpbu(query)
                    .Take(_options.BulkBatchSize)
                    .ToListAsync(cancellationToken);

            if (batch.Count == 0)
            {
                break;
            }

            kursor = batch[^1].Id;

            var dokumen =
                await LengkapiRelasiAsync(
                    batch,
                    cancellationToken);

            var (berhasil, gagal) =
                await KirimBulkAsync(
                    index,
                    dokumen,
                    cancellationToken);

            totalBerhasil += berhasil;
            totalGagal += gagal;

            _logger.LogDebug(
                "Batch terkirim: {Berhasil} berhasil, {Gagal} gagal "
                + "(kumulatif {Total}).",
                berhasil,
                gagal,
                totalBerhasil);
        }

        return (totalBerhasil, totalGagal);
    }

    private async Task SelesaikanIndexAsync(
        string index,
        CancellationToken cancellationToken)
    {
        var settings =
            JsonSerializer.Serialize(
                new
                {
                    index = new Dictionary<string, object>
                    {
                        ["refresh_interval"] = "1s",
                        ["number_of_replicas"] = _options.DefaultReplicas
                    }
                });

        await _gateway.SendAsync(
            HttpMethod.PUT,
            $"{index}/_settings",
            settings,
            ContentTypeJson,
            cancellationToken);

        // Memaksa penyegaran sekali agar seluruh dokumen langsung terlihat
        // sebelum alias dipindahkan.
        await _gateway.SendAsync(
            HttpMethod.POST,
            $"{index}/_refresh",
            null,
            ContentTypeJson,
            cancellationToken);
    }

    /// <summary>
    /// Memindahkan alias ke index baru dalam satu permintaan. Elasticsearch
    /// menjalankan seluruh aksi pada permintaan ini secara atomik, sehingga
    /// tidak ada momen di mana alias menunjuk dua index atau tidak menunjuk
    /// apa pun.
    /// </summary>
    private async Task<IReadOnlyCollection<string>> PindahkanAliasAsync(
        string indexBaru,
        CancellationToken cancellationToken)
    {
        var indexLama =
            await CariIndexLamaAsync(
                indexBaru,
                cancellationToken);

        var actions = new JsonArray();

        foreach (var lama in indexLama)
        {
            actions.Add(new JsonObject
            {
                ["remove"] = new JsonObject
                {
                    ["index"] = lama,
                    ["alias"] = Alias
                }
            });
        }

        actions.Add(new JsonObject
        {
            ["add"] = new JsonObject
            {
                ["index"] = indexBaru,
                ["alias"] = Alias,
                ["is_write_index"] = true
            }
        });

        var body = new JsonObject
        {
            ["actions"] = actions
        };

        await _gateway.SendAsync(
            HttpMethod.POST,
            "_aliases",
            body.ToJsonString(),
            ContentTypeJson,
            cancellationToken);

        return indexLama;
    }

    private async Task<IReadOnlyCollection<string>> CariIndexLamaAsync(
        string indexBaru,
        CancellationToken cancellationToken)
    {
        var pola =
            SearchIndexNames.SpbuPattern(_options.IndexPrefix);

        var respons =
            await _gateway.SendAsync(
                HttpMethod.GET,
                $"_cat/indices/{pola}?format=json&h=index",
                null,
                ContentTypeJson,
                cancellationToken,
                terimaGagal: true);

        if (string.IsNullOrWhiteSpace(respons))
        {
            return [];
        }

        using var doc = JsonDocument.Parse(respons);

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return doc.RootElement
            .EnumerateArray()
            .Select(x =>
                x.TryGetProperty("index", out var i)
                    ? i.GetString()
                    : null)
            .Where(x =>
                !string.IsNullOrEmpty(x)
                && x != indexBaru)
            .Select(x => x!)
            .ToList();
    }

    // =====================================================================
    // MEMBACA DATA DAN MEMBENTUK DOKUMEN
    // =====================================================================

    /// <summary>
    /// Baris mentah SPBU beserta rantai wilayah yang sudah diratakan pada
    /// tingkat basis data, sehingga relasi tidak perlu dimuat sebagai objek.
    /// </summary>
    private sealed class SpbuRow
    {
        public Guid Id { get; init; }
        public string KodeSpbu { get; init; } = default!;
        public string Nama { get; init; } = default!;
        public string Alamat { get; init; } = default!;
        public string? KodePos { get; init; }
        public string Kota { get; init; } = default!;
        public string KodeKota { get; init; } = default!;
        public string Provinsi { get; init; } = default!;
        public string KodeProvinsi { get; init; } = default!;
        public string Regional { get; init; } = default!;
        public string RegionalNama { get; init; } = default!;
        public int RegionalNomor { get; init; }
        public string TipeKepemilikan { get; init; } = default!;
        public string Status { get; init; } = default!;
        public int JumlahDispenser { get; init; }
        public int JumlahNozzle { get; init; }
        public DateTime? TanggalOperasi { get; init; }
        public string? NomorTelepon { get; init; }
        public decimal? Rating { get; init; }
        public int JumlahUlasan { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }

    private static IQueryable<SpbuRow> ProyeksiSpbu(
        IQueryable<Spbu> query)
    {
        return query.Select(x => new SpbuRow
        {
            Id = x.Id,
            KodeSpbu = x.KodeSpbu,
            Nama = x.Nama,
            Alamat = x.Alamat,
            KodePos = x.KodePos,

            Kota = x.Wilayah.Nama,
            KodeKota = x.Wilayah.Kode,

            Provinsi =
                x.Wilayah.Parent != null
                    ? x.Wilayah.Parent.Nama
                    : string.Empty,

            KodeProvinsi =
                x.Wilayah.Parent != null
                    ? x.Wilayah.Parent.Kode
                    : string.Empty,

            Regional =
                x.Wilayah.Parent != null && x.Wilayah.Parent.Regional != null
                    ? x.Wilayah.Parent.Regional.Kode
                    : string.Empty,

            RegionalNama =
                x.Wilayah.Parent != null && x.Wilayah.Parent.Regional != null
                    ? x.Wilayah.Parent.Regional.Nama
                    : string.Empty,

            RegionalNomor =
                x.Wilayah.Parent != null && x.Wilayah.Parent.Regional != null
                    ? x.Wilayah.Parent.Regional.Nomor
                    : 0,

            TipeKepemilikan = x.TipeKepemilikan.ToString(),
            Status = x.Status.ToString(),
            JumlahDispenser = x.JumlahDispenser,
            JumlahNozzle = x.JumlahNozzle,
            TanggalOperasi = x.TanggalOperasi,
            NomorTelepon = x.NomorTelepon,
            Rating = x.Rating,
            JumlahUlasan = x.JumlahUlasan,
            Latitude = x.Latitude,
            Longitude = x.Longitude
        });
    }

    private async Task<List<SpbuDocument>> BacaDokumenAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        var batch =
            await ProyeksiSpbu(
                _context.Spbus
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.Id)))
                .ToListAsync(cancellationToken);

        return await LengkapiRelasiAsync(
            batch,
            cancellationToken);
    }

    /// <summary>
    /// Melengkapi baris SPBU dengan daftar produk dan fasilitasnya. Relasi
    /// diambil sekali per batch, bukan per SPBU, sehingga jumlah perjalanan
    /// ke basis data tetap dua berapa pun besar batch-nya.
    /// </summary>
    private async Task<List<SpbuDocument>> LengkapiRelasiAsync(
        List<SpbuRow> batch,
        CancellationToken cancellationToken)
    {
        var ids =
            batch.Select(x => x.Id).ToList();

        var produk =
            await _context.SpbuProduks
                .AsNoTracking()
                .Where(x =>
                    ids.Contains(x.SpbuId)
                    && x.IsActive)
                .Select(x => new
                {
                    x.SpbuId,
                    x.ProdukBbm.Kode,
                    x.ProdukBbm.Nama,
                    Jenis = x.ProdukBbm.Jenis
                })
                .ToListAsync(cancellationToken);

        var fasilitas =
            await _context.SpbuFasilitas
                .AsNoTracking()
                .Where(x => ids.Contains(x.SpbuId))
                .Select(x => new
                {
                    x.SpbuId,
                    x.Fasilitas.Kode,
                    x.Fasilitas.Nama
                })
                .ToListAsync(cancellationToken);

        var produkPerSpbu =
            produk
                .GroupBy(x => x.SpbuId)
                .ToDictionary(g => g.Key, g => g.ToList());

        var fasilitasPerSpbu =
            fasilitas
                .GroupBy(x => x.SpbuId)
                .ToDictionary(g => g.Key, g => g.ToList());

        var dokumen = new List<SpbuDocument>(batch.Count);

        foreach (var row in batch)
        {
            produkPerSpbu.TryGetValue(row.Id, out var p);
            fasilitasPerSpbu.TryGetValue(row.Id, out var f);

            dokumen.Add(new SpbuDocument
            {
                Id = row.Id,
                KodeSpbu = row.KodeSpbu,
                Nama = row.Nama,
                Alamat = row.Alamat,
                KodePos = row.KodePos,
                Kota = row.Kota,
                KodeKota = row.KodeKota,
                Provinsi = row.Provinsi,
                KodeProvinsi = row.KodeProvinsi,
                Regional = row.Regional,
                RegionalNama = row.RegionalNama,
                RegionalNomor = row.RegionalNomor,
                TipeKepemilikan = row.TipeKepemilikan,
                Status = row.Status,
                JumlahDispenser = row.JumlahDispenser,
                JumlahNozzle = row.JumlahNozzle,
                TanggalOperasi = row.TanggalOperasi,
                NomorTelepon = row.NomorTelepon,
                Rating = row.Rating,
                JumlahUlasan = row.JumlahUlasan,

                Lokasi = new SpbuLokasi
                {
                    Lat = row.Latitude,
                    Lon = row.Longitude
                },

                Produk =
                    p?.Select(x => x.Kode).ToArray() ?? [],

                ProdukNama =
                    p?.Select(x => x.Nama).ToArray() ?? [],

                JenisBbm =
                    p?.Select(x => x.Jenis.ToString())
                        .Distinct()
                        .ToArray() ?? [],

                Fasilitas =
                    f?.Select(x => x.Kode).ToArray() ?? [],

                FasilitasNama =
                    f?.Select(x => x.Nama).ToArray() ?? []
            });
        }

        return dokumen;
    }

    // =====================================================================
    // KOMUNIKASI DENGAN ELASTICSEARCH
    // =====================================================================

    private async Task<(int Berhasil, int Gagal)> KirimBulkAsync(
        string index,
        IReadOnlyCollection<SpbuDocument> dokumen,
        CancellationToken cancellationToken)
    {
        if (dokumen.Count == 0)
        {
            return (0, 0);
        }

        var baris = new List<string>(dokumen.Count * 2);

        foreach (var d in dokumen)
        {
            // Id dokumen = primary key SQL. Inilah yang membuat indexing
            // bersifat idempoten: menulis ulang dokumen yang sama menimpa,
            // bukan menggandakan.
            baris.Add(
                JsonSerializer.Serialize(
                    new
                    {
                        index = new
                        {
                            _id = d.Id.ToString()
                        }
                    }));

            baris.Add(
                JsonSerializer.Serialize(d, DocumentJson));
        }

        var respons =
            await KirimBulkMentahAsync(
                index,
                baris,
                cancellationToken);

        return HitungBerhasil(respons);
    }

    private async Task<string> KirimBulkMentahAsync(
        string index,
        IReadOnlyCollection<string> baris,
        CancellationToken cancellationToken)
    {
        var ndjson =
            new StringBuilder();

        foreach (var b in baris)
        {
            ndjson.Append(b).Append('\n');
        }

        return await _gateway.SendAsync(
            HttpMethod.POST,
            $"{index}/_bulk",
            ndjson.ToString(),
            ContentTypeNdJson,
            cancellationToken);
    }

    /// <summary>
    /// Menghitung dokumen yang diterima dan ditolak dari tanggapan _bulk.
    /// Elasticsearch membalas 200 meski sebagian dokumen gagal, sehingga
    /// status HTTP saja tidak cukup untuk menilai keberhasilan.
    /// </summary>
    private (int Berhasil, int Gagal) HitungBerhasil(
        string respons)
    {
        if (string.IsNullOrWhiteSpace(respons))
        {
            return (0, 0);
        }

        using var doc = JsonDocument.Parse(respons);

        if (!doc.RootElement.TryGetProperty("items", out var items))
        {
            return (0, 0);
        }

        var berhasil = 0;
        var gagal = 0;

        foreach (var item in items.EnumerateArray())
        {
            foreach (var op in item.EnumerateObject())
            {
                var status =
                    op.Value.TryGetProperty("status", out var s)
                        ? s.GetInt32()
                        : 500;

                if (status is >= 200 and < 300)
                {
                    berhasil++;
                    continue;
                }

                gagal++;

                if (gagal <= 3
                    && op.Value.TryGetProperty("error", out var err))
                {
                    _logger.LogError(
                        "Dokumen ditolak Elasticsearch: {Error}",
                        err.ToString());
                }
            }
        }

        return (berhasil, gagal);
    }

    private static string BacaMapping()
    {
        using var stream =
            Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(MappingResource)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{MappingResource}' tidak ditemukan.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
