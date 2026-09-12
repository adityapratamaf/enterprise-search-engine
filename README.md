# SearchEngine — Mesin Pencari SPBU & Rantai Pasok BBM

Backend API untuk **mencari SPBU di seluruh Indonesia** beserta kondisi bahan
bakarnya. Dibangun di atas ASP.NET Core 10 dengan Clean Architecture, memakai
SQL Server sebagai sumber kebenaran dan Elasticsearch sebagai mesin pencarian.

> **Status:** pilot project. Data SPBU yang ada saat ini adalah data tiruan;
> data wilayah administratifnya asli.

---

## Untuk apa project ini

Pertanyaan yang ingin dijawab berubah bentuk seiring tahapannya:

| Tahap | Pertanyaan yang bisa dijawab | Status |
|---|---|---|
| **1 — Pencarian** | *"SPBU mana yang namanya Sudirman?"* — pencarian teks bebas atas nomor, nama, alamat, dan wilayah, lengkap dengan toleransi salah ketik, autocomplete, penyaringan, penyaringan jarak, dan peringkat relevansi | Sudah berjalan |
| **2 — Analitik** | *"SPBU mana di Jawa Barat yang stok Pertalite-nya di bawah 20% dan konsumsinya sedang naik?"* — agregasi stok, penjualan, pasokan, dan distribusi | Direncanakan |
| **3 — AI** | Pertanyaan dalam bahasa sehari-hari, dijawab dengan wawasan dari hasil pencarian dan data rantai pasok | Direncanakan |

**SPBU adalah objek utamanya.** Data BBM bukan entitas yang berdiri sendiri —
ia atribut yang membuat SPBU dapat disaring. Itu yang membedakan project ini
dari sistem ERP rantai pasok pada umumnya.

---

## Teknologi

| Lapis | Teknologi |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| Basis data | SQL Server 2022 + Entity Framework Core 10 |
| Pencarian | **Elasticsearch 9** + Kibana |
| Pembacaan gambar | Tesseract 5 (OCR, `ind+eng`) |
| Pola aplikasi | Clean Architecture, CQRS (MediatR), Result Pattern |
| Autentikasi | ASP.NET Identity + JWT + Refresh Token |
| Otorisasi | RBAC dinamis — `User → Role → Permission → Module` |
| Validasi | FluentValidation via MediatR pipeline |
| Pemetaan objek | Mapster |
| Latar belakang | Hangfire (SQL Server storage) |
| Log & telemetri | Serilog → Seq, OpenTelemetry |
| Dokumentasi API | OpenAPI + Scalar |
| Pengujian | xUnit, FluentAssertions, Moq |

---

## Arsitektur

```
                Presentation  ──►  Application  ──►  Domain
                                        ▲
                                        │
                                 Infrastructure
```

```
src
├── Core
│   ├── SearchEngine.Domain              entitas & aturan bisnis, tanpa dependensi
│   └── SearchEngine.Application         use case, CQRS, kontrak antarmuka
├── Infrastructure
│   ├── SearchEngine.Infrastructure.Identity      Identity, JWT, RBAC, seeder
│   ├── SearchEngine.Infrastructure.Persistence   EF Core, migration, seeder data
│   ├── SearchEngine.Infrastructure.Search        Elasticsearch — indexing & pencarian
│   ├── SearchEngine.Infrastructure.Ocr           Tesseract — pembacaan tulisan pada gambar
│   └── SearchEngine.Infrastructure.Shared        email, penyimpanan berkas
└── Presentation
    └── SearchEngine.WebAPI              controller, middleware, startup
```

### Pembagian peran SQL Server dan Elasticsearch

Ini keputusan arsitektur terpenting dalam project ini:

```
SQL Server                        Elasticsearch
─────────────                     ─────────────
sumber kebenaran                  index turunan
menjaga integritas relasi         melayani pencarian
ternormalisasi                    didenormalisasi, datar
                    ──indexing──►
```

Satu SPBU menjadi **satu dokumen** yang sudah memuat segalanya: rantai
`SPBU → Kota → Provinsi → Regional` diratakan menjadi field biasa, produk dan
fasilitas menjadi larik. Akibatnya pencarian tidak pernah melakukan *join* dan
tidak pernah menyentuh SQL Server sama sekali.

Index di Elasticsearch boleh dibangun ulang kapan saja tanpa kehilangan apa pun.

---

## Model data

```
Regional (8)                         ProdukBbm (7)        Fasilitas (11)
   │                                      ▲                     ▲
   ▼                                      │                     │
Wilayah [Provinsi] (38)              SpbuProduk           SpbuFasilitas
   │                                      ▲                     ▲
   ▼                                      │                     │
Wilayah [Kota/Kabupaten] (514) ──────►  SPBU  ─────────────────┘
```

