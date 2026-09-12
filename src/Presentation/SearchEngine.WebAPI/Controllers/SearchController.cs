using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.Search.DTOs;
using SearchEngine.Application.Features.Search.Queries.BenchmarkSearch;
using SearchEngine.Application.Features.Search.Queries.SearchSpbu;
using SearchEngine.Application.Features.Search.Queries.SearchSpbuByImage;
using SearchEngine.Application.Features.Search.Queries.SuggestSpbu;
using SearchEngine.WebAPI.Jobs;

using Hangfire;
using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("api")]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    private readonly IBackgroundJobClient _backgroundJobClient;

    public SearchController(
        IMediator mediator,
        IBackgroundJobClient backgroundJobClient)
    {
        _mediator = mediator;
        _backgroundJobClient = backgroundJobClient;
    }

    // =====================================================
    // PENCARIAN
    // =====================================================

    [HttpGet("spbu")]
    [HasPermission("search", "view")]
    [EndpointDescription(
        "Pencarian SPBU: full-text dengan toleransi salah ketik, sinonim "
        + "alamat (\"jl\" dikenali sebagai \"jalan\"), peringkat relevansi, "
        + "penyorotan kata yang cocok, hitungan facet untuk panel penyaring, "
        + "penyaringan wilayah/produk/fasilitas/status, pencarian radius, "
        + "dan paginasi. "
        + "Parameter engine dapat diisi Sql untuk membandingkan hasilnya "
        + "dengan pencarian LIKE biasa — pada mode itu penyorotan dan facet "
        + "tidak tersedia, dan hal tersebut dilaporkan lewat properti "
        + "kemampuan serta catatan.")]
    public async Task<IActionResult> SearchSpbu(
        [FromQuery] SearchSpbuRequest request)
    {
        var result =
            await _mediator.Send(
                new SearchSpbuQuery(request));

        return Ok(result);
    }

    // =====================================================
    // SARAN KETIK-LANGSUNG
    // =====================================================

    [HttpGet("spbu/suggestion")]
    [HasPermission("search", "view")]
    [EndpointDescription(
        "Saran nama SPBU untuk kotak pencarian, dicocokkan per awalan kata. "
        + "Dipanggil pada setiap ketukan tombol, sehingga muatannya sengaja "
        + "dibuat ringkas. Kata kunci di bawah dua huruf mengembalikan daftar "
        + "kosong, bukan error.")]
    public async Task<IActionResult> SuggestSpbu(
        [FromQuery] string q,
        [FromQuery] int limit = 10)
    {
        var result =
            await _mediator.Send(
                new SuggestSpbuQuery(q, limit));

        return Ok(result);
    }

    // =====================================================
    // PENCARIAN LEWAT GAMBAR
    // =====================================================

    [HttpPost("spbu/image")]
    [HasPermission("search", "view")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EndpointDescription(
        "Mencari SPBU dari foto: tulisan pada gambar dibaca, diubah menjadi "
        + "kata kunci, lalu dicari melalui jalur pencarian yang sama persis "
        + "dengan endpoint pencarian biasa — dengan Elasticsearch. "
        + "Bila gambar memuat kode SPBU, kode itu yang dipakai karena paling "
        + "menentukan; bila tidak, diambil kata-kata yang membedakan. "
        + "Teks mentah hasil pembacaan ikut dikembalikan supaya pengguna "
        + "dapat menilai sendiri dan memperbaiki kata kuncinya bila perlu. "
        + "Menerima PNG atau JPEG, maksimal 10 MB.")]
    public async Task<IActionResult> SearchSpbuByImage(
        IFormFile file,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(
                Result<object>.Failure("Berkas gambar wajib diunggah."));
        }

        using var memori = new MemoryStream();

        await file.CopyToAsync(
            memori,
            HttpContext.RequestAborted);

        var result =
            await _mediator.Send(
                new SearchSpbuByImageQuery(
                    memori.ToArray(),
                    file.FileName,
                    pageNumber,
                    pageSize));

        return Ok(result);
    }

    // =====================================================
    // PEMBANDINGAN MESIN
    // =====================================================

    [HttpGet("spbu/benchmark")]
    [HasPermission("search", "execute")]
    [EndpointDescription(
        "Menjalankan kata kunci yang sama pada Elasticsearch dan SQL Server, "
        + "lalu mengembalikan pemenang beserta waktu dan jumlah hasil "
        + "masing-masing. "
        + "Berjalan sinkron dan dapat memakan beberapa detik, karena satu "
        + "kueri SQL pada ratusan ribu baris memang selama itu.")]
    public async Task<IActionResult> BenchmarkSpbu(
        [FromQuery] BenchmarkRequest request)
    {
        var result =
            await _mediator.Send(
                new BenchmarkSearchQuery(request));

        return Ok(result);
    }

    // =====================================================
    // INDEXING ULANG
    // =====================================================

    [HttpPost("spbu/reindex")]
    [HasPermission("search", "execute")]
    [EndpointDescription(
        "Membangun ulang seluruh index pencarian SPBU dari basis data. "
        + "Permintaan ini hanya mengantrikan pekerjaan lalu langsung kembali; "
        + "prosesnya berjalan di latar belakang melalui Hangfire dan dapat "
        + "dipantau di dasbor /hangfire. "
        + "Index baru dibangun terpisah, lalu alias dipindahkan secara atomik "
        + "setelah seluruh dokumen masuk — sehingga pencarian tidak pernah "
        + "terganggu maupun melihat data setengah jadi.")]
    public IActionResult ReindexSpbu()
    {
        var jobId =
            _backgroundJobClient
                .Enqueue<SpbuReindexJob>(
                    job => job.RunAsync(
                        CancellationToken.None));

        return Accepted(
            Result<object>.SuccessResult(
                new
                {
                    jobId,
                    dashboard = "/hangfire"
                },
                "Indexing ulang SPBU telah diantrikan."));
    }
}
