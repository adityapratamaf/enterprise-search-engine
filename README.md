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
| **1 — Pencarian** | *"SPBU mana yang namanya Sudirman?"* — pencarian teks bebas atas nomor, nama, alamat, dan wilayah, lengkap dengan toleransi salah ketik, autocomplete, penyaringan, dan peringkat relevansi | Sedang dikerjakan |
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
  (COCO/CODO/DODO), status, jumlah dispenser & nozzle.
- **ProdukBbm** — Pertalite, Pertamax, Pertamax Green 95, Pertamax Turbo,
  Biosolar, Dexlite, Pertamina Dex.

Regional sebuah SPBU **selalu diturunkan lewat provinsinya**, tidak pernah
disimpan langsung pada SPBU — supaya mustahil ada SPBU beralamat di Jawa
tetapi bertanda Sulawesi.

Seluruh entitas mewarisi jejak audit dan *soft delete*.

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
