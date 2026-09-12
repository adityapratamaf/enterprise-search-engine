using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Search;
using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.Search.DTOs;
using SearchEngine.Application.Features.Search.Queries.SearchSpbu;

using MediatR;

namespace SearchEngine.Application.Features.Search.Queries.SearchSpbuByImage;

/// <summary>
/// Mencari SPBU berdasarkan tulisan yang terbaca pada sebuah gambar.
///
/// Alurnya: baca gambar → turunkan kata kunci → cari.
///
/// Langkah pencariannya sengaja dikirim ulang sebagai
/// <see cref="SearchSpbuQuery"/>, bukan memanggil mesin pencari secara
/// langsung. Dengan begitu pencarian lewat gambar menempuh jalur yang benar-
/// benar sama dengan pencarian biasa — relevansi, sinonim, penyorotan, dan
/// facet yang sama — sehingga tidak mungkin keduanya berbeda perilaku.
/// </summary>
public class SearchSpbuByImageHandler
    : IRequestHandler<
        SearchSpbuByImageQuery,
        Result<SearchByImageResponse>>
{
    private readonly IOcrService _ocr;

    private readonly IMediator _mediator;

    public SearchSpbuByImageHandler(
        IOcrService ocr,
        IMediator mediator)
    {
        _ocr = ocr;
        _mediator = mediator;
    }

    public async Task<Result<SearchByImageResponse>> Handle(
        SearchSpbuByImageQuery request,
        CancellationToken cancellationToken)
    {
        if (!_ocr.Tersedia)
        {
            throw new ConflictException(
                "Layanan OCR belum siap. Pastikan berkas data bahasa "
                + "(ind.traineddata dan eng.traineddata) sudah tersedia di "
                + "folder tessdata.");
        }

        var teks =
            await _ocr.BacaAsync(
                request.Gambar,
                request.NamaBerkas,
                cancellationToken);

        var (kataKunci, kodeSpbu) =
            OcrKeywordExtractor.Ekstrak(teks.Teks);

        // Gambar tanpa tulisan yang berguna dikembalikan sebagai hasil
        // kosong yang menjelaskan diri, bukan sebagai kesalahan — dari sudut
        // pandang pengguna, fotonya memang berhasil diproses.
        if (string.IsNullOrWhiteSpace(kataKunci))
        {
            return Result<SearchByImageResponse>
                .SuccessResult(
                    new SearchByImageResponse
                    {
                        Ocr = teks,
                        KataKunci = string.Empty,
                        Hasil = new SearchSpbuResponse
                        {
                            PageNumber = request.PageNumber,
                            PageSize = request.PageSize,
                            Engine = SearchEngineKind.Elasticsearch
                        }
                    },
                    "Tidak ada tulisan yang dapat dipakai sebagai kata kunci "
                    + "pada gambar ini.");
        }

        var pencarian =
            await _mediator.Send(
                new SearchSpbuQuery(
                    new SearchSpbuRequest
                    {
                        Search = kataKunci,

                        // Selalu Elasticsearch: pencarian dari gambar sangat
                        // bergantung pada toleransi salah ketik, karena hasil
                        // pembacaan hampir selalu memuat kekeliruan kecil.
                        Engine = SearchEngineKind.Elasticsearch,

                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    }),
                cancellationToken);

        var hasil = new SearchByImageResponse
        {
            Ocr = teks,
            KataKunci = kataKunci,
            KodeSpbuTerdeteksi = kodeSpbu,
            Hasil = pencarian.Data ?? new SearchSpbuResponse()
        };

        var pesan =
            kodeSpbu is not null
                ? $"Kode SPBU {kodeSpbu} terbaca pada gambar; "
                  + $"{hasil.Hasil.TotalCount:N0} hasil ditemukan."
                : $"Kata kunci \"{kataKunci}\" diambil dari gambar; "
                  + $"{hasil.Hasil.TotalCount:N0} hasil ditemukan.";

        return Result<SearchByImageResponse>
            .SuccessResult(hasil, pesan);
    }
}
