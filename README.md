
# WeatherStack Projesi

WeatherStack Projesi, belirli şehirlerin hava durumu bilgilerini sağlayan, Redis önbellekleme ve MySQL veritabanı ile entegre çalışan bir API projesidir. Bu proje, yüksek performanslı ve kolay izlenebilir bir hava durumu uygulaması geliştirmeyi amaçlamaktadır.

## İçindekiler

- [Kullanılan Teknolojiler](#kullanılan-teknolojiler)
- [Kurulum Adımları](#kurulum-adımları)
- [Mimari ve Tasarım](#mimari-ve-tasarım)
- [API Uç Noktaları](#api-uç-noktaları)
- [Önbellekleme ve Veritabanı](#önbellekleme-ve-veritabanı)
- [Loglama](#loglama)
- [Hata Yönetimi](#hata-yönetimi)
- [Testler](#testler)
- [Örnek Kullanım](#örnek-kullanım)

## Kullanılan Teknolojiler

- **.NET 8**: API geliştirme
- **MySQL**: Kalıcı veri depolama
- **Redis**: Önbellekleme
- **Serilog**: Loglama
- **Dapper**: MySQL ile veri etkileşimi
- **Docker Compose**: Redis ve MySQL için konteyner yönetimi
- **HTTP Client**: Hava durumu API çağrıları
- **Moq** ve **xUnit**: Unit testler için kullanılan frameworkler

## Kurulum Adımları

### 1. Projeyi Klonlayın

```bash
git clone https://github.com/defneaynur/WeatherStack.Solution.git
cd weatherstack-solution
```

### 2. Gerekli Bağımlılıkları Yükleyin

```bash
dotnet restore
```

### 3. Veritabanı ve Redis Kurulumu

Docker ile MySQL ve Redis konteynerlerini başlatın:

```bash
docker-compose -f docker-compose-moonlight.yml up -d
```

## Mimari ve Tasarım

- **API Controllerları**: Gelen HTTP isteklerini alır ve ilgili işlem katmanına yönlendirir.
- **İşlem Katmanı (Processor)**: Veritabanı ve önbellek kontrolleri ile API çağrılarını yönetir.
- **Önbellek İşlemcisi**: Redis ile önbellekleme işlemlerini gerçekleştirir.
- **Veritabanı Katmanı**: Dapper ile MySQL veritabanında işlem yapar.
- **Hata Yönetimi**: `CoreNotificationException` gibi özel hatalar iş kurallarını yönetir.

## API Uç Noktaları

### Weather Service API

- **POST** `/api/WeatherService`  
  Belirtilen şehir için hava durumu bilgisini getirir. Önbellek ve veritabanında sorgulama yaparak gereksiz API çağrılarını önler.

Örnek İstek:

```json
{
    "q": "Istanbul",
    "days": 1,
    "aqi": "no",
    "alerts": "no"
}
```

Örnek Yanıt:

```json
{
    "data": {
        "city": "Istanbul",
        "temperature": 15.4,
        "condition": "Cloudy"
    },
    "responseCode": "Success",
    "message": ""
}
```

### Favorite City Service API

- **GET** `/api/FavoriteCityService`  
  Veritabanındaki favori şehirleri listeler. Soft delete (silinmemiş) ve 15 dakikadan eski olmayan şehirler filtrelenir.

- **POST** `/api/FavoriteCityService`  
  Favori şehirler listesine yeni bir şehir ekler. Şehir zaten varsa hata verir.

- **PUT** `/api/FavoriteCityService/delete/{id}`  
  Favori şehir listesinden belirtilen ID’ye sahip şehri soft delete ile işaretler.

## Önbellekleme ve Veritabanı

### Redis Önbellekleme

Önbellek kontrolü, `WeatherApiProcessor` sınıfında yapılmaktadır. Bu, API çağrı yükünü azaltmak için kullanılır.

#### Kullanılan Yöntemler

- **CacheWeatherDataAsync**: Hava durumu bilgisini Redis’e kaydeder.
- **GetCachedWeatherDataAsync**: Redis’te veri varsa getirir.

### MySQL Veritabanı

Dapper kullanılarak favori şehirlerin kalıcı olarak saklandığı MySQL veritabanı kullanılır. Her işlem Serilog ile loglanır.

#### Kullanılan Yöntemler

- **GetFavoriteCityAsync**: Veritabanında belirli bir şehir olup olmadığını kontrol eder.
- **CreateFavoriteCitiesAsync**: Eğer yoksa favori şehirler listesine ekler.
- **DeleteFavoriteCitiesAsync**: Belirtilen ID’li şehri silinmiş olarak işaretler.

## Loglama

Proje, **Serilog** kullanılarak loglanmaktadır. Log dosya yolu, `appsettings.json` dosyasından okunur. Önemli bilgiler ve hata mesajları loglanarak sistemin kolay izlenebilir olması sağlanır.

## Hata Yönetimi

Proje, `CoreNotificationException` gibi özel istisnalar kullanarak iş kurallarıyla ilgili hataları yönetir. Örneğin, eğer şehir zaten favorilerde mevcutsa bir hata döner.

## Testler

Proje, `xUnit` ve `Moq` kullanılarak test edilmiştir. Aşağıda birkaç test açıklaması bulunmaktadır:

- **GetWeather_ShouldReturnSuccess_WhenWeatherDataExists**: Hava durumu verisi mevcut olduğunda başarıyla dönülmesi beklenir.
- **GetWeather_ShouldReturnNoData_WhenWeatherDataIsNull**: Hava durumu verisi bulunamadığında uygun mesaj ile dönülmesi beklenir.
- **CreateFavoriteCities_ShouldReturnSuccess_WhenCityIsAdded**: Şehir başarıyla eklendiğinde doğrulama yapılır.
- **CreateFavoriteCities_ShouldReturnInfo_WhenCityAlreadyExists**: Zaten mevcut olan bir şehir eklenmeye çalışıldığında hata mesajı dönülmesi beklenir.

## Örnek Kullanım

1. **Favori Şehir Ekleme**

   ```http
   POST /api/FavoriteCityService
   Content-Type: application/json
   {
       "CityName": "Istanbul",
       "Temperature": 20.5,
       "Condition": "Clear",
       "LastUpdated": "2023-11-01T10:00:00Z"
   }
   ```

2. **Belirli Bir Şehir için Hava Durumu Alma**

   ```http
   POST /api/WeatherService
   Content-Type: application/json
   {
       "q": "Ankara",
       "days": 1,
       "aqi": "no",
       "alerts": "no"
   }
   ```
