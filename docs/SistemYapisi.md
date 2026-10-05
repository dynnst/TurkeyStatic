# Ýstatistik Sistemi: Proje Tanýtým ve Devir Dokümaný

Bu doküman baþka bir yapay zekaya veya geliþtiriciye verilmek üzere hazýrlanmýþtýr.

## 1. Amaç
Havalimaný emniyet birimleri için, havalimanýna (Border) ve büroya göre yetkilendirilmiþ bir istatistik veri giriþ ve raporlama sistemi. Ýlk uygulanan büro **Pasaport Bürosu**. Diðer bürolarýn (Suç Önleme, Ýdari, Trafik) giriþ ekranlarý henüz yapýlmadý.

## 2. Teknoloji
- .NET Framework 4.7.2, ASP.NET MVC 5, Razor
- Entity Framework 6 (Code First, Migrations)
- SQL Server (LocalDB: `(localdb)\MSSQLLocalDB`, Integrated Security)
- Bootstrap 5 (CDN), Bootstrap Icons
- Newtonsoft.Json
- Oturum: `Session` ve `FormsAuthentication`

## 3. Veri tabanlarý
| Veri tabaný | Context | Kullaným |
|---|---|---|
| `CEZA` (mevcut, dýþarýdan) | `CezaContext` | Salt okunur. `dbo.E_User` tablosu kimlik doðrulama için kullanýlýr. Initializer `null`, migration yok. |
| Uygulamanýn kendi veri tabaný | `IstatistikContext` | Yetki, büro ve pasaport verileri. EF migration ile yönetilir. |

**`E_User` (`EUser` modeli) alanlarý:** `fldId`, `fldSicil`, `fldName`, `fldLastName`, `fldEmail`, `fldPassword`, `fldBorder`, `fldActive`, `IsActive`, `Role`.
Þifre kontrolü `IdentityPasswordVerifier.Verify` ile yapýlýr.

### 3.1 Baðlantý dizeleri (`Web.config`)
- `IstatistikContext`: `Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=IstatistikDb;Integrated Security=True;`
- `CezaContext`: `Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CEZA;Integrated Security=True;`

### 3.2 `CEZA` veri tabaný (dýþarýdan, salt okunur)
**`dbo.E_User`**: `fldId` (PK, int), `fldSicil`, `fldName`, `fldLastName`, `fldEmail`, `fldPassword`, `fldBorder`, `fldActive` (string, "1" = aktif), `Role`, `IsActive` (bool?).
Uygulama bu tabloya yazmaz. Gerçek tablo þemasý bu modelden fazla sütun içerebilir.

### 3.3 `IstatistikDb` tablolarý (`IstatistikContext`)
Tablo adlarý `Migrations` klasöründeki dosyalardan doðrulandý. EF çoðullaþtýrmasý nedeniyle bazý adlar sýra dýþýdýr: `Bureaux`, `UserBureaux`, `YolcuUcakIstatistiks`, `InadYolcus`, `TahditKayits`, `GunlukZamanSerisiYolcus`, `HaftalikOlayCizelgesis`. Diðer tablolarýn (`Units`, `Users`, `CrimeStatistics` vb.) adlarý `InitialCreate` migration'ýndan doðrulanmadý. Aþaðýdaki tablolarda ilk sütun DbSet adý, parantez içi gerçek tablo adýdýr.

**Yetki tablolarý**

| Tablo (DbSet) | Sütunlar | Kýsýtlar / Ýndeksler |
|---|---|---|
| `Bureaus` (`dbo.Bureaux`) |
| `UserAssignments` | Id (PK), Sicil (50, zorunlu), Border (150, zorunlu), Role (30, zorunlu), IsActive, CreatedDate, CreatedBy (50) | Sicil benzersiz, Border indeksli. Bir sicilin tek atamasý olur. |
| `UserBureaus` (`dbo.UserBureaux`) |

**Pasaport tablolarý** (hepsinde `Id` PK ve `Border` zorunlu, 150 karakter, indeksli)

| Tablo (DbSet) | Sütunlar |
|---|---|
| `YolcuUcakIstatistikleri` (`dbo.YolcuUcakIstatistiks`) |
| `InadYolcular` (`dbo.InadYolcus`) |
| `TahditKayitlari` (`dbo.TahditKayits`) |
| `GunlukZamanSerisiYolcular` (`dbo.GunlukZamanSerisiYolcus`) |
| `HaftalikOlayCizelgeleri` (`dbo.HaftalikOlayCizelgesis`) |

