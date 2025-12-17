# IEEE Web Site Backend

IEEE web sitesi için ASP.NET Core Web API backend projesi.

## 🚀 Teknolojiler

- **.NET 8.0**
- **ASP.NET Core Web API**
- **Entity Framework Core 8.0**
- **PostgreSQL** (Npgsql)
- **Swagger/OpenAPI**

## 📋 Özellikler

### Event Management
- Event CRUD işlemleri
- Komite bazlı event filtreleme
- Önemli eventler
- Yaklaşan eventler (1 hafta içinde)
- Event fotoğraf yönetimi (kapak + galeri)

### Blog Post Management
- Blog post CRUD işlemleri
- Komite bazlı blog post filtreleme
- Son 8 blog post

### Committee Management
- Komite CRUD işlemleri

## 📁 Proje Yapısı

```
IEEEBackend/
├── Controllers/          # API Controllers
├── Data/                 # DbContext
├── Dtos/                 # Data Transfer Objects
├── Interfaces/           # Repository Interfaces
├── Mappers/              # Entity-DTO Mappers
├── Models/               # Domain Models
├── Repositories/         # Repository Implementations
└── Migrations/           # EF Core Migrations
```

## 🛠️ Kurulum

### Gereksinimler
- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio 2022 veya VS Code

### Adımlar

1. **Repository'yi klonlayın**
```bash
git clone https://github.com/IEEE-CSLAB/ieee-web-site-backend.git
cd ieee-web-site-backend
```

2. **appsettings.json'ı yapılandırın**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=IEEEBackendDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

3. **Migration'ları çalıştırın**
```bash
dotnet ef database update
```

4. **Projeyi çalıştırın**
```bash
dotnet run
```

5. **Swagger UI'ya erişin**
```
https://localhost:5001/swagger
```

## 📡 API Endpoints

### Events
- `GET /api/events` - Tüm eventler
- `GET /api/events/{id}` - Event detayı
- `GET /api/events/committee/{committeeId}` - Komiteye göre eventler
- `GET /api/events/important` - Önemli eventler
- `GET /api/events/upcoming` - Yaklaşan eventler
- `POST /api/events` - Yeni event oluştur
- `PUT /api/events/{id}` - Event güncelle
- `DELETE /api/events/{id}` - Event sil
- `POST /api/events/{eventId}/cover` - Kapak fotoğrafı yükle
- `GET /api/events/{eventId}/cover` - Kapak fotoğrafını getir
- `POST /api/events/{eventId}/photos` - Etkinlik fotoğrafları yükle
- `GET /api/events/{eventId}/photos` - Etkinlik fotoğraflarını getir
- `DELETE /api/events/{eventId}/photos/{photoId}` - Fotoğraf sil

### Blog Posts
- `GET /api/blogposts` - Tüm blog postlar
- `GET /api/blogposts/{id}` - Blog post detayı
- `GET /api/blogposts/committee/{committeeId}` - Komiteye göre blog postlar
- `GET /api/blogposts/last8` - Son 8 blog post
- `POST /api/blogposts` - Yeni blog post oluştur
- `PUT /api/blogposts/{id}` - Blog post güncelle
- `DELETE /api/blogposts/{id}` - Blog post sil

### Committees
- `GET /api/committees` - Tüm komiteler
- `GET /api/committees/{id}` - Komite detayı
- `POST /api/committees` - Yeni komite oluştur
- `PUT /api/committees/{id}` - Komite güncelle
- `DELETE /api/committees/{id}` - Komite sil

## 📝 Dosya Yükleme

Event fotoğrafları `wwwroot/uploads/events/` klasörüne kaydedilir.

**Konfigürasyon (appsettings.json):**
```json
{
  "FileUpload": {
    "UploadPath": "wwwroot/uploads/events",
    "MaxFileSize": 5242880,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"]
  }
}
```

## 👥 Katkıda Bulunanlar

- İbrahim Kiraz (@ibrahimkiraz1)
- Beda (@beda03)

## 📄 Lisans

Bu proje IEEE CSLAB organizasyonu altındadır.

