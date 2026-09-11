namespace SearchEngine.Application.Common.Security;

/// <summary>
/// Validasi tanda tangan biner (magic number) file yang dapat dipakai ulang.
/// Hanya tipe berikut yang didukung: PDF, PNG, JPEG, DOCX, XLSX.
/// File yang signature binernya tidak cocok dengan ekstensi akan ditolak.
/// </summary>
public static class FileSignatureValidator
{
    // DOCX & XLSX merupakan kontainer OOXML (ZIP), sehingga signature-nya
    // sama dengan signature ZIP.
    private static readonly byte[][] ZipSignatures =
    [
        [0x50, 0x4B, 0x03, 0x04],
        [0x50, 0x4B, 0x05, 0x06],
        [0x50, 0x4B, 0x07, 0x08]
    ];

    private static readonly IReadOnlyDictionary<string, byte[][]>
        Signatures =
            new Dictionary<string, byte[][]>(
                StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = [[0x25, 0x50, 0x44, 0x46]],
                [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
                [".jpg"] = [[0xFF, 0xD8, 0xFF]],
                [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
                [".docx"] = ZipSignatures,
                [".xlsx"] = ZipSignatures
            };

    public static IReadOnlyCollection<string> AllowedExtensions =>
        (IReadOnlyCollection<string>)Signatures.Keys;

    public static bool IsAllowedExtension(
        string fileName)
    {
        var extension =
            Path.GetExtension(fileName);

        return !string.IsNullOrEmpty(extension)
            && Signatures.ContainsKey(extension);
    }

    /// <summary>
    /// Mengembalikan true bila ekstensi didukung DAN header biner stream
    /// cocok dengan signature yang diharapkan untuk ekstensi tersebut.
    /// Posisi stream dikembalikan ke awal sehingga proses upload selanjutnya
    /// tetap dapat membaca seluruh isi file.
    /// </summary>
    public static bool HasValidSignature(
        Stream? stream,
        string fileName)
    {
        var extension =
            Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(extension)
            || !Signatures.TryGetValue(
                extension,
                out var candidates))
        {
            return false;
        }

        if (stream is null || !stream.CanRead)
        {
            return false;
        }

        var maxLength =
            candidates.Max(x => x.Length);

        var header =
            ReadHeader(stream, maxLength);

        return candidates.Any(signature =>
            StartsWith(header, signature));
    }

    private static byte[] ReadHeader(
        Stream stream,
        int count)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var buffer = new byte[count];

        var totalRead = 0;

        while (totalRead < count)
        {
            var read =
                stream.Read(
                    buffer,
                    totalRead,
                    count - totalRead);

            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        if (totalRead < count)
        {
            Array.Resize(ref buffer, totalRead);
        }

        return buffer;
    }

    private static bool StartsWith(
        byte[] header,
        byte[] signature)
    {
        if (header.Length < signature.Length)
        {
            return false;
        }

        for (var i = 0; i < signature.Length; i++)
        {
            if (header[i] != signature[i])
            {
                return false;
            }
        }

        return true;
    }
}
