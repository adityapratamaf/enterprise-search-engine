using SearchEngine.Application.Common.Search;

using FluentAssertions;

namespace SearchEngine.Application.UnitTests.Features.Search;

/// <summary>
/// Menguji penurunan kata kunci dari teks hasil pembacaan gambar.
///
/// Bagian ini diuji tersendiri karena menentukan kualitas hasil jauh lebih
/// besar daripada yang terlihat, dan merupakan satu-satunya bagian alur OCR
/// yang dapat diperiksa tanpa berkas data bahasa maupun pustaka native.
/// </summary>
public class OcrKeywordExtractorTests
{
    [Theory]
    [InlineData("SPBU 34.12708")]
    [InlineData("SPBU 3412708")]
    [InlineData("SPBU 34-12708")]
    [InlineData("PERTAMINA\nSPBU 34 12708\nJl. Sudirman")]
    public void Kode_Spbu_Dikenali_Dan_Dirapikan(
        string teks)
    {
        var (kataKunci, kode) =
            OcrKeywordExtractor.Ekstrak(teks);

        kode.Should().Be("34.12708");
        kataKunci.Should().Be("34.12708");
    }

    [Fact]
    public void Kode_Spbu_Mengalahkan_Kata_Lain()
    {
        // Kode unik secara nasional, sehingga satu kode lebih menentukan
        // daripada berapa pun banyaknya nama jalan yang ikut terbaca.
        var (kataKunci, kode) =
            OcrKeywordExtractor.Ekstrak(
                "PERTAMINA\n"
                + "SPBU 31.12802\n"
                + "Jl. Jenderal Sudirman No. 45\n"
                + "Pertamax Pertalite Dexlite");

        kode.Should().Be("31.12802");
        kataKunci.Should().Be("31.12802");
    }

    [Fact]
    public void Tanpa_Kode_Mengambil_Kata_Yang_Membedakan()
    {
        var (kataKunci, kode) =
            OcrKeywordExtractor.Ekstrak(
                "PERTAMINA\n"
                + "Jl. Diponegoro Semarang\n"
                + "Pertamax Biosolar");

        kode.Should().BeNull();

        // "PERTAMINA" muncul di setiap plang sehingga tidak membedakan
        // apa pun, dan "Jl" terlalu pendek untuk dipercaya.
        kataKunci.Should().NotContain("PERTAMINA");
        kataKunci.Should().Contain("Diponegoro");
        kataKunci.Should().Contain("Semarang");
        kataKunci.Should().Contain("Pertamax");
    }

    [Fact]
    public void Angka_Lepas_Diabaikan()
    {
        // Angka pada plang biasanya nomor rumah atau harga — mencarinya
        // hanya menghasilkan kebisingan.
        var (kataKunci, _) =
            OcrKeywordExtractor.Ekstrak(
                "Jl. Merdeka No. 189\nRp 12500");

        kataKunci.Should().Contain("Merdeka");
        kataKunci.Should().NotContain("189");
        kataKunci.Should().NotContain("12500");
    }

    [Fact]
    public void Jumlah_Kata_Dibatasi()
    {
        // Semakin banyak kata, semakin sedikit SPBU yang dapat memenuhi
        // semuanya — sementara hasil pembacaan gambar terlalu rawan salah
        // untuk dipercaya sepenuhnya.
        var (kataKunci, _) =
            OcrKeywordExtractor.Ekstrak(
                "alpha bravo charlie delta echo foxtrot golf hotel india");

        kataKunci
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Should()
            .HaveCountLessThanOrEqualTo(6);
    }

    [Fact]
    public void Kata_Berulang_Tidak_Digandakan()
    {
        var (kataKunci, _) =
            OcrKeywordExtractor.Ekstrak(
                "Sudirman Sudirman SUDIRMAN Semarang");

        kataKunci
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Should()
            .HaveCount(2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!! ... ???")]
    [InlineData("PERTAMINA SPBU")]
    public void Teks_Tanpa_Isi_Berguna_Menghasilkan_Kosong(
        string? teks)
    {
        var (kataKunci, kode) =
            OcrKeywordExtractor.Ekstrak(teks);

        kataKunci.Should().BeEmpty();
        kode.Should().BeNull();
    }
}
