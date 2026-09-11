namespace SearchEngine.Domain.Enums;

/// <summary>
/// Tingkat pada hierarki wilayah administratif Indonesia.
///
/// Hierarki disimpan pada satu tabel yang mereferensikan dirinya sendiri
/// (<c>Wilayah.ParentId</c>), sehingga tingkat yang lebih dalam dapat
/// ditambahkan tanpa mengubah struktur tabel.
/// </summary>
public enum LevelWilayah
{
    Provinsi = 1,

    KotaKabupaten = 2,

    Kecamatan = 3,

    Kelurahan = 4
}
