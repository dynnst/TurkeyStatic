# Gereksinimler Belgesi - İstatistik Raporlama ve Görselleştirme Sayfası

## Giriş

Bu belge, havalimanı emniyet birimleri için istatistik raporlama ve görselleştirme sayfasının gereksinimlerini tanımlar. Sistem, rol bazlı kullanıcıların istatistiklerini grafik ve tablo formatında görüntülemesini, farklı zaman aralıklarında filtreleme yapmasını ve karşılaştırma analizleri gerçekleştirmesini sağlar.

## Sözlük

- **Dashboard**: İstatistik Raporlama ve Görselleştirme sisteminin ana ekranı
- **User**: Sisteme giriş yapmış E_User kaydı
- **Border**: Kullanıcının atandığı havalimanı (oturumdan alınır)
- **Bureau**: Havalimanına ait büro (Pasaport, Suç Önleme, İdari, Trafik)
- **Time_Filter**: Zaman aralığı filtreleme birimi (günlük, haftalık, aylık, yıllık)
- **Chart_Component**: İstatistikleri görselleştiren grafik bileşeni
- **Table_Component**: İstatistikleri tablo formatında gösteren bileşen
- **Comparison_Module**: İki farklı zaman aralığını karşılaştıran modül
- **Report_Service**: İstatistik verilerini sorgulayan ve hazırlayan servis katmanı
- **Authorization_Filter**: Kullanıcının yetki kontrolünü yapan filtre

## Gereksinimler

### Gereksinim 1: Rol Bazlı İstatistik Erişimi

**Kullanıcı Hikayesi:** Yetkili kullanıcı olarak, rol ve büro yetkilerime göre istatistikleri görmek istiyorum, böylece sadece erişim hakkım olan verileri görüntüleyebilirim.

#### Kabul Kriterleri

1. WHEN User raporlama sayfasını açtığında, THE sistem SHALL Session["UserRole"] ve Session["Border"] değerlerini kontrol etmeli
2. IF Session["UserRole"] veya Session["Border"] tanımsızsa, THEN THE sistem SHALL kullanıcıyı login sayfasına yönlendirmeli
3. WHERE Session["UserRole"] değeri "SuperAdmin" ise, THE sistem SHALL tüm Border değerleri için Pasaport bürosu istatistiklerini (YolcuUcakIstatistik, GunlukZamanSerisiYolcu, InadYolcu, TahditKayit, HaftalikOlayCizelgesi) görüntüleme yetkisi vermeli
4. WHERE Session["UserRole"] değeri "UnitAdmin" ise, THE sistem SHALL sadece Session["Border"] ile eşleşen Border değeri için tüm büroların istatistiklerini görüntüleme yetkisi vermeli
5. WHERE Session["UserRole"] değeri "BureauUser" ise, THE sistem SHALL sadece Session["Border"] ile eşleşen Border değeri ve Session["BureauCodes"] listesinde bulunan Bureau kodları için istatistikleri görüntüleme yetkisi vermeli
6. WHEN User yetkisi olmayan bir Bureau için istatistik talep ettiğinde, THE sistem SHALL HTTP 403 durum kodu ve "Bu büro için yetkiniz yok" mesajı döndürmeli
7. WHEN User yetkisi olmayan bir Border için istatistik talep ettiğinde, THE sistem SHALL HTTP 403 durum kodu ve "Bu havalimanı için yetkiniz yok" mesajı döndürmeli
8. THE sistem SHALL Border parametresini her zaman Session["Border"] değerinden almalı ve istemciden gelen Border parametresini reddedmeli

### Gereksinim 2: Zaman Aralığı Filtreleme

**Kullanıcı Hikayesi:** Kullanıcı olarak, istatistikleri farklı zaman aralıklarında filtrelemek istiyorum, böylece günlük, haftalık, aylık veya yıllık verileri görebilirim.

#### Kabul Kriterleri

