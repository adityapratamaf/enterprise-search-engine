using SearchEngine.Application.Features.Search.DTOs;

namespace SearchEngine.Application.Common.Interfaces;

/// <summary>
/// Membaca tulisan yang terkandung pada sebuah gambar.
///
/// Tugasnya berhenti di situ: mengubah gambar menjadi teks. Penentuan kata
/// kunci maupun pencariannya dikerjakan di tempat lain, sehingga mesin OCR
/// dapat diganti tanpa menyentuh logika pencarian sama sekali.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Menandakan mesin OCR siap dipakai — berkas data bahasa tersedia dan
    /// pustaka nativenya dapat dimuat. Dipakai untuk memberi pesan yang
    /// jelas alih-alih membiarkan permintaan gagal dengan kesalahan native
    /// yang tidak bisa dibaca siapa pun.
    /// </summary>
    bool Tersedia { get; }

    Task<OcrResult> BacaAsync(
        byte[] gambar,
        string namaBerkas,
        CancellationToken cancellationToken = default);
}
