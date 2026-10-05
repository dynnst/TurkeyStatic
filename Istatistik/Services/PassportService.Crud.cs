using Istatistik.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Istatistik.Services
{
    /// <summary>
    /// Pasaport bürosu veri girişi: listeleme, ekleme/düzenleme, silme.
    /// Tüm işlemler oturumdaki havalimanına (Border) kilitlidir; Border istemciden alınmaz.
    /// </summary>
    public partial class PassportService
    {
        private const int MaxRows = 500;
        private const int MaxCount = 100000000;

        private static string Norm(string type)
        {
            return (type ?? "").Trim().ToLowerInvariant();
        }

        private static string D(DateTime? d)
        {
            return d.HasValue ? d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
        }

        private static string S(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private static T Deserialize<T>(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Veri boş.");
            try
            {
                var m = JsonConvert.DeserializeObject<T>(payload);
                if (m == null) throw new ArgumentException("Veri boş.");
                return m;
            }
            catch (JsonException)
            {
                throw new ArgumentException("Geçersiz veri biçimi. Tarih ve sayı alanlarını kontrol edin.");
            }
        }

        private static void Count(int value, string name)
        {
            if (value < 0 || value > MaxCount)
                throw new ArgumentException(name + " 0 ile " + MaxCount.ToString("N0", CultureInfo.GetCultureInfo("tr-TR")) + " arasında olmalıdır.");
        }

        private static void Validate(object entity)
        {
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(entity, new ValidationContext(entity), results, true))
                throw new ArgumentException("Alan uzunluğu veya değeri geçersiz: " +
                    string.Join(" ", results.Select(r => r.ErrorMessage)));
        }

        private static void RequireDate(DateTime d, string name)
        {
            if (d == DateTime.MinValue || d.Year < 1990 || d.Year > 2100)
                throw new ArgumentException(name + " geçerli bir tarih olmalıdır.");
        }

        private static DateTime? OptDate(DateTime? d, string name)
        {
            if (!d.HasValue || d.Value == DateTime.MinValue) return null;
            RequireDate(d.Value, name);
            return d.Value.Date;
        }

        private static DateTime? BirthDate(DateTime? d)
        {
            if (!d.HasValue || d.Value == DateTime.MinValue) return null;
            if (d.Value.Year < 1900 || d.Value.Date > DateTime.Today)
                throw new ArgumentException("Doğum tarihi geçerli değil.");
            return d.Value.Date;
        }

        #region Listeleme

        public List<object> List(string type, int? year, string search)
        {
            search = S(search);

            switch (Norm(type))
            {
                case "yolcuucak":
                    {
                        var q = _db.YolcuUcakIstatistikleri.Where(x => x.Border == _border);
                        if (year.HasValue) q = q.Where(x => x.Yil == year.Value);
                        return q.OrderByDescending(x => x.Yil).ThenByDescending(x => x.Ay).ThenBy(x => x.HatTuru)
                            .Take(MaxRows).ToList()
                            .Select(x => (object)new
                            {
                                x.Id, x.Yil, x.Ay, HatTuru = (int)x.HatTuru,
                                x.GelenYolcu, x.GidenYolcu, x.ToplamYolcu,
                                x.GelenUcak, x.GidenUcak, x.ToplamUcak
                            }).ToList();
                    }

                case "inad":
                    {
                        var q = _db.InadYolcular.Where(x => x.Border == _border);
                        if (year.HasValue)
                        {
                            var from = new DateTime(year.Value, 1, 1);
                            var to = from.AddYears(1);
                            q = q.Where(x => x.Tarih >= from && x.Tarih < to);
                        }
                        if (search != null)
                            q = q.Where(x => x.AdSoyad.Contains(search) || x.PasaportNo.Contains(search) || x.Uyruk.Contains(search));
                        return q.OrderByDescending(x => x.Tarih).ThenByDescending(x => x.Id)
                            .Take(MaxRows).ToList()
                            .Select(x => (object)new
                            {
                                x.Id, x.SiraNo, Tarih = D(x.Tarih), x.AdSoyad, x.Uyruk,
                                DogumTarihi = D(x.DogumTarihi), GelisTarihi = D(x.GelisTarihi), GidisTarihi = D(x.GidisTarihi), x.PasaportNo, x.GeldigiUlke,
                                x.GittigiUlke, x.HavayoluSirketi, x.InadGerekcesi, x.Aciklamalar
                            }).ToList();
                    }

                case "tahdit":
                    {
                        var q = _db.TahditKayitlari.Where(x => x.Border == _border);
                        if (year.HasValue)
                        {
                            var from = new DateTime(year.Value, 1, 1);
                            var to = from.AddYears(1);
                            q = q.Where(x => x.Tarih >= from && x.Tarih < to);
                        }
                        if (search != null)
                            q = q.Where(x => x.AdSoyad.Contains(search) || x.PasaportVeyaKimlikNo.Contains(search) || x.TahditKodu.Contains(search));
                        return q.OrderByDescending(x => x.Tarih).ThenByDescending(x => x.Id)
                            .Take(MaxRows).ToList()
                            .Select(x => (object)new
                            {
                                x.Id, Tarih = D(x.Tarih), x.AdSoyad, x.Uyruk,
                                DogumTarihi = D(x.DogumTarihi), x.PasaportVeyaKimlikNo, x.TahditKodu, x.Neden
                            }).ToList();
                    }

                case "gunluk":
                    {
                        var q = _db.GunlukZamanSerisiYolcular.Where(x => x.Border == _border);
                        if (year.HasValue) q = q.Where(x => x.Yil == year.Value);
                        return q.OrderByDescending(x => x.Tarih).ThenBy(x => x.Yon).ThenBy(x => x.HatTuru)
                            .Take(MaxRows).ToList()
                            .Select(x => (object)new
                            {
                                x.Id, Tarih = D(x.Tarih), Yon = (int)x.Yon, HatTuru = (int)x.HatTuru,
                                x.GunlukYolcuSayisi, x.UcakSayisi, x.GunlukKumulatifToplam, x.OnAylikToplam
                            }).ToList();
                    }

                case "haftalik":
                    {
                        return _db.HaftalikOlayCizelgeleri
                            .OrderByDescending(x => x.Id).Take(MaxRows).ToList()
                            .Select(x => (object)new
                            {
                                x.Id, x.TarihAraligi, BaslangicTarihi = D(x.BaslangicTarihi), BitisTarihi = D(x.BitisTarihi), x.Havalimani,
                                x.ArananSahisSayisi, x.SahteBelgeSayisi, x.InadEdilenSayisi,
                                x.YazilanCezaMiktari, x.TrafiktenMenSayisi
                            }).ToList();
                    }

                default:
                    throw new ArgumentException("Geçersiz veri tipi.");
            }
        }

        #endregion

        #region Ekleme / Düzenleme

        public object Save(string type, string payload)
        {
            switch (Norm(type))
            {
                case "yolcuucak": return SaveYolcuUcak(Deserialize<YolcuUcakIstatistik>(payload));
                case "inad": return SaveInad(Deserialize<InadYolcu>(payload));
                case "tahdit": return SaveTahdit(Deserialize<TahditKayit>(payload));
                case "gunluk": return SaveGunluk(Deserialize<GunlukZamanSerisiYolcu>(payload));
                case "haftalik": return SaveHaftalik(Deserialize<HaftalikOlayCizelgesi>(payload));
                default: throw new ArgumentException("Geçersiz veri tipi.");
            }
        }

        private object SaveYolcuUcak(YolcuUcakIstatistik m)
        {
            if (m.Yil < 2000 || m.Yil > 2100) throw new ArgumentException("Yıl 2000 ile 2100 arasında olmalıdır.");
            if (m.Ay < 1 || m.Ay > 12) throw new ArgumentException("Ay 1 ile 12 arasında olmalıdır.");
            if (!Enum.IsDefined(typeof(HatTuru), m.HatTuru)) throw new ArgumentException("Hat türü geçersiz.");
            Count(m.GelenYolcu, "Gelen yolcu");
            Count(m.GidenYolcu, "Giden yolcu");
            Count(m.GelenUcak, "Gelen uçak");
            Count(m.GidenUcak, "Giden uçak");

            var duplicate = _db.YolcuUcakIstatistikleri.Any(x =>
                x.Border == _border && x.Yil == m.Yil && x.Ay == m.Ay && x.HatTuru == m.HatTuru && x.Id != m.Id);
            if (duplicate)
                throw new InvalidOperationException("Bu yıl, ay ve hat türü için kayıt zaten var. Mevcut kaydı düzenleyin.");

            YolcuUcakIstatistik e;
            if (m.Id == 0)
            {
                e = new YolcuUcakIstatistik { Border = _border };
                _db.YolcuUcakIstatistikleri.Add(e);
            }
            else
            {
                e = _db.YolcuUcakIstatistikleri.FirstOrDefault(x => x.Id == m.Id && x.Border == _border);
                if (e == null) throw new ArgumentException("Kayıt bulunamadı.");
            }

            e.Yil = m.Yil;
            e.Ay = m.Ay;
            e.HatTuru = m.HatTuru;
            e.GelenYolcu = m.GelenYolcu;
            e.GidenYolcu = m.GidenYolcu;
            e.ToplamYolcu = m.GelenYolcu + m.GidenYolcu;
            e.GelenUcak = m.GelenUcak;
            e.GidenUcak = m.GidenUcak;
            e.ToplamUcak = m.GelenUcak + m.GidenUcak;

            Validate(e);
            _db.SaveChanges();
            return new { e.Id };
        }

        private object SaveInad(InadYolcu m)
        {
            RequireDate(m.Tarih, "Tarih");
            if (S(m.AdSoyad) == null) throw new ArgumentException("Ad soyad gereklidir.");
            if (m.SiraNo < 0 || m.SiraNo > MaxCount) throw new ArgumentException("Sıra no geçersiz.");

            InadYolcu e;
            if (m.Id == 0)
            {
                e = new InadYolcu { Border = _border };
                _db.InadYolcular.Add(e);
            }
            else
            {
                e = _db.InadYolcular.FirstOrDefault(x => x.Id == m.Id && x.Border == _border);
                if (e == null) throw new ArgumentException("Kayıt bulunamadı.");
            }

            e.SiraNo = m.SiraNo;
            e.Tarih = m.Tarih.Date;
            e.AdSoyad = S(m.AdSoyad);
            e.Uyruk = S(m.Uyruk);
            e.DogumTarihi = BirthDate(m.DogumTarihi);
            e.GelisTarihi = OptDate(m.GelisTarihi, "Geliş tarihi");
            e.GidisTarihi = OptDate(m.GidisTarihi, "Gidiş tarihi");
            if (e.GelisTarihi.HasValue && e.GidisTarihi.HasValue && e.GidisTarihi < e.GelisTarihi)
                throw new ArgumentException("Gidiş tarihi geliş tarihinden önce olamaz.");
            e.PasaportNo = S(m.PasaportNo);
            e.GeldigiUlke = S(m.GeldigiUlke);
            e.GittigiUlke = S(m.GittigiUlke);
            e.HavayoluSirketi = S(m.HavayoluSirketi);
            e.InadGerekcesi = S(m.InadGerekcesi);
            e.Aciklamalar = S(m.Aciklamalar);

            Validate(e);
            _db.SaveChanges();
            return new { e.Id };
        }

        private object SaveTahdit(TahditKayit m)
        {
            RequireDate(m.Tarih, "Tarih");
            if (S(m.AdSoyad) == null) throw new ArgumentException("Ad soyad gereklidir.");
            if (S(m.TahditKodu) == null) throw new ArgumentException("Tahdit kodu gereklidir.");

            TahditKayit e;
            if (m.Id == 0)
            {
                e = new TahditKayit { Border = _border };
                _db.TahditKayitlari.Add(e);
            }
            else
            {
                e = _db.TahditKayitlari.FirstOrDefault(x => x.Id == m.Id && x.Border == _border);
                if (e == null) throw new ArgumentException("Kayıt bulunamadı.");
            }

            e.Tarih = m.Tarih.Date;
            e.AdSoyad = S(m.AdSoyad);
            e.Uyruk = S(m.Uyruk);
            e.DogumTarihi = BirthDate(m.DogumTarihi);
            e.PasaportVeyaKimlikNo = S(m.PasaportVeyaKimlikNo);
            e.TahditKodu = S(m.TahditKodu).ToUpper(new CultureInfo("tr-TR"));
            e.Neden = S(m.Neden);

            Validate(e);
            _db.SaveChanges();
            return new { e.Id };
        }

        private object SaveGunluk(GunlukZamanSerisiYolcu m)
        {
            RequireDate(m.Tarih, "Tarih");
            if (!Enum.IsDefined(typeof(Yon), m.Yon)) throw new ArgumentException("Yön geçersiz.");
            if (!Enum.IsDefined(typeof(HatTuru), m.HatTuru)) throw new ArgumentException("Hat türü geçersiz.");
            Count(m.GunlukYolcuSayisi, "Günlük yolcu");
            Count(m.UcakSayisi, "Uçak sayısı");

            var tarih = m.Tarih.Date;
            var duplicate = _db.GunlukZamanSerisiYolcular.Any(x =>
                x.Border == _border && x.Tarih == tarih && x.Yon == m.Yon && x.HatTuru == m.HatTuru && x.Id != m.Id);
            if (duplicate)
                throw new InvalidOperationException("Bu tarih, yön ve hat türü için kayıt zaten var. Mevcut kaydı düzenleyin.");

            GunlukZamanSerisiYolcu e;
            if (m.Id == 0)
            {
                e = new GunlukZamanSerisiYolcu { Border = _border };
                _db.GunlukZamanSerisiYolcular.Add(e);
            }
            else
            {
                e = _db.GunlukZamanSerisiYolcular.FirstOrDefault(x => x.Id == m.Id && x.Border == _border);
                if (e == null) throw new ArgumentException("Kayıt bulunamadı.");
            }

            e.Tarih = tarih;
            e.Yil = tarih.Year;
            e.Ay = tarih.Month;
            e.Gun = tarih.Day;
            e.Yon = m.Yon;
            e.HatTuru = m.HatTuru;
            e.GunlukYolcuSayisi = m.GunlukYolcuSayisi;
            e.UcakSayisi = m.UcakSayisi;

            Validate(e);
            _db.SaveChanges();
            RecalcKumulatif(e.Yil, e.Ay, e.Yon, e.HatTuru);
            _db.SaveChanges();
            return new { e.Id };
        }

        private void RecalcKumulatif(int yil, int ay, Yon yon, HatTuru hat)
        {
            var items = _db.GunlukZamanSerisiYolcular
                .Where(x => x.Border == _border && x.Yil == yil && x.Ay == ay && x.Yon == yon && x.HatTuru == hat)
                .OrderBy(x => x.Tarih).ToList();
            var toplam = 0;
            foreach (var i in items)
            {
                toplam += i.GunlukYolcuSayisi;
                i.GunlukKumulatifToplam = toplam;
            }
        }

        private object SaveHaftalik(HaftalikOlayCizelgesi m)
        {
            var bas = OptDate(m.BaslangicTarihi, "Başlangıç tarihi");
            var bit = OptDate(m.BitisTarihi, "Bitiş tarihi");
            if (bas.HasValue && bit.HasValue && bit < bas)
                throw new ArgumentException("Bitiş tarihi başlangıçtan önce olamaz.");
            if (bas.HasValue && bit.HasValue && S(m.TarihAraligi) == null)
                m.TarihAraligi = bas.Value.ToString("dd.MM.yyyy") + " - " + bit.Value.ToString("dd.MM.yyyy");
            if (S(m.TarihAraligi) == null) throw new ArgumentException("Tarih aralığı gereklidir.");
            Count(m.SorgulananSahisSayisi, "Sorgulanan şahıs sayısı");
            Count(m.ArananSahisSayisi, "Aranan şahıs sayısı");
            Count(m.SahteBelgeSayisi, "Sahte belge sayısı");
            Count(m.InadEdilenSayisi, "İnad edilen sayısı");
            Count(m.TrafiktenMenSayisi, "Trafikten men sayısı");
            if (m.YazilanCezaMiktari < 0 || m.YazilanCezaMiktari > 1000000000000m)
                throw new ArgumentException("Ceza miktarı geçersiz.");

            HaftalikOlayCizelgesi e;
            if (m.Id == 0)
            {
                e = new HaftalikOlayCizelgesi { Border = _border };
                _db.HaftalikOlayCizelgeleri.Add(e);
            }
            else
            {
                e = _db.HaftalikOlayCizelgeleri.FirstOrDefault(x => x.Id == m.Id && x.Border == _border);
                if (e == null) throw new ArgumentException("Kayıt bulunamadı.");
            }

            e.TarihAraligi = S(m.TarihAraligi);
            e.BaslangicTarihi = bas;
            e.BitisTarihi = bit;
            e.Havalimani = S(m.Havalimani) ?? _border;
            e.SorgulananSahisSayisi = m.SorgulananSahisSayisi;
            e.ArananSahisSayisi = m.ArananSahisSayisi;
            e.SahteBelgeSayisi = m.SahteBelgeSayisi;
            e.InadEdilenSayisi = m.InadEdilenSayisi;
            e.YazilanCezaMiktari = Math.Round(m.YazilanCezaMiktari, 2);
            e.TrafiktenMenSayisi = m.TrafiktenMenSayisi;

            Validate(e);
            _db.SaveChanges();
            return new { e.Id };
        }

        #endregion

        #region Silme

        public void Delete(string type, int id)
        {
            switch (Norm(type))
            {
                case "yolcuucak":
                    RemoveOne(_db.YolcuUcakIstatistikleri, x => x.Id == id && x.Border == _border);
                    break;
                case "inad":
                    RemoveOne(_db.InadYolcular, x => x.Id == id && x.Border == _border);
                    break;
                case "tahdit":
                    RemoveOne(_db.TahditKayitlari, x => x.Id == id && x.Border == _border);
                    break;
                case "gunluk":
                    RemoveOne(_db.GunlukZamanSerisiYolcular, x => x.Id == id && x.Border == _border);
                    break;
                case "haftalik":
                    RemoveOne(_db.HaftalikOlayCizelgeleri, x => x.Id == id && x.Border == _border);
                    break;
                default:
                    throw new ArgumentException("Geçersiz veri tipi.");
            }
            _db.SaveChanges();
        }

        private static void RemoveOne<T>(System.Data.Entity.DbSet<T> set, System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            var e = set.FirstOrDefault(predicate);
            if (e == null) throw new ArgumentException("Kayıt bulunamadı.");
            set.Remove(e);
        }

        #endregion
    }
}