1. THE raporlama sayfası SHALL günlük, haftalık, aylık ve yıllık zaman filtresi seçeneklerini sunmalı
2. WHEN User günlük filtre seçtiğinde, THE Report_Service SHALL belirtilen tarih aralığındaki her gün için kayıt sayılarını toplam değer olarak döndürmeli
3. IF günlük filtre için veri bulunamazsa, THEN THE raporlama sayfası SHALL "Veri bulunamadı" mesajı göstermeli
4. WHEN User haftalık filtre seçtiğinde, THE Report_Service SHALL belirtilen tarih aralığını Pazartesi başlangıçlı haftalara bölerek her hafta için kayıt sayılarını toplam değer olarak döndürmeli
5. WHEN User aylık filtre seçtiğinde, THE Report_Service SHALL belirtilen tarih aralığındaki her takvim ayı için kayıt sayılarını toplam değer olarak döndürmeli
6. WHEN User yıllık filtre seçtiğinde, THE Report_Service SHALL belirtilen tarih aralığındaki her takvim yılı için kayıt sayılarını toplam değer olarak döndürmeli
7. THE raporlama sayfası SHALL başlangıç ve bitiş tarih seçicileri sunmalı
8. IF bitiş tarihi başlangıç tarihinden önceyse, THEN THE raporlama sayfası SHALL "Bitiş tarihi başlangıç tarihinden önce olamaz" ifadesini içeren hata mesajı göstermeli
9. IF User başlangıç tarihinden bitiş tarihine kadar olan süre 5 yılı aşarsa, THEN THE raporlama sayfası SHALL "Maksimum 5 yıllık zaman aralığı seçebilirsiniz" ifadesini içeren hata mesajı göstermeli
10. IF User gelecek tarih seçerse, THEN THE raporlama sayfası SHALL "Gelecek tarih seçilemez" ifadesini içeren hata mesajı göstermeli

### Gereksinim 3: Grafik Görselleştirme

**Kullanıcı Hikayesi:** Kullanıcı olarak, istatistikleri grafik formatında görmek istiyorum, böylece verilerdeki trendleri ve değişimleri görsel olarak algılayabilirim.

#### Kabul Kriterleri

1. THE grafik bileşeni SHALL çizgi grafik, sütun grafik ve pasta grafik türlerini desteklemeli
2. WHEN Pasaport bürosu verileri görüntülendiğinde, THE grafik bileşeni SHALL yolcu sayıları, uçak sayıları, INAD yolcu sayıları, tahdit kayıt sayıları ve haftalık olay istatistiklerini ayrı ayrı görselleştirebilmeli
3. WHERE grafik türü çizgi veya sütun grafiği ise, THE grafik bileşeni SHALL zaman ekseninde birden fazla metriği aynı anda gösterebilmeli
4. WHERE grafik türü pasta grafiği ise, THE grafik bileşeni SHALL tek seferde tek bir metriği göstermeli
5. WHEN grafik üzerinde herhangi bir veri noktasına işaret edildiğinde (hover), THE grafik bileşeni SHALL o noktanın sayısal değerini ve tarihini tooltip olarak göstermeli
6. THE grafik bileşeni SHALL grafik türü değiştirme için açılır menü sunmalı
7. WHEN User pasta grafiği seçtiğinde ve zaman serisi verisi yüklüyse, THE grafik bileşeni SHALL kullanıcıya "Pasta grafik sadece tek bir dönem için kullanılabilir" uyarısı göstermeli
8. THE grafik bileşeni SHALL grafikleri 300 DPI çözünürlükte PNG formatında indirme özelliği sunmalı
9. THE grafik bileşeni SHALL grafikleri 90% kalitede JPEG formatında indirme özelliği sunmalı
10. WHEN grafik verisi boş olduğunda, THE grafik bileşeni SHALL "Seçilen tarih aralığında veri bulunamadı" mesajı göstermeli

### Gereksinim 4: Tablo Görselleştirme

**Kullanıcı Hikayesi:** Kullanıcı olarak, istatistikleri tablo formatında görmek istiyorum, böylece detaylı sayısal verileri inceleyebilirim.

#### Kabul Kriterleri

