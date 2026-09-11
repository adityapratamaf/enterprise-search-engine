using SearchEngine.Application.Common.Security;
using FluentAssertions;

namespace SearchEngine.Application.UnitTests.Features.Security;

/// <summary>
/// Unit tests for file-upload security validation (extension allow-list and
/// magic-number / MIME-spoof protection).
/// </summary>
public class FileSignatureValidatorTests
{
    [Theory]
    [InlineData("image.png", true)]
    [InlineData("image.PNG", true)]
    [InlineData("doc.pdf", true)]
    [InlineData("sheet.xlsx", true)]
    [InlineData("evil.exe", false)]
    [InlineData("notes.txt", false)]
    [InlineData("noextension", false)]
    public void IsAllowedExtension_Returns_Expected(
        string fileName,
        bool expected)
    {
        FileSignatureValidator
            .IsAllowedExtension(fileName)
            .Should()
            .Be(expected);
    }

    [Fact]
    public void HasValidSignature_Valid_Png_Returns_True()
    {
        var png = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47,
            0x0D, 0x0A, 0x1A, 0x0A,
            0x01, 0x02, 0x03
        };

        using var stream = new MemoryStream(png);

        FileSignatureValidator
            .HasValidSignature(stream, "image.png")
            .Should()
            .BeTrue();
    }

    [Fact]
    public void HasValidSignature_MimeSpoof_PngBytesWithExeName_Returns_False()
    {
        // Real PNG bytes but a disallowed/incorrect extension -> rejected.
        var png = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47,
            0x0D, 0x0A, 0x1A, 0x0A
        };

        using var stream = new MemoryStream(png);

        FileSignatureValidator
            .HasValidSignature(stream, "malware.exe")
            .Should()
            .BeFalse();
    }

    [Fact]
    public void HasValidSignature_WrongMagicBytes_Returns_False()
    {
        // Bytes that do not match the PNG signature -> rejected even though
        // the extension is allowed (content/extension mismatch).
        var fake = new byte[]
        {
            0x00, 0x01, 0x02, 0x03,
            0x04, 0x05, 0x06, 0x07
        };

        using var stream = new MemoryStream(fake);

        FileSignatureValidator
            .HasValidSignature(stream, "image.png")
            .Should()
            .BeFalse();
    }
}
