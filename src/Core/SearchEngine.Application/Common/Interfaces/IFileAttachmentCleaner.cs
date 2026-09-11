namespace SearchEngine.Application.Common.Interfaces;

/// <summary>
/// Membersihkan seluruh file attachment (baris database + file fisik) milik
/// sebuah record ketika record induknya dihapus. Relasi attachment bersifat
/// polymorphic (tanpa FK) sehingga cascade harus dilakukan di level aplikasi.
/// </summary>
public interface IFileAttachmentCleaner
{
    Task CleanupByOwnerAsync(
        string module,
        Guid recordId,
        CancellationToken cancellationToken);
}
