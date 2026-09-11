using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Features
    .FileAttachments.DTOs;

using Microsoft.AspNetCore.Hosting;

namespace SearchEngine.Infrastructure.Shared.Storage;

public class LocalStorageService
    : IFileAttachmentService
{
    private readonly IWebHostEnvironment
        _environment;

    public LocalStorageService(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    // Upload
    public async Task<ReadFileUploadResponse>
        UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken)
    {
        var uploadFolder =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "storage",
                "uploads");

        Directory.CreateDirectory(
            uploadFolder);

        var uniqueFileName =
            $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        var fullPath =
            Path.Combine(
                uploadFolder,
                uniqueFileName);

        await using var fileStream =
            new FileStream(
                fullPath,
                FileMode.Create);

        await stream.CopyToAsync(
            fileStream,
            cancellationToken);

        return new ReadFileUploadResponse
        {
            FileName = uniqueFileName,

            FilePath =
                $"storage/uploads/{uniqueFileName}",

            FileSize = fileStream.Length,

            StorageProvider = "Local"
        };
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var rootPath =
            EnsureTrailingSeparator(
                Path.GetFullPath(
                    Directory.GetCurrentDirectory()));

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    rootPath,
                    filePath));

        if (!fullPath.StartsWith(
                rootPath,
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string EnsureTrailingSeparator(
        string path)
    {
        return path.EndsWith(
            Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}