1. THE tablo bileşeni SHALL filtrelenmiş istatistik verilerini tablo formatında göstermeli
2. WHEN User herhangi bir sütun başlığına tıkladığında, THE tablo bileşeni SHALL o sütuna göre artan sırada sıralama yapmalı
3. WHEN User zaten artan sırada sıralanmış bir sütun başlığına tekrar tıkladığında, THE tablo bileşeni SHALL o sütuna göre azalan sırada sıralama yapmalı
4. THE tablo bileşeni SHALL sayfa başına 25 kayıt göstermeli (varsayılan)
5. THE tablo bileşeni SHALL sayfa boyutu seçenekleri olarak 10, 25, 50 ve 100 sunmalı
6. THE tablo bileşeni SHALL tablonun altında tüm filtrelenmiş verilere göre toplam, ortalama ve maksimum değer hesaplamalarını göstermeli
7. WHEN tablo verisi boş olduğunda, THE tablo bileşeni SHALL "Gösterilecek veri yok" mesajı göstermeli
8. THE tablo bileşeni SHALL tabloyu UTF-8 kodlamalı CSV formatında dışa aktarma özelliği sunmalı
9. THE tablo bileşeni SHALL tabloyu XLSX formatında (Excel 2007+) dışa aktarma özelliği sunmalı
10. IF dışa aktarma işlemi başarısız olursa, THEN THE tablo bileşeni SHALL "Dışa aktarma sırasında hata oluştu" mesajı göstermeli

### Gereksinim 5: Karşılaştırma Analizi

**Kullanıcı Hikayesi:** Kullanıcı olarak, farklı zaman aralıklarını karşılaştırmak istiyorum, böylece dönemsel değişimleri ve trendleri görebilirim.

#### Kabul Kriterleri

1. THE karşılaştırma modülü SHALL birinci dönem ve ikinci dönem için ayrı başlangıç-bitiş tarih seçicileri sunmalı
2. IF birinci dönem ve ikinci dönem tarih aralıkları örtüşüyorsa, THEN THE karşılaştırma modülü SHALL "Dönemler örtüşemez" ifadesini içeren hata mesajı göstermeli
3. WHEN User "Karşılaştır" butonuna tıkladığında, THE karşılaştırma modülü SHALL her iki dönemin verilerini yan yana göstermeli
4. IF her iki dönem için de veri boşsa, THEN THE karşılaştırma modülü SHALL "Her iki dönem için de veri bulunamadı" mesajı göstermeli
5. WHEN karşılaştırma görüntülendiğinde, THE karşılaştırma modülü SHALL (Dönem2 değeri - Dönem1 değeri) formülüyle mutlak fark değerini hesaplamalı
6. WHEN karşılaştırma görüntülendiğinde ve Dönem1 değeri sıfırdan büyükse, THE karşılaştırma modülü SHALL ((Dönem2 - Dönem1) / Dönem1) × 100 formülüyle yüzde değişimi 2 ondalık basamağa yuvarlanmış olarak hesaplamalı
7. IF Dönem1 değeri sıfırsa ve Dönem2 değeri sıfırdan büyükse, THEN THE karşılaştırma modülü SHALL yüzde değişimi yerine "∞" simgesi göstermeli
8. THE karşılaştırma modülü SHALL karşılaştırma sonuçlarını grafik ve tablo formatında sunmalı
9. WHERE yüzde değişimi pozitifse, THE karşılaştırma modülü SHALL değeri yeşil renk ve yukarı ok ikonu ile göstermeli
10. WHERE yüzde değişimi negatifse, THE karşılaştırma modülü SHALL değeri kırmızı renk ve aşağı ok ikonu ile göstermeli

### Gereksinim 6: Büro Bazlı Veri Sunumu

**Kullanıcı Hikayesi:** Kullanıcı olarak, farklı büroların istatistiklerini görmek istiyorum, böylece erişim hakkım olan büro verilerini inceleyebilirim.

#### Kabul Kriterleri

