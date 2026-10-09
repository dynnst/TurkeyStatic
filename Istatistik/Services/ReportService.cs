using Istatistik.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Istatistik.Services
{
    /// <summary>
    /// Raporlama ve görselleştirme servisi. Tüm sorgu işlemleri oturumdaki havalimanına (Border) kilitlidir.
    /// </summary>
    public class ReportService
    {
        private readonly IstatistikContext _db;
        private readonly CurrentUser _user;
        private readonly string _border;
        private readonly List<string> _bureauCodes;

        private static readonly CultureInfo TrCulture = new CultureInfo("tr-TR");

        public ReportService(IstatistikContext db, CurrentUser user)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _border = _user.Border;

            if (string.IsNullOrWhiteSpace(_border))
                throw new UnauthorizedAccessException("Havalimanı bilginiz tanımlı değil.");

            _bureauCodes = _user.BureauCodes ?? new List<string>();
        }

        #region Yetkilendirme

        private void ValidateDataTypeAccess(string dataType)
        {
            if (_user.IsSuperAdmin || _user.IsUnitAdmin) return;

            var dt = (dataType ?? "").ToLowerInvariant();
            if (dt.StartsWith("bilgitek_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.BilgiTeknolojileri))
                    throw new UnauthorizedAccessException("Bilgi Teknolojileri bürosu verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("cctv_"))
            {
                // CCTV bürosu kodu var mı kontrol et (varsayalım CCTV ayrı bir büro koduna sahip veya BilgiTeknolojileri altında)
                // BureauCodes içinde CCTV yoksa BilgiTeknolojileri sayabiliriz veya yeni bir kod ekleyebiliriz.
                // Eğer ayrı büro değilse herkes görebilir mantığı kalabilir ama genelde Bilgi Teknolojileri bakar.
                if (!_user.CanAccessBureau(BureauCodes.BilgiTeknolojileri)) // Varsa BureauCodes.Cctv eklenebilir
                    throw new UnauthorizedAccessException("CCTV verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("trafik_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.Trafik))
                    throw new UnauthorizedAccessException("Trafik bürosu verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("gbtuyap_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.GbtUyap))
                    throw new UnauthorizedAccessException("GBT / UYAP verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("ytssorgu_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.YtsSorgu))
                    throw new UnauthorizedAccessException("YTS Sorgu verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("idari_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.Idari))
                    throw new UnauthorizedAccessException("İdari büro verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("guvenlik_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.Guvenlik))
                    throw new UnauthorizedAccessException("Güvenlik hizmetleri verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("suconleme_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.SucOnleme))
                    throw new UnauthorizedAccessException("Suç önleme verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("seyahat_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.SeyahatBelgeRisk))
                    throw new UnauthorizedAccessException("Seyahat belgesi risk verileri için yetkiniz yok.");
                return;
            }
            if (dt.StartsWith("pasaport_"))
            {
                if (!_user.CanAccessBureau(BureauCodes.Pasaport))
                    throw new UnauthorizedAccessException("Pasaport bürosu verileri için yetkiniz yok.");
                return;
            }
        }

        #endregion

        #region Tarih Validasyon

        private void ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("Bitiş tarihi başlangıç tarihinden önce olamaz");

            if (startDate > DateTime.Now)
                throw new ArgumentException("Gelecek tarih seçilemez");

            if ((endDate - startDate).TotalDays > 1825)
                throw new ArgumentException("Maksimum 5 yıllık zaman aralığı seçebilirsiniz");
        }

        #endregion

        #region Ana Toplama Metodu

        public AggregatedDataResult GetAggregatedData(string dataType, DateTime startDate, DateTime endDate, PeriodType periodType)
        {
            ValidateDateRange(startDate, endDate);
            ValidateDataTypeAccess(dataType);

            var result = new AggregatedDataResult
            {
                DataType = dataType,
                PeriodType = periodType,
                StartDate = startDate,
                EndDate = endDate
            };

            var dt = (dataType ?? "").ToLowerInvariant();
            if (dt.Contains("_"))
            {
                result.DataPoints = AggregateDynamic(startDate, endDate, periodType, dataType);
            }
            else
            {
                switch (dt)
                {
                    case "yolcuucak":
                        result.DataPoints = AggregateYolcuUcak(startDate, endDate, periodType);
                        break;
                    case "gunluk":
                        result.DataPoints = AggregateGunluk(startDate, endDate, periodType);
                        break;
                    case "inad":
                        result.DataPoints = AggregateInad(startDate, endDate, periodType);
                        break;
                    case "tahdit":
                        result.DataPoints = AggregateTahdit(startDate, endDate, periodType);
                        break;
                    case "haftalik":
                        result.DataPoints = AggregateHaftalik(startDate, endDate, periodType);
                        break;
                    default:
                        throw new ArgumentException("Geçersiz veri tipi: " + dataType);
                }
            }

            // Özet hesapla
            if (dt == "tahdit")
            {
                result.Summary["Toplam"] = result.DataPoints.Sum(p => p.Value);
                result.Summary["Ekleme"] = result.DataPoints.Sum(p => MetricValue(p, "Ekleme"));
                result.Summary["Kaldırma"] = result.DataPoints.Sum(p => MetricValue(p, "Kaldırma"));
            }
            else if (result.DataPoints.Any())
            {
                var first = result.DataPoints.First();
                if (first.Metrics != null && first.Metrics.Count > 0)
                {
                    foreach (var m in first.Metrics.Keys)
                    {
                        result.Summary[m + " (Toplam)"]   = result.DataPoints.Sum(p => p.Metrics.ContainsKey(m) ? p.Metrics[m] : 0);
                        result.Summary[m + " (Ortalama)"] = Math.Round(result.DataPoints.Average(p => p.Metrics.ContainsKey(m) ? p.Metrics[m] : 0), 2);
                        result.Summary[m + " (Maksimum)"] = result.DataPoints.Max(p => p.Metrics.ContainsKey(m) ? p.Metrics[m] : 0);
                    }
                }
                else
                {
                    result.Summary["Toplam"]   = result.DataPoints.Sum(p => p.Value);
                    result.Summary["Ortalama"] = Math.Round(result.DataPoints.Average(p => p.Value), 2);
                    result.Summary["Maksimum"] = result.DataPoints.Max(p => p.Value);
                }
            }

            return result;
        }

        #endregion

        #region Yolcu/Uçak İstatistikleri

        private List<AggregatedDataPoint> AggregateYolcuUcak(DateTime start, DateTime end, PeriodType periodType)
        {
            // Bu tablo Yil+Ay bazlıdır; günlük/haftalık desteklenmez
            if (periodType == PeriodType.Daily || periodType == PeriodType.Weekly)
                throw new ArgumentException("Yolcu/Uçak verileri günlük veya haftalık gösterilemez. Lütfen Aylık veya Yıllık seçin.");

            // Veritabanından border + yıl aralığı filtresiyle çek
            var raw = _db.YolcuUcakIstatistikleri
                .Where(x => x.Border == _border && x.Yil >= start.Year && x.Yil <= end.Year)
                .ToList();

            if (periodType == PeriodType.Monthly)
            {
                return raw
                    .Select(x => new { Date = new DateTime(x.Yil, x.Ay, 1), x.ToplamYolcu, x.ToplamUcak })
                    .Where(x => x.Date >= new DateTime(start.Year, start.Month, 1)
                             && x.Date <= new DateTime(end.Year, end.Month, 1))
                    .GroupBy(x => x.Date)
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = g.Key,
                        Value = g.Sum(x => x.ToplamYolcu),
                        Label = g.Key.ToString("MMM yyyy", TrCulture),
                        Metrics = new Dictionary<string, decimal>
                        {
                            ["ToplamYolcu"] = g.Sum(x => x.ToplamYolcu),
                            ["ToplamUcak"]  = g.Sum(x => x.ToplamUcak)
                        }
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else // Yearly
            {
                return raw
                    .GroupBy(x => x.Yil)
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = new DateTime(g.Key, 1, 1),
                        Value = g.Sum(x => x.ToplamYolcu),
                        Label = g.Key.ToString(),
                        Metrics = new Dictionary<string, decimal>
                        {
                            ["ToplamYolcu"] = g.Sum(x => x.ToplamYolcu),
                            ["ToplamUcak"]  = g.Sum(x => x.ToplamUcak)
                        }
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
        }

        #endregion

        #region Günlük Zaman Serisi (Yolcu ve Uçak Birlikte)

        private List<AggregatedDataPoint> AggregateGunluk(DateTime start, DateTime end, PeriodType periodType)
        {
            var s = start.Date;
            var e = end.Date;

            var raw = _db.GunlukZamanSerisiYolcular
                .Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e)
                .ToList();

            if (periodType == PeriodType.Daily)
            {
                return raw
                    .GroupBy(x => x.Tarih.Date)
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = g.Key,
                        Value = g.Sum(x => x.GunlukYolcuSayisi), // Grafiğin ana çizgisi Yolcu olacak
                        Label = g.Key.ToString("dd.MM.yyyy"),
                        Metrics = new Dictionary<string, decimal>
                        {
                            ["İç Hat Gelen Yolcu"]  = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Ic).Sum(x => x.GunlukYolcuSayisi),
                            ["Dış Hat Gelen Yolcu"] = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Dis).Sum(x => x.GunlukYolcuSayisi),
                            ["İç Hat Giden Yolcu"]  = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Ic).Sum(x => x.GunlukYolcuSayisi),
                            ["Dış Hat Giden Yolcu"] = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Dis).Sum(x => x.GunlukYolcuSayisi),
                            ["Toplam Yolcu"]        = g.Sum(x => x.GunlukYolcuSayisi),
                            
                            ["İç Hat Gelen Uçak"]   = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Ic).Sum(x => x.UcakSayisi),
                            ["Dış Hat Gelen Uçak"]  = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Dis).Sum(x => x.UcakSayisi),
                            ["İç Hat Giden Uçak"]   = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Ic).Sum(x => x.UcakSayisi),
                            ["Dış Hat Giden Uçak"]  = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Dis).Sum(x => x.UcakSayisi),
                            ["Toplam Uçak"]         = g.Sum(x => x.UcakSayisi)
                        }
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else if (periodType == PeriodType.Weekly)
            {
                return raw
                    .GroupBy(x => GetWeekStartDate(x.Tarih))
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = g.Key,
                        Value = g.Sum(x => x.GunlukYolcuSayisi),
                        Label = "Hafta: " + g.Key.ToString("dd.MM.yyyy"),
                        Metrics = new Dictionary<string, decimal>
                        {
                            ["İç Hat Gelen Yolcu"]  = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Ic).Sum(x => x.GunlukYolcuSayisi),
                            ["Dış Hat Gelen Yolcu"] = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Dis).Sum(x => x.GunlukYolcuSayisi),
                            ["İç Hat Giden Yolcu"]  = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Ic).Sum(x => x.GunlukYolcuSayisi),
                            ["Dış Hat Giden Yolcu"] = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Dis).Sum(x => x.GunlukYolcuSayisi),
                            ["Toplam Yolcu"]        = g.Sum(x => x.GunlukYolcuSayisi),
                            
                            ["İç Hat Gelen Uçak"]   = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Ic).Sum(x => x.UcakSayisi),
                            ["Dış Hat Gelen Uçak"]  = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Dis).Sum(x => x.UcakSayisi),
                            ["İç Hat Giden Uçak"]   = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Ic).Sum(x => x.UcakSayisi),
                            ["Dış Hat Giden Uçak"]  = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Dis).Sum(x => x.UcakSayisi),
                            ["Toplam Uçak"]         = g.Sum(x => x.UcakSayisi)
                        }
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else if (periodType == PeriodType.Monthly)
            {
                return raw
                    .GroupBy(x => new DateTime(x.Tarih.Year, x.Tarih.Month, 1))
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = g.Key,
                        Value = g.Sum(x => x.GunlukYolcuSayisi),
                        Label = g.Key.ToString("MMM yyyy", TrCulture),
                        Metrics = new Dictionary<string, decimal>
                        {
                            ["İç Hat Gelen Yolcu"]  = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Ic).Sum(x => x.GunlukYolcuSayisi),
                            ["Dış Hat Gelen Yolcu"] = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Dis).Sum(x => x.GunlukYolcuSayisi),
                            ["İç Hat Giden Yolcu"]  = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Ic).Sum(x => x.GunlukYolcuSayisi),
                            ["Dış Hat Giden Yolcu"] = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Dis).Sum(x => x.GunlukYolcuSayisi),
                            ["Toplam Yolcu"]        = g.Sum(x => x.GunlukYolcuSayisi),
                            
                            ["İç Hat Gelen Uçak"]   = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Ic).Sum(x => x.UcakSayisi),
                            ["Dış Hat Gelen Uçak"]  = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Dis).Sum(x => x.UcakSayisi),
                            ["İç Hat Giden Uçak"]   = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Ic).Sum(x => x.UcakSayisi),
                            ["Dış Hat Giden Uçak"]  = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Dis).Sum(x => x.UcakSayisi),
                            ["Toplam Uçak"]         = g.Sum(x => x.UcakSayisi)
                        }
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else // Yearly
            {
                return raw
                    .GroupBy(x => new DateTime(x.Tarih.Year, 1, 1))
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = g.Key,
                        Value = g.Sum(x => x.GunlukYolcuSayisi),
                        Label = g.Key.ToString("yyyy"),
                        Metrics = new Dictionary<string, decimal>
                        {
                            ["İç Hat Gelen Yolcu"]  = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Ic).Sum(x => x.GunlukYolcuSayisi),
                            ["Dış Hat Gelen Yolcu"] = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Dis).Sum(x => x.GunlukYolcuSayisi),
                            ["İç Hat Giden Yolcu"]  = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Ic).Sum(x => x.GunlukYolcuSayisi),
                            ["Dış Hat Giden Yolcu"] = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Dis).Sum(x => x.GunlukYolcuSayisi),
                            ["Toplam Yolcu"]        = g.Sum(x => x.GunlukYolcuSayisi),
                            
                            ["İç Hat Gelen Uçak"]   = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Ic).Sum(x => x.UcakSayisi),
                            ["Dış Hat Gelen Uçak"]  = g.Where(x => x.Yon == Yon.Gelen && x.HatTuru == HatTuru.Dis).Sum(x => x.UcakSayisi),
                            ["İç Hat Giden Uçak"]   = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Ic).Sum(x => x.UcakSayisi),
                            ["Dış Hat Giden Uçak"]  = g.Where(x => x.Yon == Yon.Giden && x.HatTuru == HatTuru.Dis).Sum(x => x.UcakSayisi),
                            ["Toplam Uçak"]         = g.Sum(x => x.UcakSayisi)
                        }
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
        }

        #endregion

        
        #region Dinamik Raporlama

        private List<AggregatedDataPoint> AggregateDynamic(DateTime start, DateTime end, PeriodType periodType, string dataType)
        {
            var s = start.Date;
            var e = end.Date.AddDays(1);
            var parts = (dataType ?? "").Split('_');
            if (parts.Length < 2) return new List<AggregatedDataPoint>();

            var prefix = parts[0].ToLowerInvariant();
            var propName = parts[1];

            List<RawDataPoint> raw = new List<RawDataPoint>();

            if (prefix == "bilgitek")
            {
                var records = _db.BilgiTeknolojileriIstatistikleri.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(BilgiTeknolojileriIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }
            else if (prefix == "cctv")
            {
                var records = _db.CctvIstatistikleri.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(CctvIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }
            else if (prefix == "trafik")
            {
                var records = _db.TrafikIstatistikleri.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(TrafikIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }
            else if (prefix == "gbtuyap")
            {
                var records = _db.GbtUyapSorgulari.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(GbtUyapSorgu).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }
            else if (prefix == "ytssorgu")
            {
                var records = _db.YtsSorgulari.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(YtsSorgu).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }
            else if (prefix == "idari")
            {
                var records = _db.IdariBuroIstatistikleri.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(IdariBuroIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }
            else if (prefix == "guvenlik")
            {
                var records = _db.GuvenlikHizmetleriIstatistikleri.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(GuvenlikHizmetleriIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }
            else if (prefix == "suconleme")
            {
                var records = _db.SucOnlemeIcmallari.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(SucOnlemeIcmal).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }
            else if (prefix == "seyahat")
            {
                var records = _db.SeyahatBelgesiRiskAnalizleri.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < e).ToList();
                var prop = typeof(SeyahatBelgesiRiskAnaliz).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    raw = records.Select(x => {
                        var val = prop.GetValue(x);
                        decimal num = 0;
                        if (val != null) decimal.TryParse(val.ToString(), out num);
                        return new RawDataPoint { Tarih = x.Tarih, Value = num };
                    }).ToList();
                }
            }

            return AggregateByPeriod(raw, periodType);
        }

        #endregion

        #region İNAD

        private List<AggregatedDataPoint> AggregateInad(DateTime start, DateTime end, PeriodType periodType)
        {
            var s = start.Date;
            var e = end.Date;

            var raw = _db.InadYolcular
                .Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e)
                .ToList()
                .Select(x => new RawDataPoint { Tarih = x.Tarih, Value = 1 })
                .ToList();

            return AggregateByPeriod(raw, periodType);
        }

        #endregion

        #region Tahdit

        private List<AggregatedDataPoint> AggregateTahdit(DateTime start, DateTime end, PeriodType periodType)
        {
            var records = QueryTahditKayitlari(start, end);

            IEnumerable<IGrouping<DateTime, TahditKayit>> groups;
            Func<DateTime, string> labelFn;

            if (periodType == PeriodType.Daily)
            {
                groups = records.GroupBy(x => x.Tarih.Date);
                labelFn = d => d.ToString("dd.MM.yyyy");
            }
            else if (periodType == PeriodType.Weekly)
            {
                groups = records.GroupBy(x => GetWeekStartDate(x.Tarih));
                labelFn = d => "Hafta: " + d.ToString("dd.MM.yyyy");
            }
            else if (periodType == PeriodType.Monthly)
            {
                groups = records.GroupBy(x => new DateTime(x.Tarih.Year, x.Tarih.Month, 1));
                labelFn = d => d.ToString("MMM yyyy", TrCulture);
            }
            else
            {
                groups = records.GroupBy(x => new DateTime(x.Tarih.Year, 1, 1));
                labelFn = d => d.Year.ToString();
            }

            return groups
                .Select(g =>
                {
                    int ekleme = g.Count(x => x.IslemTuru == TahditIslemTuru.Ekleme);
                    int kaldirma = g.Count(x => x.IslemTuru == TahditIslemTuru.Kaldirma);
                    return new AggregatedDataPoint
                    {
                        Date = g.Key,
                        Value = g.Count(),
                        Label = labelFn(g.Key),
                        Metrics = new Dictionary<string, decimal>
                        {
                            { "Ekleme", ekleme },
                            { "Kaldırma", kaldirma }
                        }
                    };
                })
                .OrderBy(x => x.Date)
                .ToList();
        }

        private List<TahditKayit> QueryTahditKayitlari(DateTime start, DateTime end)
        {
            var s = start.Date;
            var eExclusive = end.Date.AddDays(1);

            return _db.TahditKayitlari
                .Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < eExclusive)
                .ToList();
        }

        #endregion

        #region Haftalık

        private List<AggregatedDataPoint> AggregateHaftalik(DateTime start, DateTime end, PeriodType periodType)
        {
            var raw = _db.HaftalikOlayCizelgeleri
                .Where(x => x.Border == _border
                    && x.BaslangicTarihi.HasValue
                    && x.BaslangicTarihi.Value >= start
                    && x.BitisTarihi.HasValue
                    && x.BitisTarihi.Value <= end)
                .ToList();

            return raw
                .Select(x => new AggregatedDataPoint
                {
                    Date  = x.BaslangicTarihi.Value,
                    Value = x.SorgulananSahisSayisi,
                    Label = !string.IsNullOrWhiteSpace(x.TarihAraligi)
                        ? x.TarihAraligi
                        : x.BaslangicTarihi.Value.ToString("dd.MM.yyyy")
                })
                .OrderBy(x => x.Date)
                .ToList();
        }

        #endregion

        #region Genel Periyot Toplayıcı

        // EF sorgusunu önce ToList() ile materialize et, sonra bu metoda geç
        private List<AggregatedDataPoint> AggregateByPeriod(List<RawDataPoint> data, PeriodType periodType)
        {
            if (periodType == PeriodType.Daily)
            {
                return data
                    .GroupBy(x => x.Tarih.Date)
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = g.Key,
                        Value = g.Sum(x => x.Value),
                        Label = g.Key.ToString("dd.MM.yyyy")
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else if (periodType == PeriodType.Weekly)
            {
                return data
                    .GroupBy(x => GetWeekStartDate(x.Tarih))
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = g.Key,
                        Value = g.Sum(x => x.Value),
                        Label = "Hafta: " + g.Key.ToString("dd.MM.yyyy")
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else if (periodType == PeriodType.Monthly)
            {
                return data
                    .GroupBy(x => new DateTime(x.Tarih.Year, x.Tarih.Month, 1))
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = g.Key,
                        Value = g.Sum(x => x.Value),
                        Label = g.Key.ToString("MMM yyyy", TrCulture)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else // Yearly
            {
                return data
                    .GroupBy(x => x.Tarih.Year)
                    .Select(g => new AggregatedDataPoint
                    {
                        Date  = new DateTime(g.Key, 1, 1),
                        Value = g.Sum(x => x.Value),
                        Label = g.Key.ToString()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
        }

        private static decimal MetricValue(AggregatedDataPoint point, string key)
        {
            if (point == null || point.Metrics == null || !point.Metrics.ContainsKey(key))
                return 0;
            return point.Metrics[key];
        }

        private DateTime GetWeekStartDate(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        // Dahili transfer nesnesi — EF anonymous type reflection sorununu ortadan kaldırır
        private class RawDataPoint
        {
            public DateTime Tarih { get; set; }
            public decimal Value { get; set; }
        }

        #endregion

        #region Karşılaştırma

        public ComparisonResult GetComparisonData(
            string dataType,
            DateTime p1Start, DateTime p1End,
            DateTime p2Start, DateTime p2End,
            PeriodType periodType)
        {
            // Dönem örtüşme kontrolü
            if (p1Start <= p2End && p1End >= p2Start)
                throw new ArgumentException("Dönemler örtüşemez");

            var period1 = GetAggregatedData(dataType, p1Start, p1End, periodType);
            var period2 = GetAggregatedData(dataType, p2Start, p2End, periodType);

            var result = new ComparisonResult
            {
                DataType      = dataType,
                PeriodType    = periodType,
                Period1Start  = p1Start,
                Period1End    = p1End,
                Period1Data   = period1.DataPoints,
                Period1Summary = period1.Summary,
                Period2Start  = p2Start,
                Period2End    = p2End,
                Period2Data   = period2.DataPoints,
                Period2Summary = period2.Summary
            };

            // Karşılaştırmada ortalama/maksimum gürültüsünü alma; varsa yalnızca toplamlar
            var allKeys = period1.Summary.Keys.Union(period2.Summary.Keys).Distinct().ToList();
            var totalKeys = allKeys.Where(k => k.EndsWith(" (Toplam)", StringComparison.Ordinal)).ToList();
            var keysToCompare = totalKeys.Count > 0 ? (IEnumerable<string>)totalKeys : allKeys;

            foreach (var key in keysToCompare)
            {
                var p1Val = period1.Summary.ContainsKey(key) ? period1.Summary[key] : 0m;
                var p2Val = period2.Summary.ContainsKey(key) ? period2.Summary[key] : 0m;
                var diff  = p2Val - p1Val;

                decimal? pct;
                if (p1Val != 0)
                    pct = Math.Round((diff / p1Val) * 100, 2);
                else if (p2Val > 0)
                    pct = null;   // ∞
                else
                    pct = 0m;

                var displayName = key.EndsWith(" (Toplam)", StringComparison.Ordinal)
                    ? key.Substring(0, key.Length - " (Toplam)".Length)
                    : key;

                result.Differences.Add(new ComparisonMetric
                {
                    MetricName         = displayName,
                    Period1Value       = p1Val,
                    Period2Value       = p2Val,
                    AbsoluteDifference = diff,
                    PercentageChange   = pct,
                    Trend = diff > 0 ? TrendDirection.Up
                          : diff < 0 ? TrendDirection.Down
                          : TrendDirection.Neutral
                });
            }

            return result;
        }

        #endregion

        #region Sayfalı Tablo

        public PagedTableResult GetPagedTableData(
            string dataType, DateTime startDate, DateTime endDate,
            PeriodType periodType, int page, int pageSize,
            string sortBy, bool sortDesc)
        {
            var dt = (dataType ?? "").ToLowerInvariant();
            if (dt == "tahdit")
                return GetPagedTahditRecords(startDate, endDate, page, pageSize, sortBy, sortDesc);
            if (dt == "inad")
                return GetPagedInadRecords(startDate, endDate, page, pageSize, sortBy, sortDesc);

            var aggregated = GetAggregatedData(dataType, startDate, endDate, periodType);

            IEnumerable<AggregatedDataPoint> sorted = aggregated.DataPoints;

            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.ToLowerInvariant() == "value")
                    sorted = sortDesc ? sorted.OrderByDescending(x => x.Value) : sorted.OrderBy(x => x.Value);
                else
                    sorted = sortDesc ? sorted.OrderByDescending(x => x.Date) : sorted.OrderBy(x => x.Date);
            }

            var totalRecords = aggregated.DataPoints.Count;
            var totalPages   = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 1;
            var paged        = sorted.Skip((page - 1) * pageSize).Take(pageSize);

            var result = new PagedTableResult
            {
                CurrentPage  = page,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                TotalPages   = totalPages,
                Summary      = aggregated.Summary,
                Rows = paged.Select(p => 
                {
                    var dict = new Dictionary<string, object>
                    {
                        { "Tarih",  p.Date.ToString("dd.MM.yyyy") },
                        { "Etiket", p.Label }
                    };

                    if (p.Metrics != null && p.Metrics.Count > 0)
                    {
                        foreach(var kvp in p.Metrics)
                        {
                            dict.Add(kvp.Key, kvp.Value);
                        }
                    }
                    else
                    {
                        dict.Add("Değer", p.Value);
                    }

                    return dict;
                }).ToList()
            };

            return result;
        }

        private PagedTableResult GetPagedTahditRecords(DateTime startDate, DateTime endDate, int page, int pageSize, string sortBy, bool sortDesc)
        {
            ValidateDateRange(startDate, endDate);
            ValidateDataTypeAccess("tahdit");

            var list = QueryTahditKayitlari(startDate, endDate);
            IEnumerable<TahditKayit> sorted = SortTahdit(list, sortBy, sortDesc);

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;

            var totalRecords = list.Count;
            var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 1;
            var paged = sorted.Skip((page - 1) * pageSize).Take(pageSize);

            return new PagedTableResult
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                Summary = new Dictionary<string, decimal>
                {
                    { "Kayıt Sayısı", totalRecords },
                    { "Ekleme", list.Count(x => x.IslemTuru == TahditIslemTuru.Ekleme) },
                    { "Kaldırma", list.Count(x => x.IslemTuru == TahditIslemTuru.Kaldirma) }
                },
                Rows = paged.Select(x => new Dictionary<string, object>
                {
                    { "İşlem Tarihi", x.Tarih.ToString("dd.MM.yyyy") },
                    { "Adı Soyadı", x.AdSoyad ?? "" },
                    { "Uyruk", x.Uyruk ?? "" },
                    { "Doğum Tarihi", x.DogumTarihi.HasValue ? x.DogumTarihi.Value.ToString("dd.MM.yyyy") : "" },
                    { "Pasaport / Kimlik No", x.PasaportVeyaKimlikNo ?? "" },
                    { "İşlem", x.IslemTuru == TahditIslemTuru.Ekleme ? "Ekleme" : "Kaldırma" },
                    { "Tahdit Kodu", x.TahditKodu ?? "" },
                    { "Tahdit Nedeni", x.Neden ?? "" }
                }).ToList()
            };
        }

        private static IEnumerable<TahditKayit> SortTahdit(IEnumerable<TahditKayit> source, string sortBy, bool sortDesc)
        {
            Func<TahditKayit, object> key;
            switch ((sortBy ?? "").Trim())
            {
                case "Adı Soyadı":
                case "AdSoyad":
                    key = x => x.AdSoyad ?? "";
                    break;
                case "Uyruk":
                    key = x => x.Uyruk ?? "";
                    break;
                case "Doğum Tarihi":
                case "DogumTarihi":
                    key = x => x.DogumTarihi ?? DateTime.MinValue;
                    break;
                case "Pasaport / Kimlik No":
                case "PasaportVeyaKimlikNo":
                    key = x => x.PasaportVeyaKimlikNo ?? "";
                    break;
                case "İşlem":
                case "IslemTuru":
                    key = x => (int)x.IslemTuru;
                    break;
                case "Tahdit Kodu":
                case "TahditKodu":
                    key = x => x.TahditKodu ?? "";
                    break;
                case "Tahdit Nedeni":
                case "Neden":
                    key = x => x.Neden ?? "";
                    break;
                default:
                    key = x => x.Tarih;
                    break;
            }

            return sortDesc ? source.OrderByDescending(key) : source.OrderBy(key);
        }

        private PagedTableResult GetPagedInadRecords(DateTime startDate, DateTime endDate, int page, int pageSize, string sortBy, bool sortDesc)
        {
            ValidateDateRange(startDate, endDate);
            ValidateDataTypeAccess("inad");

            var s = startDate.Date;
            var eExclusive = endDate.Date.AddDays(1);
            var list = _db.InadYolcular
                .Where(x => x.Border == _border && x.Tarih >= s && x.Tarih < eExclusive)
                .ToList();

            Func<InadYolcu, object> key;
            switch ((sortBy ?? "").Trim())
            {
                case "Adı Soyadı":
                case "AdSoyad":
                    key = x => x.AdSoyad ?? "";
                    break;
                case "Uyruk":
                    key = x => x.Uyruk ?? "";
                    break;
                case "Pasaport No":
                case "PasaportNo":
                    key = x => x.PasaportNo ?? "";
                    break;
                case "Havayolu":
                case "HavayoluSirketi":
                    key = x => x.HavayoluSirketi ?? "";
                    break;
                default:
                    key = x => x.Tarih;
                    break;
            }

            var sorted = sortDesc ? list.OrderByDescending(key) : list.OrderBy(key);
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;

            var totalRecords = list.Count;
            var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 1;

            return new PagedTableResult
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                Summary = new Dictionary<string, decimal> { { "Kayıt Sayısı", totalRecords } },
                Rows = sorted.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new Dictionary<string, object>
                {
                    { "Tarih", x.Tarih.ToString("dd.MM.yyyy") },
                    { "Sıra No", x.SiraNo },
                    { "Adı Soyadı", x.AdSoyad ?? "" },
                    { "Uyruk", x.Uyruk ?? "" },
                    { "Doğum Tarihi", x.DogumTarihi.HasValue ? x.DogumTarihi.Value.ToString("dd.MM.yyyy") : "" },
                    { "Pasaport No", x.PasaportNo ?? "" },
                    { "Geliş Tarihi", x.GelisTarihi.HasValue ? x.GelisTarihi.Value.ToString("dd.MM.yyyy") : "" },
                    { "Gidiş Tarihi", x.GidisTarihi.HasValue ? x.GidisTarihi.Value.ToString("dd.MM.yyyy") : "" },
                    { "Geldiği Ülke", x.GeldigiUlke ?? "" },
                    { "Gittiği Ülke", x.GittigiUlke ?? "" },
                    { "Havayolu", x.HavayoluSirketi ?? "" },
                    { "İNAD Gerekçesi", x.InadGerekcesi ?? "" },
                    { "Açıklamalar", x.Aciklamalar ?? "" }
                }).ToList()
            };
        }

        #endregion

        #region Export

        public byte[] ExportToCsv(PagedTableResult data, string fileName)
        {
            var sb = new StringBuilder();
            if (data == null || data.Rows == null || data.Rows.Count == 0)
                return Encoding.UTF8.GetBytes("Veri yok");

            var headers = data.Rows[0].Keys.ToList();
            sb.AppendLine(string.Join(";", headers));

            foreach (var row in data.Rows)
            {
                var cells = headers.Select(h => (row.ContainsKey(h) ? row[h] : null) == null ? "" : row[h].ToString().Replace(";", ","));
                sb.AppendLine(string.Join(";", cells));
            }

            if (data.Summary != null && data.Summary.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Özet");
                foreach (var kvp in data.Summary)
                    sb.AppendLine(string.Format("{0};{1}", kvp.Key, kvp.Value));
            }

            var preamble = Encoding.UTF8.GetPreamble();
            var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var resultBytes = new byte[preamble.Length + contentBytes.Length];
            Buffer.BlockCopy(preamble, 0, resultBytes, 0, preamble.Length);
            Buffer.BlockCopy(contentBytes, 0, resultBytes, preamble.Length, contentBytes.Length);
            return resultBytes;
        }

        public byte[] ExportToExcel(PagedTableResult data, string fileName)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Emniyet");

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Rapor");

                if (data == null || data.Rows == null || data.Rows.Count == 0)
                {
                    ws.Cells[1, 1].Value = "Veri yok";
                    return package.GetAsByteArray();
                }

                var headers = data.Rows[0].Keys.ToList();
                for (int c = 0; c < headers.Count; c++)
                    ws.Cells[1, c + 1].Value = headers[c];

                int row = 2;
                foreach (var item in data.Rows)
                {
                    for (int c = 0; c < headers.Count; c++)
                    {
                        object val;
                        item.TryGetValue(headers[c], out val);
                        ws.Cells[row, c + 1].Value = val;
                    }
                    row++;
                }

                if (data.Summary != null && data.Summary.Count > 0)
                {
                    row++;
                    ws.Cells[row, 1].Value = "Özet";
                    row++;
                    foreach (var kvp in data.Summary)
                    {
                        ws.Cells[row, 1].Value = kvp.Key;
                        ws.Cells[row, 2].Value = (double)kvp.Value;
                        row++;
                    }
                }

                ws.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        #endregion

        #region Excel + Grafik Export

        /// <summary>
        /// Canvas'tan alınan base64 görsel + veri tablosunu birlikte Excel'e gömer.
        /// </summary>
        public byte[] ExportToExcelWithChart(AggregatedDataResult data, string chartImageBase64, string chartTitle)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Emniyet");

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Grafik Raporu");

                // ── Başlık ──────────────────────────────────────────────
                ws.Cells[1, 1].Value = chartTitle ?? data.DataType;
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.Font.Size = 14;
                ws.Cells[1, 1, 1, 5].Merge = true;

                ws.Cells[2, 1].Value = string.Format("Dönem: {0} – {1}",
                    data.StartDate.ToString("dd.MM.yyyy"),
                    data.EndDate.ToString("dd.MM.yyyy"));
                ws.Cells[2, 1].Style.Font.Italic = true;
                ws.Cells[2, 1, 2, 5].Merge = true;

                // ── Grafik Görseli ───────────────────────────────────────
                int imageEndRow = 3;
                if (!string.IsNullOrWhiteSpace(chartImageBase64))
                {
                    try
                    {
                        var base64 = chartImageBase64;
                        var commaIdx = base64.IndexOf(',');
                        if (commaIdx >= 0) base64 = base64.Substring(commaIdx + 1);

                        var imgBytes = Convert.FromBase64String(base64);
                        var picture = ws.Drawings.AddPicture("Grafik",
                            new System.IO.MemoryStream(imgBytes));
                        picture.SetPosition(3, 0, 0, 0);   // 4. satırdan başla
                        picture.SetSize(800, 380);
                        imageEndRow = 25;
                    }
                    catch
                    {
                        imageEndRow = 4;
                    }
                }

                // ── Veri Tablosu ─────────────────────────────────────────
                int tableStart = imageEndRow + 2;

                // Başlık satırı
                ws.Cells[tableStart, 1].Value = "Tarih";
                ws.Cells[tableStart, 2].Value = "Dönem";
                ws.Cells[tableStart, 3].Value = "Değer";

                using (var hdr = ws.Cells[tableStart, 1, tableStart, 3])
                {
                    hdr.Style.Font.Bold = true;
                    hdr.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    hdr.Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.FromArgb(68, 114, 196));
                    hdr.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                int row = tableStart + 1;
                foreach (var point in data.DataPoints)
                {
                    ws.Cells[row, 1].Value = point.Date.ToString("dd.MM.yyyy");
                    ws.Cells[row, 2].Value = point.Label;
                    ws.Cells[row, 3].Value = (double)point.Value;

                    if (row % 2 == 0)
                    {
                        using (var r = ws.Cells[row, 1, row, 3])
                        {
                            r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            r.Style.Fill.BackgroundColor.SetColor(
                                System.Drawing.Color.FromArgb(235, 241, 250));
                        }
                    }
                    row++;
                }

                // ── Özet ────────────────────────────────────────────────
                if (data.Summary != null && data.Summary.Count > 0)
                {
                    row++;
                    ws.Cells[row, 1].Value = "Özet";
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, 3].Merge = true;
                    row++;

                    foreach (var kvp in data.Summary)
                    {
                        ws.Cells[row, 1].Value = kvp.Key;
                        ws.Cells[row, 2].Value = (double)kvp.Value;
                        ws.Cells[row, 1].Style.Font.Bold = true;
                        row++;
                    }
                }

                ws.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Karşılaştırma sonuçlarını ve grafiğini Excel formatında dışa aktarır.
        /// </summary>
        public byte[] ExportComparisonExcel(ComparisonResult data, string chartImageBase64)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Emniyet");

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Dönem Karşılaştırma");

                // Başlık
                ws.Cells[1, 1].Value = "Dönem Karşılaştırma Raporu - " + (data.DataType ?? "");
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.Font.Size = 14;
                ws.Cells[1, 1, 1, 5].Merge = true;

                ws.Cells[2, 1].Value = string.Format("Dönem 1: {0} – {1}  |  Dönem 2: {2} – {3}",
                    data.Period1Start.ToString("dd.MM.yyyy"),
                    data.Period1End.ToString("dd.MM.yyyy"),
                    data.Period2Start.ToString("dd.MM.yyyy"),
                    data.Period2End.ToString("dd.MM.yyyy"));
                ws.Cells[2, 1].Style.Font.Italic = true;
                ws.Cells[2, 1, 2, 5].Merge = true;

                int imageEndRow = 3;
                if (!string.IsNullOrWhiteSpace(chartImageBase64))
                {
                    try
                    {
                        var base64 = chartImageBase64;
                        var commaIdx = base64.IndexOf(',');
                        if (commaIdx >= 0) base64 = base64.Substring(commaIdx + 1);

                        var imgBytes = Convert.FromBase64String(base64);
                        var picture = ws.Drawings.AddPicture("KarsilastirmaGrafik",
                            new System.IO.MemoryStream(imgBytes));
                        picture.SetPosition(3, 0, 0, 0);
                        picture.SetSize(750, 350);
                        imageEndRow = 23;
                    }
                    catch
                    {
                        imageEndRow = 4;
                    }
                }

                int tableStart = imageEndRow + 2;
                ws.Cells[tableStart, 1].Value = "Metrik";
                ws.Cells[tableStart, 2].Value = string.Format("Dönem 1 ({0:dd.MM.yyyy} - {1:dd.MM.yyyy})", data.Period1Start, data.Period1End);
                ws.Cells[tableStart, 3].Value = string.Format("Dönem 2 ({0:dd.MM.yyyy} - {1:dd.MM.yyyy})", data.Period2Start, data.Period2End);
                ws.Cells[tableStart, 4].Value = "Mutlak Fark";
                ws.Cells[tableStart, 5].Value = "Değişim %";

                using (var hdr = ws.Cells[tableStart, 1, tableStart, 5])
                {
                    hdr.Style.Font.Bold = true;
                    hdr.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    hdr.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(44, 62, 80));
                    hdr.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                int row = tableStart + 1;
                if (data.Differences != null)
                {
                    foreach (var diff in data.Differences)
                    {
                        ws.Cells[row, 1].Value = diff.MetricName;
                        ws.Cells[row, 2].Value = (double)diff.Period1Value;
                        ws.Cells[row, 3].Value = (double)diff.Period2Value;
                        ws.Cells[row, 4].Value = (double)diff.AbsoluteDifference;
                        ws.Cells[row, 5].Value = diff.PercentageChange.HasValue ? string.Format("%{0:F2}", diff.PercentageChange.Value) : "∞";

                        if (row % 2 == 0)
                        {
                            using (var r = ws.Cells[row, 1, row, 5])
                            {
                                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 247, 250));
                            }
                        }

                        if (diff.Trend == TrendDirection.Up)
                        {
                            ws.Cells[row, 4, row, 5].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                            ws.Cells[row, 4, row, 5].Style.Font.Bold = true;
                        }
                        else if (diff.Trend == TrendDirection.Down)
                        {
                            ws.Cells[row, 4, row, 5].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                            ws.Cells[row, 4, row, 5].Style.Font.Bold = true;
                        }
                        row++;
                    }
                }

                ws.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        #endregion
    }
}
