using SearchEngine.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Persistence.Services;

public class FileAttachmentCleaner
    : IFileAttachmentCleaner
{
    private readonly IApplicationBusinessDbContext
        _context;

    private readonly IFileAttachmentService
        _storage;

    public FileAttachmentCleaner(
        IApplicationBusinessDbContext context,
        IFileAttachmentService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task CleanupByOwnerAsync(
        string module,
        Guid recordId,
        CancellationToken cancellationToken)
    {
        var moduleCode =
            module.ToLower();

        var files =
            await _context.FileAttachments
                .Where(x =>
                    x.Module.ToLower() == moduleCode
                    && x.RecordId == recordId)
                .ToListAsync(cancellationToken);

        if (files.Count == 0)
        {
            return;
        }

        // Hapus baris database dulu (atomik) baru file fisik. Jika
        // penghapusan file fisik gagal, database tetap konsisten dan
        // file yatim di disk bisa dibersihkan terpisah.
        _context.FileAttachments
            .RemoveRange(files);

        await _context.SaveChangesAsync(
            cancellationToken);

        foreach (var file in files)
        {
            await _storage.DeleteAsync(
                file.FilePath,
                cancellationToken);
        }
    }
}
