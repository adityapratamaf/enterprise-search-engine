# tessdata

Folder ini menampung berkas data bahasa Tesseract yang dipakai fitur
**pencarian lewat gambar** (`POST /api/search/spbu/image`).

Berkasnya **tidak ikut repositori** karena berukuran besar (puluhan MB) dan
merupakan artefak pihak ketiga yang lebih tepat diunduh terpisah.

## Berkas yang dibutuhkan

Letakkan dua berkas berikut langsung di folder ini:

```
tessdata/
├── ind.traineddata     ← Bahasa Indonesia
└── eng.traineddata     ← Bahasa Inggris
```

Unduh dari <https://github.com/tesseract-ocr/tessdata>.

Repositori itu menyediakan tiga varian, dan pilihannya berpengaruh nyata:

| Varian | Ukuran | Catatan |
|---|---|---|
| `tessdata` | sedang | Seimbang. Cukup untuk sebagian besar keperluan |
| `tessdata_best` | besar | Paling akurat, paling lambat |
| `tessdata_fast` | kecil | Paling cepat, paling sering keliru |

Untuk foto plang SPBU di lapangan — sering miring, silau, atau kurang
cahaya — **`tessdata_best` biasanya sepadan** dengan tambahan waktunya.

## Kenapa dua bahasa

Plang SPBU memuat keduanya sekaligus: nama jalan berbahasa Indonesia
berdampingan dengan istilah seperti "24 hours" atau "self service".
Konfigurasi bawaannya `ind+eng`, dan Tesseract menuntut berkas untuk
**setiap** bahasa yang disebut — kekurangan salah satu membuat pembacaan
gagal seluruhnya.

Daftar bahasa dapat diubah lewat `Ocr:Bahasa` pada `appsettings.json`.

## Memeriksa kesiapan

Selama berkasnya belum ada, aplikasi tetap berjalan normal — seluruh fitur
pencarian lain tidak bergantung pada OCR. Yang terjadi hanyalah endpoint
pencarian lewat gambar membalas pesan bahwa layanan OCR belum siap.
