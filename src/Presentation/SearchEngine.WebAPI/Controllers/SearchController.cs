using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using SearchEngine.WebAPI.Jobs;

using Hangfire;

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
    private readonly IBackgroundJobClient _backgroundJobClient;

    public SearchController(
        IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
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
