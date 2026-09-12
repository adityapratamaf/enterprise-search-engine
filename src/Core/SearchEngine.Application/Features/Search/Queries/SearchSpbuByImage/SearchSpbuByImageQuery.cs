using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Search.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Search.Queries.SearchSpbuByImage;

/// <param name="Gambar">Isi berkas gambar.</param>
/// <param name="NamaBerkas">
/// Nama berkas asli — dipakai memeriksa ekstensi dan tanda tangan binernya.
/// </param>
/// <param name="PageNumber">Halaman hasil pencarian.</param>
/// <param name="PageSize">Banyak hasil per halaman.</param>
public record SearchSpbuByImageQuery(
    byte[] Gambar,
    string NamaBerkas,
    int PageNumber,
    int PageSize)
    : IRequest<Result<SearchByImageResponse>>;
