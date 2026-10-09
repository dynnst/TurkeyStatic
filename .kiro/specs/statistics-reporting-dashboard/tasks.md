# İstatistik Raporlama ve Görselleştirme Sayfası - Görevler

## Genel Bakış

Bu görev listesi, İstatistik Raporlama ve Görselleştirme özelliğinin eksiksiz implementasyonunu tanımlar. Sistem, PassportController ve PassportService desenine uygun olarak geliştirilecek ve kullanıcıların rol ve büro yetkilerine göre istatistikleri grafik ve tablo formatında görüntülemesini, zaman aralığı filtreleme yapmasını ve dönemsel karşılaştırmalar gerçekleştirmesini sağlayacaktır.

## Görevler

### 1. NuGet Paketlerini Yükle

- [x] 1.1 EPPlus kütüphanesini projeye ekle
  - Visual Studio'da Solution Explorer'da projeye sağ tıkla ve "Manage NuGet Packages" seç
  - "Browse" sekmesinden "EPPlus" ara (versiyon 6.x veya 7.x)
  - EPPlus paketini yükle
  - `packages.config` dosyasında paketin eklendiğini doğrula
  - _Gereksinimler: 4.9_

### 2. Backend: DTO Modellerini Oluştur

- [x] 2.1 DTO sınıflarını Models klasörüne ekle
  - `Models/ReportModels.cs` dosyası oluştur
  - `PeriodType` enum tanımla (Daily, Weekly, Monthly, Yearly)
  - `AggregatedDataPoint` sınıfını tanımla (Date, Value, Label, Metrics dictionary)
  - `AggregatedDataResult` sınıfını tanımla (DataType, PeriodType, StartDate, EndDate, DataPoints listesi, Summary dictionary)
  - `ComparisonResult` sınıfını tanımla (iki dönem bilgisi ve karşılaştırma metrikleri)
  - `ComparisonMetric` sınıfını tanımla (MetricName, Period1Value, Period2Value, AbsoluteDifference, PercentageChange, Trend)
  - `TrendDirection` enum tanımla (Up, Down, Neutral)
  - `PagedTableResult` sınıfını tanımla (CurrentPage, PageSize, TotalRecords, TotalPages, Rows, Summary)
  - _Gereksinimler: 1.1, 2.1, 3.1, 4.1, 5.1_

### 3. Backend: ReportService Oluştur

- [x] 3.1 Temel ReportService yapısını oluştur
  - `Services/ReportService.cs` dosyası oluştur
  - Constructor'da IstatistikContext ve CurrentUser parametrelerini al
  - Border değerini oturumdan al ve doğrula (null/boş kontrolü)
  - Bureau kodlarını CurrentUser'dan al
  - PassportService desenine uygun olarak UnauthorizedAccessException fırlat
  - _Gereksinimler: 1.1, 1.4, 1.8_

- [x] 3.2 Veri türü ve yetkilendirme metodlarını ekle
  - `ValidateDataTypeAccess(string dataType)` metodu yaz
  - `GetBureauCodeForDataType(string dataType)` metodu yaz
  - Desteklenen veri türleri için mapping oluştur: yolcuucak, gunluk, inad, tahdit, haftalik → PASAPORT bureau
  - `CanAccessBureau` kontrolü ile yetkilendirme yap
  - Yetki yoksa UnauthorizedAccessException fırlat
  - _Gereksinimler: 1.5, 1.6, 6.3_

- [x] 3.3 Günlük toplama metodunu implement et
  - `AggregateByDay` private metodu yaz
  - `DbFunctions.TruncateTime` ile tarihleri grupla
  - Her gün için toplam değer hesapla
  - Border filtresi uygula
  - Tarihe göre sırala
  - _Gereksinimler: 2.2, 2.3, 8.1_