- **Regional** — 8 wilayah pemasaran Pertamina. Nomornya sekaligus **digit
  pertama kode SPBU**, sehingga konsistensi kode dapat divalidasi silang
  terhadap lokasi administratifnya.
- **Wilayah** — satu tabel yang mereferensikan dirinya sendiri, menampung
  Provinsi → Kota/Kabupaten.
- **SPBU** — kode (`34.12708`), nama, alamat, koordinat, tipe kepemilikan
  (COCO/CODO/DODO), status, jumlah dispenser & nozzle, serta rating dan
  jumlah ulasan.
- **ProdukBbm** — Pertalite, Pertamax, Pertamax Green 95, Pertamax Turbo,
  Biosolar, Dexlite, Pertamina Dex.

Regional sebuah SPBU **selalu diturunkan lewat provinsinya**, tidak pernah
disimpan langsung pada SPBU — supaya mustahil ada SPBU beralamat di Jawa
tetapi bertanda Sulawesi.

Rating dibiarkan **boleh kosong**, dan itu berbeda artinya dari nol: kosong
berarti belum ada yang menilai, sedangkan nol berarti dinilai buruk oleh semua
orang. Jumlah ulasan disimpan terpisah karena rata-rata tanpa jumlah
menyesatkan — 5,0 dari satu ulasan tidak sebanding dengan 4,5 dari tiga ratus.

Seluruh entitas mewarisi jejak audit dan *soft delete*.

---

## Kemampuan pencarian

### Pencarian teks

Satu kotak pencarian menelusuri nomor, nama, alamat, wilayah, produk, dan
fasilitas sekaligus — seluruhnya disalin ke satu field gabungan saat indexing.

- **Toleransi salah ketik** — "sudriman" tetap menemukan "Sudirman".
- **Sinonim alamat Indonesia** — `jl` dikenali sebagai `jalan`, `kec` sebagai
  `kecamatan`, `solar` sebagai `biosolar`, dan seterusnya.
- **Pencocokan awalan** — menekan Enter di tengah kata ("jend sud") tetap
  mengembalikan hasil, bukan daftar kosong.
- **Peringkat relevansi** dengan bobot berbeda per field: kode SPBU paling
  menentukan, lalu nama, lalu alamat.
- **Penyorotan** bagian yang cocok, dibungkus `<mark>`.

### Penyaringan

Seluruh penyaring berbentuk larik sehingga mendukung pemilihan berganda, dan
nilainya sama persis dengan yang dikembalikan facet — klien cukup meneruskan
apa yang diklik pengguna tanpa penerjemahan apa pun.

| Penyaring | Contoh nilai |
|---|---|
| `regional` | `JBB` |
| `provinsi` · `kota` | `Jawa Barat` · `Kota Semarang` |
| `produk` | `PERTALITE` |
| `fasilitas` | `ATM` |
| `status` | `Aktif` |
| `tipeKepemilikan` | `Dodo` |
| `ratingMin` · `ulasanMin` | `4` · `50` |

### Penyaringan jarak

Dua bentuk yang boleh dipakai bersamaan dan saling mempersempit:

- **Radius** — `lat`, `lon`, `radiusKm`. Berbentuk lingkaran.
- **Kotak peta** — `latMin`, `lonMin`, `latMax`, `lonMax`. Mengikuti bentuk
  layar, untuk "cari di area peta ini". Keempatnya wajib diisi bersamaan.

Jarak tiap hasil ikut dikembalikan sebagai `jarakKm`, dihitung di sisi
aplikasi supaya tetap tersedia meski hasil tidak diurutkan menurut jarak.

### Facet

Hitungan per nilai untuk panel penyaring — regional, provinsi, kota, produk,
fasilitas, status, dan tipe kepemilikan. Dapat dimatikan lewat `includeFacets`
bila klien hanya berpindah halaman.

### Pengurutan

`relevansi` (bawaan bila ada kata kunci), `nama`, `kode`, `nozzle`, `rating`,
dan `jarak`. Rating kosong selalu ditempatkan paling belakang, bukan dianggap
bernilai nol. Dasar pengurutan yang benar-benar dipakai dikembalikan sebagai
`urutan`, siap ditampilkan pada kendali "Urutkan".

### Autocomplete

Saran nama SPBU per awalan kata, sengaja **tanpa** toleransi salah ketik —
saat pengguna baru mengetik sebagian kata, setiap potongan pada dasarnya
memang "salah", dan fuzzy hanya membuat saran melebar ke nama yang tidak
diharapkan. Muatannya dibuat seringkas mungkin karena dipanggil pada hampir
setiap ketukan tombol.

### Pencarian lewat gambar