Servis katmaný þu mükerrer kayýtlarý engeller (veri tabanýnda benzersiz indeks yoktur):
- `YolcuUcakIstatistikleri`: Border + Yil + Ay + HatTuru
- `GunlukZamanSerisiYolcular`: Border + Tarih + Yon + HatTuru

**Eski (legacy) tablolar.** Bunlar `Border` içermez ve yeni yetki modeline baðlý deðildir. `Unit` ve `UnitId` mantýðýna dayanýr.

| Tablo (DbSet) | Sütunlar |
|---|---|
| `Units` | UnitId (PK), UnitName (100), Description (500), IsActive, CreatedDate |
| `Users` | UserId (PK), Username (100), Email (256), FullName (256), PasswordHash (256), UnitId (FK), Role (50), IsActive, CreatedDate, LastLoginDate. **Artýk kimlik doðrulamada kullanýlmýyor**, yerine `E_User` var. |
| `CrimeStatistics` | CrimeStatisticId (PK), UnitId (FK), EntryDate, CrimeType (100), CrimeCode (20), CasesRequiringFollow_up, SuspectCount, ArrestedCount, JudicialControlCount, Notes (500), CreatedDate, ModifiedDate, CreatedBy (100), ModifiedBy (100) |
| `QueryStatistics` | QueryStatisticId (PK), UnitId (FK), EntryDate, Shift (50), PersonQueriedCount, PersonArrestedSearchedCount, OperationType (50), Notes (500), CreatedDate, ModifiedDate, CreatedBy, ModifiedBy |
| `CrimePreventionActivities` | ActivityId (PK), UnitId (FK), EntryDate, WarrantSource (50), WarrantCount, ApprehendedCount, ApprehensionLocation (200), ArrestedCount, Notes (500), CreatedDate, ModifiedDate, CreatedBy, ModifiedBy |
| `AuditLogs` | AuditLogId (PK), UnitId (null, FK), Username (100), Action (50), EntityType (100), EntityId, OldValue (1000), NewValue (1000), ActionDate, IPAddress (500) |

Not: Eski `HomeController` bu tablolarý kullanmak yerine bellek içi listelerle çalýþýyor. Tablolar migration ile oluþturuldu ama veri yazýlmýyor olabilir.

### 3.4 Ýliþki özeti
- `E_User.fldSicil` ? `UserAssignments.Sicil` (mantýksal bað. Farklý veri tabanlarý olduðu için FK yok)
- `E_User.fldBorder` ? `UserAssignments.Border` ? `Bureaus.Border` ? pasaport tablolarýndaki `Border` (metin eþleþmesi, FK yok)
- `UserAssignments` 1—N `UserBureaus` N—1 `Bureaus`

## 4. Yetki modeli (rol + havalimaný + büro)
**Roller (`AppRoles`)**
- `SuperAdmin`: Tüm havalimanlarý. Web.config içindeki `SuperAdminSicils` listesinden veya yönetim ekranýndan atanýr.
- `UnitAdmin`: Yalnýzca kendi havalimaný. Tüm bürolara eriþir, kullanýcý ve büro yönetir.
- `BureauUser`: Yalnýzca kendisine atanan bürolar.

**Tablolar (`Models/Authorization.cs`)**
- `Bureau`: `Border`, `Code`, `Name`, `IsActive`
- `UserAssignment`: `Sicil`, `Border`, `Role`, `IsActive`
- `UserBureau`: `UserAssignmentId` ile `BureauId` arasýndaki iliþki
- `BureauCodes.Pasaport` bir sabittir. Diðer varsayýlan bürolar: `SUC_ONLEME`, `IDARI`, `TRAFIK`.
- `IBorderEntity`: `Border` alaný olan tüm veri tablolarý bunu uygular.

**Temel kural:** `Border` her zaman oturumdan (`Session["Border"]`) alýnýr, istemciden hiçbir zaman alýnmaz. Her sorgu `Border == _border` ile filtrelenir. Yalnýzca `SuperAdmin` baþka havalimaný seçebilir.

