using System.Diagnostics;

using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.Search.DTOs;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tesseract;

namespace SearchEngine.Infrastructure.Ocr;

/// <summary>
/// Pembacaan tulisan pada gambar memakai Tesseract.
///
/// Dua hal yang perlu diketahui tentang cara kelas ini bekerja:
///
/// <para>
/// <b>Mesinnya dibuat per permintaan, bukan dipakai bersama.</b>
/// <c>TesseractEngine</c> tidak aman dipakai beberapa thread sekaligus.
/// Membuatnya ulang memang menanggung pembacaan berkas bahasa setiap kali,
/// tetapi biaya itu kecil dibanding pembacaan gambarnya sendiri, dan
/// menukarnya dengan kepastian bebas dari kerusakan data antar-permintaan.
/// </para>
///
/// <para>
/// <b>Ketersediaannya diperiksa, tidak diasumsikan.</b> Berkas data bahasa
/// tidak ikut repositori dan pustaka nativenya bergantung pada sistem
/// operasi. Aplikasi tetap harus bisa hidup tanpa keduanya — seluruh fitur
/// pencarian lain tidak ada hubungannya dengan OCR — sehingga
/// ketidaktersediaan dilaporkan sebagai keterangan yang bisa dibaca, bukan
/// kegagalan saat start.
/// </para>
/// </summary>
public sealed class TesseractOcrService
    : IOcrService
{
    private static readonly string[] EkstensiGambar =
    [
        ".png", ".jpg", ".jpeg"
    ];

    private readonly OcrOptions _options;

    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(
        IOptions<OcrOptions> options,
        ILogger<TesseractOcrService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private string JalurTessData =>
        Path.IsPathRooted(_options.TessDataPath)
            ? _options.TessDataPath
            : Path.Combine(
                Directory.GetCurrentDirectory(),
                _options.TessDataPath);

    public bool Tersedia
    {
        get
        {
            if (!Directory.Exists(JalurTessData))
            {
                return false;
            }

            // Setiap bahasa yang diminta harus punya berkasnya sendiri;
            // Tesseract gagal bila salah satu saja tidak ada.
            return _options.Bahasa
                .Split('+', StringSplitOptions.RemoveEmptyEntries)
                .All(bahasa =>
                    File.Exists(
                        Path.Combine(
                            JalurTessData,
                            $"{bahasa.Trim()}.traineddata")));
        }
    }

    public Task<OcrResult> BacaAsync(
        byte[] gambar,
        string namaBerkas,
        CancellationToken cancellationToken = default)
    {
        PeriksaBerkas(gambar, namaBerkas);

        cancellationToken.ThrowIfCancellationRequested();

        var jam = Stopwatch.StartNew();

        try
        {
            using var engine =
                new TesseractEngine(
                    JalurTessData,
                    _options.Bahasa,
                    EngineMode.Default);

            using var pix = Pix.LoadFromMemory(gambar);

            // Auto: Tesseract menentukan sendiri tata letak halamannya.
            // Plang SPBU memuat beberapa blok teks dengan ukuran berbeda,
            // sehingga menetapkan satu mode tertentu justru merugikan.
            using var page =
                engine.Process(pix, PageSegMode.Auto);

            var teks = page.GetText() ?? string.Empty;

            var keyakinan = page.GetMeanConfidence();

            jam.Stop();

            _logger.LogInformation(
                "OCR selesai dalam {Durasi} ms: {Panjang} karakter, "
                + "keyakinan {Keyakinan:P0}.",
                jam.ElapsedMilliseconds,
                teks.Length,
                keyakinan);

            return Task.FromResult(new OcrResult
            {
                Teks = teks.Trim(),
                Keyakinan = Math.Round(keyakinan, 3),
                Bahasa = _options.Bahasa,
                DurasiMs = jam.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
            when (ex is DllNotFoundException or TypeInitializationException)
        {
            // Terjadi ketika pustaka native Tesseract tidak ada pada sistem
            // ini. Pesannya dibuat jelas agar tidak terbaca sebagai
            // kesalahan program.
            _logger.LogError(
                ex,
                "Pustaka native Tesseract gagal dimuat.");

            throw new ConflictException(
                "Pustaka native Tesseract tidak tersedia pada sistem ini. "
                + "Paket Tesseract hanya menyertakan berkas native untuk "
                + "Windows; sistem lain perlu memasangnya sendiri.");
        }
    }

    private void PeriksaBerkas(
        byte[] gambar,
        string namaBerkas)
    {
        if (gambar.Length == 0)
        {
            throw new ConflictException("Berkas gambar kosong.");
        }

        var maks = _options.MaksUkuranMb * 1024L * 1024L;

        if (gambar.Length > maks)
        {
            throw new ConflictException(
                $"Ukuran gambar melebihi batas {_options.MaksUkuranMb} MB.");
        }

        var ekstensi =
            Path.GetExtension(namaBerkas);

        if (!EkstensiGambar.Contains(
                ekstensi,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                "Format gambar tidak didukung. Gunakan PNG atau JPEG.");
        }

        // Memeriksa tanda tangan biner, bukan sekadar ekstensinya —
        // berkas yang namanya diubah tidak lantas menjadi gambar.
        using var stream = new MemoryStream(gambar);

        if (!FileSignatureValidator.HasValidSignature(stream, namaBerkas))
        {
            throw new ConflictException(
                "Isi berkas tidak cocok dengan ekstensinya.");
        }
    }
}