Foto plang SPBU dibaca dengan OCR, diubah menjadi kata kunci, lalu dicari
lewat **jalur yang sama persis** dengan pencarian biasa.

Penyaringan kata kuncinya bertingkat: bila kode SPBU terbaca, kode itu yang
dipakai — satu-satunya penanda yang benar-benar unik pada sebuah plang. Bila
tidak, diambil kata-kata yang membedakan saja; "PERTAMINA" dan "SPBU" muncul
di semua plang sehingga tidak menyaring apa pun dan dibuang.

Teks mentah hasil pembacaan ikut dikembalikan supaya pengguna dapat menilai
sendiri apakah fotonya terbaca dengan benar, dan memperbaiki kata kuncinya
lewat endpoint pencarian biasa bila perlu.

### Dua mesin, untuk dibandingkan

Parameter `engine` dapat diisi `Sql` untuk menjalankan pencarian apa adanya
dengan `LIKE` di basis data. Ini **bukan** fallback, melainkan pembanding —
untuk memperlihatkan secara jujur apa yang didapat dan apa yang hilang:

| Kemampuan | Elasticsearch | SQL Server |
|---|---|---|
| Toleransi salah ketik | ✅ | — |
| Sinonim alamat | ✅ | — |
| Peringkat relevansi | ✅ | — |
| Penyorotan | ✅ | — |
| Facet | ✅ | — |
| Penyaringan jarak | ✅ | — |
| Penyaringan wilayah, produk, fasilitas | ✅ | ✅ |

Yang tidak didukung dilaporkan lewat properti `kemampuan` dan `catatan` pada
setiap tanggapan, sehingga klien dapat membedakan *"mesin ini tidak mampu"*
dari *"memang tidak ada hasil"* — tampilan tidak pernah diam-diam kosong
tanpa penjelasan.

Endpoint pembanding menjalankan kata kunci yang sama pada kedua mesin lalu
menyandingkan waktu dan jumlah hasilnya. Pengukurannya dijaga setara: jam
dinding di sisi aplikasi untuk kedua mesin, satu eksekusi pemanasan dibuang,
yang dilaporkan median bukan rata-rata, dan facet dimatikan di kedua sisi
karena hanya satu mesin yang mampu menghasilkannya.

---

## API pencarian

| Endpoint | Keterangan |
|---|---|
| `GET /api/search/spbu` | Pencarian utama — teks, penyaring, facet, jarak, paginasi |
| `GET /api/search/spbu/suggestion` | Saran ketik-langsung (`q`, `limit`) |
| `POST /api/search/spbu/image` | Pencarian dari foto (PNG/JPEG, maks. 10 MB) |
| `GET /api/search/spbu/benchmark` | Pembandingan Elasticsearch vs SQL Server |
| `POST /api/search/spbu/reindex` | Mengantrikan indexing ulang penuh |

Seluruhnya memerlukan autentikasi. Pencarian menuntut permission
`search.view`; pembandingan dan indexing ulang menuntut `search.execute`.

Paginasi dalam dibatasi pada 10.000 hasil (`pageNumber × pageSize`), sama
seperti mesin pencari pada umumnya yang tidak mengizinkan menelusuri lewat
halaman ke-100. Untuk menjangkau hasil yang jauh, persempit kata kunci atau
tambahkan penyaring — bukan menelusuri ribuan halaman.

---

## Menjalankan secara lokal

### Prasyarat

- .NET 10 SDK
- **SQL Server** terpasang di mesin (bukan di container)
- Docker Desktop — untuk Elasticsearch, Kibana, dan Seq

### 1. Nyalakan infrastruktur

```bash
cp .env.example .env     # isi ELASTIC_PASSWORD, KIBANA_SYSTEM_PASSWORD,
                         # ELASTIC_APP_PASSWORD, KIBANA_ENCRYPTION_KEY (min. 32 karakter)
docker compose up -d
```

Menyalakan Elasticsearch, Kibana, dan Seq. Container `elasticsearch-setup`
berjalan sekali lalu keluar dengan kode 0 — itu memang perilaku yang benar:
tugasnya menyetel password `kibana_system` dan membuat user aplikasi yang
aksesnya dibatasi pada index berawalan `searchengine-`.

### 2. Jalankan aplikasi

```bash
dotnet run --project src/Presentation/SearchEngine.WebAPI
```

Skema basis data dan data referensi (regional, wilayah, produk, fasilitas)
ditanam otomatis saat start dan bersifat idempoten.

### 3. Isi data SPBU tiruan

```bash
dotnet run --project src/Presentation/SearchEngine.WebAPI -- --seed-demo
dotnet run --project src/Presentation/SearchEngine.WebAPI -- --seed-demo --count 25000
```