1. THE Dashboard SHALL User'ın erişim yetkisi olan Bureau listesini göstermeli
2. WHEN User bir Bureau seçtiğinde, THE Dashboard SHALL o Bureau'ya ait istatistik türlerini göstermeli
3. WHERE User Pasaport Bureau erişimine sahipse, THE Dashboard SHALL YolcuUcakIstatistik, GunlukZamanSerisiYolcu, InadYolcu, TahditKayit ve HaftalikOlayCizelgesi verilerini sunmalı
4. THE Report_Service SHALL tüm veri sorgularında Border değerini oturumdan almalı
5. THE Report_Service SHALL Border değerini istemciden kabul etmemeli

### Gereksinim 7: Performans ve Kullanılabilirlik

**Kullanıcı Hikayesi:** Kullanıcı olarak, raporların hızlı yüklenmesini ve sistemin akıcı çalışmasını istiyorum, böylece verimli bir şekilde analiz yapabilirim.

#### Kabul Kriterleri

1. WHEN User 1 yıldan az veri sorguladığında, THE Report_Service SHALL 3 saniye içinde sonuç döndürmeli
2. WHEN User 1 yıldan fazla veri sorguladığında, THE Report_Service SHALL 10 saniye içinde sonuç döndürmeli
3. WHILE veri yükleniyorken, THE Dashboard SHALL yükleniyor göstergesi göstermeli
4. THE Dashboard SHALL responsive tasarıma sahip olmalı ve mobil cihazlarda düzgün görüntülenmeli
5. IF sorgu 30 saniyeden uzun sürerse, THEN THE Report_Service SHALL timeout hatası döndürmeli

### Gereksinim 8: Veri Bütünlüğü ve Doğrulama

**Kullanıcı Hikayesi:** Kullanıcı olarak, görüntülenen istatistiklerin doğru ve tutarlı olmasını istiyorum, böylece güvenilir veriler üzerinde çalışabilirim.

#### Kabul Kriterleri

1. THE Report_Service SHALL tüm sorgularda Border filtresi uygulamalı
2. THE Report_Service SHALL tüm sorgularda Bureau yetkisi kontrolü yapmalı
3. WHEN hesaplanmış değerler gösterildiğinde, THE Report_Service SHALL toplam, ortalama ve yüzde hesaplamalarını doğru yapmalı
4. THE Report_Service SHALL null veya geçersiz verileri uygun varsayılan değerlerle değiştirmeli
5. IF veri sorgusu hata döndürürse, THEN THE Dashboard SHALL kullanıcıya anlamlı hata mesajı göstermeli

### Gereksinim 9: Navigasyon ve Entegrasyon

**Kullanıcı Hikayesi:** Kullanıcı olarak, raporlama sayfasına kolayca erişmek istiyorum, böylece mevcut menü yapısından sorunsuz geçiş yapabilirim.

#### Kabul Kriterleri

1. THE Dashboard SHALL ana menüde "Raporlar" veya "İstatistikler" bölümü altında yer almalı
2. WHEN User menüden Dashboard'a tıkladığında, THE Dashboard SHALL yeni bir sayfada açılmalı
3. THE Dashboard SHALL mevcut _Layout.cshtml düzenini kullanmalı
4. THE Dashboard SHALL oturum bilgilerini koruyarak çalışmalı
5. WHEN User Dashboard'dan çıkıp geri döndüğünde, THE Dashboard SHALL son seçilen filtreleri hatırlamalı

### Gereksinim 10: Anti-Forgery ve Güvenlik

**Kullanıcı Hikayesi:** Sistem yöneticisi olarak, raporlama işlemlerinin güvenli olmasını istiyorum, böylece yetkisiz erişim ve veri manipülasyonu engellensin.

#### Kabul Kriterleri

1. THE Dashboard SHALL RoleAuthorize filtresini kullanmalı
2. THE Report_Service SHALL tüm veri sorgularında kullanıcı yetkilerini kontrol etmeli
3. WHEN POST istekleri yapıldığında, THE Dashboard SHALL ValidateAntiForgeryToken kullanmalı
4. THE Report_Service SHALL SQL injection saldırılarına karşı parametreli sorgular kullanmalı
5. THE Dashboard SHALL kullanıcı oturumu geçersizse login sayfasına yönlendirmeli