- [x] 3.4 Haftalık toplama metodunu implement et
  - `AggregateByWeek` private metodu yaz
  - `GetWeekStartDate` helper metodu ekle (Pazartesi başlangıçlı ISO 8601 hafta hesaplama)
  - Her hafta için toplam değer hesapla
  - Border filtresi uygula
  - Hafta başlangıç tarihine göre sırala
  - _Gereksinimler: 2.4, 8.1_

- [x] 3.5 Aylık ve yıllık toplama metodlarını implement et
  - `AggregateByMonth` private metodu yaz (Year ve Month'a göre grupla)
  - `AggregateByYear` private metodu yaz (Year'a göre grupla)
  - Her dönem için toplam değer hesapla
  - Border filtresi uygula
  - Tarihe göre sırala
  - _Gereksinimler: 2.5, 2.6, 8.1_

- [x] 3.6 Ana toplama metodunu implement et
  - `GetAggregatedData(string dataType, DateTime startDate, DateTime endDate, PeriodType periodType)` public metodu yaz
  - Tarih validasyonları ekle (bitiş > başlangıç, gelecek tarih yok, maksimum 5 yıl)
  - ValidateDataTypeAccess çağır
  - dataType'a göre uygun tabloyu sorgula (YolcuUcakIstatistikleri, GunlukZamanSerisiYolcular, etc.)
  - periodType'a göre uygun toplama metodunu çağır
  - Summary hesapla (Toplam, Ortalama, Maksimum)
  - AggregatedDataResult döndür
  - _Gereksinimler: 2.1-2.10, 8.1, 8.2, 8.3_

- [x] 3.7 Karşılaştırma metodunu implement et
  - `GetComparisonData` metodu yaz (iki dönem parametreleriyle)
  - Her iki dönem için GetAggregatedData çağır
  - Mutlak fark hesapla (Period2 - Period1)
  - Yüzde değişimi hesapla (Period1 != 0 ise)
  - Trend yönünü belirle (Up/Down/Neutral)
  - ComparisonResult döndür
  - _Gereksinimler: 5.1-5.10_

- [x] 3.8 Sayfalı tablo metodunu implement et
  - `GetPagedTableData` metodu yaz
  - GetAggregatedData'dan veri al
  - Sıralama uygula (sortBy ve sortDesc parametrelerine göre)
  - Sayfalama uygula (Skip ve Take ile)
  - Toplam sayfa sayısını hesapla
  - PagedTableResult döndür
  - _Gereksinimler: 4.1-4.7_

- [x] 3.9 CSV export metodunu implement et
  - `ExportToCsv` metodu yaz
  - AggregatedDataResult'tan CSV formatı oluştur
  - UTF-8 encoding kullan
  - Başlık satırı ekle
  - Veri satırlarını yaz
  - byte[] olarak döndür
  - _Gereksinimler: 4.8_

- [x] 3.10 Excel export metodunu implement et
  - `ExportToExcel` metodu yaz
  - EPPlus kütüphanesini kullan
  - ExcelPackage oluştur
  - Worksheet ekle ve başlıkları yaz
  - Veri satırlarını ekle
  - Hücre formatlaması uygula (tarih, sayı formatları)
  - XLSX formatında byte[] döndür
  - _Gereksinimler: 4.9_

### 4. Backend: ReportsController Oluştur

- [x] 4.1 Temel controller yapısını oluştur
  - `Controllers/ReportsController.cs` dosyası oluştur
  - `[RoleAuthorize(AppRoles.SuperAdmin, AppRoles.UnitAdmin, AppRoles.BureauUser)]` attribute ekle
  - IstatistikContext field tanımla
  - `CreateService()` private metodu yaz (PassportController desenine uygun)
  - `Run()` helper metodu ekle (try-catch ile hata yönetimi)
  - Dispose metodunu override et
  - _Gereksinimler: 1.1, 10.1, 10.2_

- [x] 4.2 Index action metodunu implement et
  - `[HttpGet] Index()` metodu yaz
  - CurrentUser.FromSession ile kullanıcıyı al
  - Session kontrolü yap (null ise login'e redirect)
  - Border bilgisini ViewBag'e ekle
  - View döndür
  - _Gereksinimler: 1.1, 1.2, 9.1, 10.5_

- [x] 4.3 GetChartData action metodunu implement et
  - `[HttpPost, ValidateAntiForgeryToken] GetChartData()` metodu yaz
  - Parametreleri al: dataType, startDate, endDate, periodType
  - Run() helper içinde ReportService.GetAggregatedData çağır
  - JSON response döndür
  - _Gereksinimler: 3.1-3.10, 10.3_

- [x] 4.4 GetTableData action metodunu implement et
  - `[HttpPost, ValidateAntiForgeryToken] GetTableData()` metodu yaz
  - Parametreleri al: dataType, startDate, endDate, periodType, page, pageSize, sortBy, sortDesc
  - Run() helper içinde ReportService.GetPagedTableData çağır
  - JSON response döndür
  - _Gereksinimler: 4.1-4.7, 10.3_

- [x] 4.5 GetComparisonData action metodunu implement et
  - `[HttpPost, ValidateAntiForgeryToken] GetComparisonData()` metodu yaz
  - Parametreleri al: dataType, period1Start, period1End, period2Start, period2End, periodType
  - Run() helper içinde ReportService.GetComparisonData çağır
  - JSON response döndür
  - _Gereksinimler: 5.1-5.10, 10.3_

- [x] 4.6 ExportCsv action metodunu implement et
  - `[HttpPost, ValidateAntiForgeryToken] ExportCsv()` metodu yaz
  - Parametreleri al: dataType, startDate, endDate, periodType
  - ReportService'ten veri al ve CSV'ye çevir
  - FileResult döndür (text/csv, UTF-8)
  - _Gereksinimler: 4.8_

- [x] 4.7 ExportExcel action metodunu implement et
  - `[HttpPost, ValidateAntiForgeryToken] ExportExcel()` metodu yaz
  - Parametreleri al: dataType, startDate, endDate, periodType
  - ReportService'ten veri al ve Excel'e çevir
  - FileResult döndür (application/vnd.openxmlformats-officedocument.spreadsheetml.sheet)
  - _Gereksinimler: 4.9_

### 5. Checkpoint - Backend testleri

- [x] 5.1 Backend derlemesini doğrula
  - Visual Studio'da Solution'ı build et
  - Derleme hatası olmadığından emin ol
  - Varsa hataları düzelt

### 6. Frontend: Ana View Oluştur

- [x] 6.1 Reports view klasörünü ve Index.cshtml'i oluştur
  - `Views/Reports/` klasörü oluştur
  - `Views/Reports/Index.cshtml` dosyası oluştur
  - ViewBag.Title ve Layout ayarla
  - Anti-forgery token ekle: `@Html.AntiForgeryToken()`
  - Bootstrap 5 container-fluid yapısı oluştur
  - _Gereksinimler: 9.3_

- [x] 6.2 Başlık ve havalimanı bilgisini ekle
  - Sayfa başlığı ekle: "İstatistik Raporları"
  - ViewBag.Border'ı göster
  - _Gereksinimler: 1.1_

- [x] 6.3 Filtre panelini oluştur
  - Filtre card'ı ekle
  - Veri türü seçimi için select dropdown ekle (yolcuucak, gunluk, inad, tahdit, haftalik)
  - Başlangıç tarihi için date input ekle
  - Bitiş tarihi için date input ekle
  - Dönem tipi için select dropdown ekle (Günlük, Haftalık, Aylık, Yıllık)
  - "Filtrele" butonu ekle
  - _Gereksinimler: 2.1, 2.7, 6.2_

- [x] 6.4 Sekmeli yapıyı oluştur
  - Bootstrap 5 nav-tabs yapısı ekle
  - Üç sekme ekle: Grafik, Tablo, Karşılaştırma
  - Tab-content div'i ekle
  - Her sekme için tab-pane div'i ekle
  - _Gereksinimler: 3.1, 4.1, 5.1_

- [x] 6.5 Grafik sekmesini tasarla
  - Grafik card'ı ekle
  - Canvas element ekle (id="reportChart")
  - Grafik türü seçimi için dropdown ekle (Çizgi, Sütun, Pasta)
  - İndirme butonları ekle (PNG, JPEG)
  - _Gereksinimler: 3.1-3.9_

- [x] 6.6 Tablo sekmesini tasarla
  - Tablo card'ı ekle
  - Table element ekle (id="reportTable")
  - Sayfa boyutu seçimi için select ekle (10, 25, 50, 100)
  - Pagination container ekle (id="tablePagination")
  - Export butonları ekle (CSV, Excel)
  - Özet bilgisi için footer div ekle
  - _Gereksinimler: 4.1-4.10_

- [x] 6.7 Karşılaştırma sekmesini tasarla
  - Karşılaştırma card'ı ekle
  - Dönem 1 tarih seçicileri ekle
  - Dönem 2 tarih seçicileri ekle
  - "Karşılaştır" butonu ekle
  - Karşılaştırma tablosu için container ekle (id="comparisonTable")
  - Karşılaştırma grafiği için canvas ekle (id="comparisonChart")
  - _Gereksinimler: 5.1-5.10_

- [x] 6.8 Yükleniyor göstergesi ve hata container'ı ekle
  - Loading spinner div ekle (başlangıçta gizli)
  - Hata mesajları için alert container ekle (id="error-container")
  - _Gereksinimler: 7.3, 8.5_

- [x] 6.9 Scripts section'ını ekle
  - Chart.js CDN linki ekle (v4.4.0)
  - JavaScript modül dosyalarını referans et
  - _Gereksinimler: 3.1_

### 7. Frontend: JavaScript Modüllerini Oluştur

- [x] 7.1 api-client.js modülünü oluştur
  - `Scripts/reports/api-client.js` dosyası oluştur
  - ApiClient IIFE modülü tanımla
  - Anti-forgery token'ı DOM'dan al
  - `post(url, data)` async fonksiyonu yaz (fetch API ile)
  - `downloadFile(url, data, filename)` fonksiyonu yaz (form submit ile)
  - Hata yönetimi ekle (HTTP 403 kontrolü)
  - showLoader/hideLoader fonksiyonlarını çağır
  - _Gereksinimler: 10.3, 10.4_

- [x] 7.2 chart-module.js modülünü oluştur
  - `Scripts/reports/chart-module.js` dosyası oluştur
  - ChartModule IIFE modülü tanımla
  - chartInstance global değişkeni tanımla
  - `createChart(canvasId, data, type)` fonksiyonu yaz
  - `destroyChart()` fonksiyonu yaz
  - `changeChartType(type)` fonksiyonu yaz
  - `downloadChart(format)` fonksiyonu yaz (PNG/JPEG)
  - Chart.js options yapılandır (responsive, tooltips, legend)
  - _Gereksinimler: 3.1-3.10_

- [x] 7.3 table-module.js modülünü oluştur
  - `Scripts/reports/table-module.js` dosyası oluştur
  - TableModule IIFE modülü tanımla
  - `renderTable(containerId, data, columns)` fonksiyonu yaz
  - `sortData(column, desc)` fonksiyonu yaz
  - `generateSummary(data, columns)` fonksiyonu yaz (Toplam, Ortalama, Maksimum)
  - `attachSortHandlers` fonksiyonu yaz (sütun başlıklarına click event)
  - `getSortIcon(column)` helper fonksiyonu ekle
  - `formatCellValue(value, type)` helper fonksiyonu ekle
  - _Gereksinimler: 4.1-4.7_

- [x] 7.4 comparison-module.js modülünü oluştur
  - `Scripts/reports/comparison-module.js` dosyası oluştur
  - ComparisonModule IIFE modülü tanımla
  - `calculateComparison(period1Data, period2Data)` fonksiyonu yaz
  - `renderComparisonTable(containerId, comparisonData)` fonksiyonu yaz
  - `getTrendIcon(trend)` helper fonksiyonu ekle (↑ ↓ →)
  - Yüzde değişimi hesaplama ekle (∞ desteği dahil)
  - Trend renklendirmesi ekle (yeşil/kırmızı/gri)
  - _Gereksinimler: 5.1-5.10_

- [x] 7.5 Helper fonksiyonlar modülü oluştur
  - `Scripts/reports/helpers.js` dosyası oluştur
  - `formatDate(date)` fonksiyonu yaz (TR locale)
  - `formatNumber(number)` fonksiyonu yaz (binlik ayırıcı)
  - `showLoader()` fonksiyonu yaz
  - `hideLoader()` fonksiyonu yaz
  - `showError(message)` fonksiyonu yaz (Bootstrap alert)
  - `showSuccess(message)` fonksiyonu yaz
  - _Gereksinimler: 7.3, 8.5_

- [x] 7.6 main.js orchestration modülünü oluştur
  - `Scripts/reports/main.js` dosyası oluştur
  - DOMContentLoaded event listener ekle
  - Form submit handler'ları ekle
  - Filtre butonuna click handler ekle
  - Tab değişikliği handler'ı ekle (Bootstrap tab events)
  - Export butonlarına handler'lar ekle
  - Grafik türü değiştirme handler'ı ekle
  - Karşılaştırma butonuna handler ekle
  - Sayfa boyutu değişikliği handler'ı ekle
  - Tüm modülleri koordine et
  - _Gereksinimler: 2.1-2.10, 3.1-3.10, 4.1-4.10, 5.1-5.10_

### 8. Checkpoint - Frontend testleri

- [x] 8.1 Frontend'in çalıştığını doğrula
  - Projeyi çalıştır (F5)
  - Reports sayfasına git
  - Filtreleri test et
  - Her sekmeyi kontrol et
  - Console'da JavaScript hatası olmadığını doğrula

### 9. Menü Entegrasyonu

- [x] 9.1 _Layout.cshtml'e menü öğesi ekle
  - `Views/Shared/_Layout.cshtml` dosyasını aç
  - Navbar'a "Raporlar" menü öğesi ekle
  - Link: `@Html.ActionLink("Raporlar", "Index", "Reports", ...)`
  - Mevcut menü stili ile uyumlu olduğunu doğrula
  - _Gereksinimler: 9.1, 9.2_

### 10. Responsive Tasarım ve Styling

- [ ] 10.1 Responsive CSS kuralları ekle
  - Mobil cihazlar için media query'ler ekle
  - Grafik ve tablo boyutlarını responsive yap
  - Filtre panelini mobilde collapse edilebilir yap
  - _Gereksinimler: 7.4_

- [ ]* 10.2 Custom CSS stilleri ekle
  - `Content/reports.css` dosyası oluştur (opsiyonel)
  - Grafik ve tablo için custom stiller ekle
  - Trend renklendirmesi için CSS sınıfları ekle
  - Loading spinner stilleri ekle
  - BundleConfig.cs'e CSS dosyasını ekle

### 11. Validasyon ve Hata Yönetimi

- [ ] 11.1 Client-side validasyonları ekle
  - Tarih aralığı kontrolü (bitiş > başlangıç)
  - Maksimum 5 yıl kontrolü
  - Gelecek tarih kontrolü
  - Karşılaştırma dönem örtüşme kontrolü
  - _Gereksinimler: 2.8, 2.9, 2.10, 5.2_

- [ ]* 11.2 Server-side validasyonları test et
  - ReportService validasyonlarını test et
  - Hatalı parametrelerle endpoint çağrıları yap
  - Hata mesajlarının doğru geldiğini kontrol et
  - _Gereksinimler: 8.1-8.5_

### 12. Export Fonksiyonları Testi

- [ ]* 12.1 CSV export'u test et
  - Farklı veri türleri için CSV export yap
  - UTF-8 encoding'i doğrula
  - Excel'de açılabilirliğini kontrol et
  - _Gereksinimler: 4.8_

- [ ]* 12.2 Excel export'u test et
  - EPPlus ile XLSX dosyası oluşturulduğunu doğrula
  - Excel'de açılabilirliğini kontrol et
  - Hücre formatlarının doğru olduğunu kontrol et
  - _Gereksinimler: 4.9_

### 13. Performans Testi

- [ ]* 13.1 Performans gereksinimlerini test et
  - 1 yıldan az veri için yanıt süresini ölç (< 3 saniye)
  - 1 yıldan fazla veri için yanıt süresini ölç (< 10 saniye)
  - Timeout senaryosunu test et (> 30 saniye)
  - _Gereksinimler: 7.1, 7.2, 7.5_

### 14. Son Kontrol

- [ ] 14.1 Tüm gereksinimleri gözden geçir
  - Her kabul kriterinin karşılandığını doğrula
  - Farklı roller ile giriş yapıp yetkilendirmeyi test et (SuperAdmin, UnitAdmin, BureauUser)
  - Border filtrelemesinin doğru çalıştığını doğrula
  - Bureau yetki kontrolünün çalıştığını doğrula
  - Tüm sekmelerin düzgün çalıştığını kontrol et

- [ ] 14.2 Dokümantasyon güncelle
  - SistemYapisi.md dosyasına ReportService ve ReportsController bilgilerini ekle
  - Yeni endpoint'leri dokümante et
  - Bureau/Index'ten raporlama sayfasına yönlendirme ekle (gerekirse)

## Notlar

- `*` ile işaretli görevler opsiyoneldir ve MVP için atlanabilir
- Her görev bağımsız test edilebilir şekilde tasarlanmıştır
- Backend görevleri (1-5) frontend'den önce tamamlanmalıdır
- PassportController ve PassportService deseni kesinlikle takip edilmelidir
- Border değeri her zaman oturumdan alınmalı, asla istemciden alınmamalıdır
- Tüm HTTP POST işlemlerinde ValidateAntiForgeryToken kullanılmalıdır

## Görev Bağımlılık Grafiği

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "2.1"] },
    { "id": 1, "tasks": ["3.1", "3.2"] },
    { "id": 2, "tasks": ["3.3", "3.4", "3.5"] },
    { "id": 3, "tasks": ["3.6", "3.7", "3.8"] },
    { "id": 4, "tasks": ["3.9", "3.10", "4.1"] },
    { "id": 5, "tasks": ["4.2", "4.3", "4.4", "4.5"] },
    { "id": 6, "tasks": ["4.6", "4.7", "5.1"] },
    { "id": 7, "tasks": ["6.1", "6.2", "6.3"] },
    { "id": 8, "tasks": ["6.4", "6.5", "6.6", "6.7"] },
    { "id": 9, "tasks": ["6.8", "6.9"] },
    { "id": 10, "tasks": ["7.1", "7.5"] },
    { "id": 11, "tasks": ["7.2", "7.3", "7.4"] },
    { "id": 12, "tasks": ["7.6"] },
    { "id": 13, "tasks": ["8.1"] },
    { "id": 14, "tasks": ["9.1", "10.1", "10.2"] },
    { "id": 15, "tasks": ["11.1", "11.2"] },
    { "id": 16, "tasks": ["12.1", "12.2", "13.1"] },
    { "id": 17, "tasks": ["14.1", "14.2"] }
  ]
}
```