Menghasilkan 10.000 SPBU yang sebarannya dibobot jumlah penduduk tiap
kota/kabupaten, sehingga mengikuti kenyataan: Jawa padat, Papua jarang.
Prosesnya deterministik — dijalankan berapa kali pun hasilnya identik.

Perintah ini **tidak pernah** ikut start biasa. Puluhan ribu baris tiruan tidak
boleh masuk basis data hanya karena aplikasi dijalankan.

### 4. Bangun index pencarian

```bash
POST /api/search/spbu/reindex
```

Berjalan di latar belakang lewat Hangfire dan juga terjadwal otomatis setiap
hari pukul 02.00 UTC.

### 5. Siapkan pembacaan gambar

Pencarian lewat gambar memakai berkas data bahasa Tesseract di
`src/Presentation/SearchEngine.WebAPI/tessdata/` — `ind.traineddata` dan
`eng.traineddata`. Keduanya sudah tersedia di repositori ini; penggantinya
dapat diunduh dari
[tesseract-ocr/tessdata](https://github.com/tesseract-ocr/tessdata).

Untuk foto plang di lapangan — sering miring, silau, atau kurang cahaya —
varian `tessdata_best` biasanya sepadan dengan tambahan waktunya. Rinciannya
ada pada [README folder tersebut](src/Presentation/SearchEngine.WebAPI/tessdata/README.md).

Bila berkasnya tidak ada, aplikasi tetap berjalan normal: hanya endpoint
pencarian lewat gambar yang membalas bahwa layanan OCR belum siap. Seluruh
fitur pencarian lain tidak bergantung padanya.

---

## Alamat penting

| URL | Keterangan |
|---|---|
| `http://localhost:5152/scalar/docs` | Dokumentasi API |
| `http://localhost:5152/healthcheck-ui` | Dasbor status layanan |
| `http://localhost:5152/hangfire` | Dasbor pekerjaan latar belakang |
| `http://localhost:5601` | Kibana |
| `http://localhost:9200` | Elasticsearch |
| `http://localhost:5341` | Seq (log aplikasi) |

Akun bawaan: `admin@searchengine.local` / `Admin123!` (role SuperAdmin).

---

## Indexing ke Elasticsearch

Menggunakan **pola alias** — aplikasi tidak pernah menyebut index fisik:

```
searchengine-spbu                          ← alias yang dipakai mencari
       └──► searchengine-spbu-20260912-013045    ← index fisik
```

Setiap indexing ulang membangun index baru dari nol, lalu memindahkan alias
dalam satu operasi atomik dan menghapus versi lama. Manfaatnya:

- pencarian tidak pernah melihat index yang belum selesai dibangun
- tidak ada jeda tanpa layanan
- salah mapping cukup dibatalkan dengan menukar balik alias
- **alias hanya berpindah bila seluruh dokumen diterima** — bila ada yang
  ditolak, index lama tetap melayani

Id dokumen memakai primary key SQL, sehingga indexing bersifat idempoten:
menulis ulang menimpa, bukan menggandakan.

Bentuk index — analyzer, sinonim alamat, dan pemetaan tiap field — disimpan
sebagai satu berkas JSON yang disematkan ke dalam assembly. Berkas itulah
sumber kebenarannya, dan bentuknya sama persis dengan yang dapat diuji
langsung di Kibana Dev Tools.

Perubahan yang dilakukan langsung lewat SQL tidak akan pernah memberi tahu
Elasticsearch — itulah sebabnya indexing ulang terjadwal tetap diperlukan.

---

## Pengujian

```bash
dotnet test SearchEngine.slnx
```

Integration test menyalakan aplikasi yang sesungguhnya, sehingga memerlukan
SQL Server dan Elasticsearch dalam keadaan hidup.

---

## Sumber data

Data wilayah administratif berasal dari
[cahyadsn/wilayah](https://github.com/cahyadsn/wilayah) (MIT), yang memirror
Kepmendagri No. 300.2.2-2138 Tahun 2025 — 38 provinsi dan 514 kota/kabupaten
beserta koordinat dan jumlah penduduk.

> **Koreksi terhadap data sumber:** baris `74.07` (Kabupaten Wakatobi) pada
> sumber aslinya memuat `lng = 23.538901`, kehilangan angka 1 di depan,
> sehingga titiknya jatuh di Samudra Atlantik. Nilai pada salinan project ini
> sudah dibetulkan menjadi `123.538901`, dan pemuat data kini menolak seluruh
> berkas bila ada koordinat di luar batas Indonesia.

Data SPBU seluruhnya tiruan dan tidak merujuk SPBU nyata mana pun.

---

## Lisensi

Lihat [LICENSE.md](LICENSE.md).