**Giriþ akýþý (`AuthService.Authenticate`)**
1. `E_User` içinde sicil aranýr. `IsActive` ve `fldActive == "1"` olmalý.
2. Þifre doðrulanýr.
3. Sicil `SuperAdminSicils` listesindeyse `SuperAdmin` olur.
4. Deðilse `UserAssignments` tablosundan rol okunur. Atama yoksa veya atamanýn `Border` deðeri `fldBorder` ile uyuþmuyorsa giriþ reddedilir.
5. Büro kodlarý `UserBureaus` üzerinden yüklenir.
6. `AccountController.Login`, `Session["UserRole"]`, `["Border"]`, `["BureauCodes"]`, `["Username"]`, `["FullName"]` ve `["LoginTime"]` deðerlerini yazar.

`CurrentUser.FromSession(Session)` bu oturum bilgilerini okur. `CanAccessBureau(code)` metodu SuperAdmin ve UnitAdmin için her zaman true döner.
Kullanýlan filtre: `[RoleAuthorize(AppRoles.SuperAdmin, ...)]`.

## 5. Pasaport veri modelleri (`Models/PassportModels.cs`)
Hepsinde `Border` (zorunlu, 150 karakter, indeksli) vardýr.

| Model | Önemli alanlar |
|---|---|
| `YolcuUcakIstatistik` | Yil, Ay, HatTuru (Ic/Dis), Gelen/Giden/Toplam Yolcu, Gelen/Giden/Toplam Ucak |
| `GunlukZamanSerisiYolcu` | Tarih, Yil, Ay, Gun, Yon (Gelen/Giden), HatTuru, GunlukYolcuSayisi, UcakSayisi, GunlukKumulatifToplam, OnAylikToplam |
| `InadYolcu` | SiraNo, Tarih, AdSoyad, Uyruk, DogumTarihi, GelisTarihi, GidisTarihi, PasaportNo, GeldigiUlke, GittigiUlke, HavayoluSirketi, InadGerekcesi, Aciklamalar |
| `TahditKayit` | Tarih, AdSoyad, Uyruk, DogumTarihi, PasaportVeyaKimlikNo, TahditKodu, Neden |
| `HaftalikOlayCizelgesi` | TarihAraligi, BaslangicTarihi, BitisTarihi, Havalimani, SorgulananSahisSayisi, ArananSahisSayisi, SahteBelgeSayisi, InadEdilenSayisi, YazilanCezaMiktari, TrafiktenMenSayisi |

`UcakSayisi`, `GelisTarihi`, `GidisTarihi`, `BaslangicTarihi` ve `BitisTarihi` son aþamada eklendi ve `Update-Database` ile veri tabanýna uygulandý.

`AddAuthorizationAndBorder1` ve `AddPassportMissingFields` (202610041617394)

## 6. Katmanlar ve dosyalar
**Controllers**
- `AccountController`: Login, Logout, Profile. Register kapalý.
- `BureauController`: Kullanýcýnýn yetkili bürolarýný kart olarak listeler. Pasaport için `Passport/Index`'e, diðerleri için eski `Home/DataEntry`'e yönlendirir. `BureauSeeder`, bir havalimanýnda hiç büro yoksa varsayýlan 4 bürosunu oluþturur.
- `PassportController`: `Index`, `List`, `Save`, `Delete`, analitik uçlar, dosya `Import`. Tüm iþlemler `PassportService` üzerinden `Run(...)` ile yapýlýr. Hatalar JSON `{success, message}` olarak döner.
- `AdminController`: Rol atama, büro izinleri, yeni büro ekleme, büro aktif/pasif yapma, SuperAdmin listesi (`AdminService` kullanýr).
- `HomeController`: Eski suç/sorgu/faaliyet modülleri. Bellek içi listelerle çalýþýr, veri tabanýna baðlý deðildir.

**Services**
- `AuthService` içinde `CurrentUser` ve `AuthResult` sýnýflarý da vardýr.
- `AdminService`, `BureauSeeder`
- `PassportService.cs`: Aylýk istatistik, günlük zaman serisi, INAD özeti, tahdit arama, CSV/JSON import.
- `PassportService.Crud.cs` (partial): `List`, `Save`, `Delete`. Tip anahtarlarý: `yolcuucak`, `gunluk`, `inad`, `tahdit`, `haftalik`. Doðrulama, mükerrer kayýt engeli ve `Border` kilidi burada. Kümülatif toplam kayýtta hesaplanýr (`RecalcKumulatif`).

