// Integration test berjalan terhadap database (Identity) yang sama dan
// berbagi satu akun "admin@searchengine.local". Menjalankan test class secara
// paralel membuat beberapa request menulis baris admin yang sama secara
// bersamaan (mis. reset AccessFailedCount saat login sukses vs. increment saat
// login gagal), sehingga terjadi konflik optimistic-concurrency yang membuat
// login valid sesekali balas 401 "Invalid credentials".
//
// Menonaktifkan paralelisasi membuat akses ke database bersama menjadi
// deterministik tanpa mengubah logika aplikasi.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
