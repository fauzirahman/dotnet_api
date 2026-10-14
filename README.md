# Market

Market adalah aplikasi web dan REST API untuk operasional toko kebutuhan sehari-hari. Proyek ini menggabungkan katalog produk, pengelolaan stok, checkout COD, pemrosesan pesanan, autentikasi berbasis role, dan laporan dalam satu aplikasi ASP.NET Core.

## Ringkasan Proyek

- **Jenis:** Web app dan REST API
- **Backend:** ASP.NET Core 8, C#
- **Database:** PostgreSQL melalui Entity Framework Core dan Npgsql
- **Antarmuka:** Razor Views, JavaScript, dan CSS responsif
- **Autentikasi:** Cookie HTTP-only, role-based authorization, password hash PBKDF2

## Fitur

### Pembeli

- Melihat katalog, harga, kategori, dan stok produk.
- Membuat akun dan masuk ke aplikasi.
- Membuat pesanan dengan metode pembayaran COD.
- Melihat riwayat pesanan dan membatalkan pesanan yang masih berstatus `Pending`.

### Kasir

- Melihat antrean pesanan.
- Memproses status `Pending` → `Confirmed` → `Shipped` → `Delivered`.
- Mencatat pembayaran COD setelah pesanan terkirim.
- Melihat laporan stok, penjualan, dan status pesanan.

### Admin

- Mengelola produk, harga, stok, dan kategori.
- Membuat akun kasir.
- Mengakses seluruh fungsi kasir dan laporan operasional.
- Membuat akun administrator pertama melalui proses bootstrap yang dilindungi secret.

## Alur Bisnis

1. Pembeli memilih produk yang tersedia dan mengirim jumlah barang yang dipesan.
2. Server memvalidasi harga dan stok dari database, menghitung total, menyimpan snapshot harga pesanan, lalu mengurangi stok dalam transaksi database.
3. Kasir mengonfirmasi dan mengirim pesanan. Pembayaran COD baru dapat dicatat setelah pesanan berstatus `Delivered`.
4. Laporan penjualan menghitung pesanan berstatus lunas dan mengecualikan pesanan yang dibatalkan.
5. Pembatalan pesanan yang diizinkan mengembalikan stok. Produk dalam riwayat pesanan dan kategori yang masih memiliki produk tidak dapat dihapus.

## Menjalankan Secara Lokal

### Prasyarat

- .NET SDK 8
- PostgreSQL yang dapat diakses dari mesin pengembangan

### Konfigurasi

Simpan connection string melalui User Secrets, bukan di file konfigurasi yang dilacak Git:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=marketdb;Username=postgres;Password=PASSWORD_ANDA"
dotnet user-secrets set "Bootstrap:AdminKey" "GANTI_DENGAN_SECRET_PANJANG"
```

Ganti connection string dan bootstrap key dengan nilai milik lingkungan lokal. Di lingkungan selain Development, sediakan konfigurasi yang sama melalui environment variable `ConnectionStrings__DefaultConnection` dan `Bootstrap__AdminKey`.

### Jalankan

```bash
dotnet restore
dotnet run
```

Profil `Market` membuka HTTPS di `https://localhost:5001` dan HTTP di `http://localhost:5000`. Database harus dapat diakses saat aplikasi mulai karena migrasi EF Core diterapkan pada startup. Jika sertifikat HTTPS pengembangan belum dipercaya, gunakan HTTP lokal atau percayai sertifikat development .NET.

### Buat Admin Pertama

1. Atur `Bootstrap:AdminKey` seperti pada konfigurasi di atas.
2. Buka halaman Market dan pilih **Set up the first administrator**.
3. Masukkan email, password, dan bootstrap key.
4. Setelah admin dibuat, endpoint bootstrap menolak pembuatan admin kedua. Admin dapat membuat akun kasir dari endpoint yang tersedia.

Pembeli dapat mendaftar melalui pilihan **Create account** pada halaman utama.

## Endpoint Utama

Semua body request dan response menggunakan JSON. Endpoint yang memerlukan sesi menggunakan cookie autentikasi.

| Metode | Endpoint | Akses | Keterangan |
| --- | --- | --- | --- |
| `GET` | `/api/products` | Publik | Daftar produk |
| `GET` | `/api/products/{id}` | Publik | Detail produk |
| `POST` | `/api/products` | Admin | Membuat produk |
| `PUT` | `/api/products/{id}` | Admin | Memperbarui produk, harga, dan stok |
| `DELETE` | `/api/products/{id}` | Admin | Menghapus produk yang tidak terikat riwayat pesanan |
| `GET` | `/api/categories` | Publik | Daftar kategori |
| `POST`, `PUT`, `DELETE` | `/api/categories` | Admin | Mengelola kategori |
| `POST` | `/api/auth/register` | Publik | Registrasi pembeli |
| `POST` | `/api/auth/login` | Publik | Login |
| `POST` | `/api/auth/logout` | Login | Logout |
| `GET` | `/api/auth/me` | Login | Identitas sesi aktif |
| `POST` | `/api/auth/bootstrap-admin` | Bootstrap key | Membuat admin pertama |
| `POST` | `/api/auth/cashiers` | Admin | Membuat akun kasir |
| `POST`, `GET` | `/api/orders` | Pembeli / staf | Membuat pesanan atau melihat daftar pesanan sesuai role |
| `GET` | `/api/orders/{id}` | Login | Detail pesanan; pembeli hanya melihat pesanan miliknya |
| `PUT` | `/api/orders/{id}/status` | Admin / kasir | Memperbarui status sesuai transisi yang diizinkan |
| `PUT` | `/api/orders/{id}/payment` | Admin / kasir | Mencatat pembayaran setelah pengantaran |
| `POST` | `/api/orders/{id}/cancel` | Pemilik pesanan | Membatalkan pesanan yang masih `Pending` |
| `GET` | `/api/reports/stock` | Admin / kasir | Posisi stok; menerima parameter opsional `threshold` |
| `GET` | `/api/reports/sales` | Admin / kasir | Ringkasan penjualan berdasarkan `fromDate` dan `toDate` |
| `GET` | `/api/reports/orders` | Admin / kasir | Daftar dan jumlah pesanan per status berdasarkan rentang tanggal |

Format tanggal laporan adalah `YYYY-MM-DD`. Tanpa filter tanggal, laporan menggunakan 30 hari terakhir dalam UTC.

## Validasi

```bash
dotnet build
dotnet list package --vulnerable --include-transitive
node --check wwwroot/js/market.js
```

## Struktur Utama

```text
Controllers/       Endpoint API dan halaman web
Domain/            Kontrak service, repository, dan response
Mapping/           Pemetaan model dan resource API
Migrations/        Migrasi skema Entity Framework Core
Models/            Entitas kategori, produk, pengguna, dan pesanan
Persistence/       DbContext dan repository
Resources/         DTO request, response, dan laporan
Services/          Aturan bisnis autentikasi, katalog, pesanan, dan laporan
Views/             Halaman Razor
wwwroot/           CSS dan JavaScript antarmuka
```

## Batasan Versi Ini

- Pembayaran hanya mendukung pencatatan manual COD; belum terhubung ke payment gateway.
- Pengiriman dikelola melalui perubahan status pesanan; integrasi kurir belum tersedia.
- Registrasi publik hanya membuat akun pembeli. Akun admin pertama dibuat melalui bootstrap, sedangkan akun kasir dibuat oleh admin.