**Views**
- `Shared/_Layout.cshtml`: Menü. Bootstrap JS yalnýzca bir kez yüklenmeli, aksi hâlde dropdown bozulur.
- `Bureau/Index`: Büro seçimi.
- `Passport/Index`: Beþ sekmeli veri giriþ ekraný (Yolcu/Uçak, Günlük, Ýnad, Tahdit, Haftalýk). Form alanlarý JavaScript'te `TYPES` nesnesiyle tanýmlanýr ve dinamik üretilir. AJAX (`fetch`) ile `List`, `Save`, `Delete` çaðrýlýr. Anti-forgery token kullanýlýr.
- `Admin/Index`: Yetki yönetimi ekraný.
- `Home/Index`, `Home/DataEntry` (eski), `Account/*`

**Diðer:** `Data/IstatistikContext.cs`, `Data/CezaContext.cs`, `Scripts/data-entry.js` (eski ekran).

## 7. Kullanýcý akýþý
1. Kullanýcý `E_User` bilgileriyle giriþ yapar.
2. **Veri Giriþ** menüsü `Bureau/Index` sayfasýný açar. Büro kartlarý yalnýzca yetkili ve aktif bürolar için gösterilir.
3. Pasaport kartý `Passport/Index` ekranýna gider.
4. Kayýt eklenir, düzenlenir, silinir. Her iþlem sadece kendi havalimaný için çalýþýr.
5. Yönetici, **Yetki Yönetimi**'nden kullanýcýya rol ve büro atar.

## 8. Bilinen sorunlar ve dikkat edilecekler
- **Eski modül:** `Home/DataEntry` hâlâ bellek içi çalýþýyor ve eski `UnitId` mantýðýndan kalýntýlar içeriyor. Diðer bürolar için yeniden yazýlmalý.
- **On Aylýk Toplam:** `OnAylikToplam` için bir hesaplama yok. Tanýmý belirsiz, formdan kaldýrýldý, deðer 0 kalýyor.
- **Kümülatif toplam:** Kayýt silinince veya kaydýn tarihi baþka aya taþýnýnca kalan kayýtlar yeniden hesaplanmýyor. Eski kayýtlar da hesaptan geçmedi.
- **Import:** CSV/JSON import yeni alanlarý okuyacak þekilde güncellendi ama test edilmedi. Import tarafýnda kayýt baþýna doðrulama ve mükerrer kontrolü yok.
- **Analiz ekranlarý:** `UcakSayisi` ve yeni tarih alanlarý raporlarda henüz kullanýlmýyor.
- **Dosya bozulmasý:** Düzenleme araçlarý birkaç kez dosyalarý bozdu (satýr sonlarýný kýrpmak, Türkçe karakter kodlamasýný bozmak gibi). Birçok dosya UTF-8'e çevrildi. Büyük düzenlemelerde dosyayý tamamen yeniden yazmak daha güvenli.
- **Test:** Otomatik test yok. Son deðiþikliklerin derlemesi ve çalýþmasý doðrulanamadý.
- **Parola doðrulama:** `IdentityPasswordVerifier` ve `CEZA` baðlantý dizesi ortama baðlýdýr, kendi ortamýnýzda kontrol edin.

## 9. Yeni bir yapay zekaya verilecek görev önerisi

> Bu projede `Border` oturumdan alýnýr, istemciden asla alýnmaz. Her yeni büro modülü için þunlarý yap: (1) `IBorderEntity` uygulayan modeller ve migration, (2) `PassportService.Crud.cs` desenine uygun `Border` filtreli servis, (3) `PassportController` desenine uygun controller (`RoleAuthorize` ve `CanAccessBureau` kontrolüyle), (4) `Passport/Index` desenine uygun sekmeli veri giriþ görünümü, (5) `BureauController.ResolveUrl` içinde bürosu için yönlendirme. Dosyalarý deðiþtirirken UTF-8 kodlamasýný ve satýr sonlarýný koru. Her deðiþiklikten sonra projeyi derle.

**Önerilen sýradaki iþler**
1. Suç Önleme, Ýdari ve Trafik bürolarý için giriþ ekranlarý
2. Pasaport raporlama ve grafik ekranlarý
3. `OnAylikToplam` tanýmý ve hesaplamasý
4. Silme/taþýma sonrasý kümülatif yeniden hesaplama
5. Import doðrulamasý
6. Eski `Home/DataEntry` ekranýnýn kaldýrýlmasý
