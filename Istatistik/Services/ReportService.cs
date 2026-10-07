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
            if (result.DataPoints.Any())
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
                            ["ToplamYolcu"] = g.Sum(x => x.GunlukYolcuSayisi),
                            ["ToplamUcak"]  = g.Sum(x => x.UcakSayisi)
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
                            ["ToplamYolcu"] = g.Sum(x => x.GunlukYolcuSayisi),
                            ["ToplamUcak"]  = g.Sum(x => x.UcakSayisi)
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
                            ["ToplamYolcu"] = g.Sum(x => x.GunlukYolcuSayisi),
                            ["ToplamUcak"]  = g.Sum(x => x.UcakSayisi)
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
                            ["ToplamYolcu"] = g.Sum(x => x.GunlukYolcuSayisi),
                            ["ToplamUcak"]  = g.Sum(x => x.UcakSayisi)
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
            var s = start.Date;
            var e = end.Date;

            var raw = _db.TahditKayitlari
                .Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e)
                .ToList()
                .Select(x => new RawDataPoint { Tarih = x.Tarih, Value = 1 })
                .ToList();

            return AggregateByPeriod(raw, periodType);
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

            // Summary anahtarlarına göre fark metrikleri (her iki dönemdeki tüm anahtarlar)
            var allKeys = period1.Summary.Keys.Union(period2.Summary.Keys).Distinct();

            foreach (var key in allKeys)
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

                result.Differences.Add(new ComparisonMetric
                {
                    MetricName         = key,
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

        #endregion

        #region Export

        public byte[] ExportToCsv(AggregatedDataResult data, string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Tarih;Etiket;Değer");

            foreach (var point in data.DataPoints)
                sb.AppendLine(string.Format("{0};{1};{2}", point.Date.ToString("dd.MM.yyyy"), point.Label, point.Value));

            sb.AppendLine();
            sb.AppendLine("Özet");
            foreach (var kvp in data.Summary)
                sb.AppendLine(string.Format("{0};{1}", kvp.Key, kvp.Value));

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public byte[] ExportToExcel(AggregatedDataResult data, string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Rapor");

                ws.Cells[1, 1].Value = "Tarih";
                ws.Cells[1, 2].Value = "Etiket";
                ws.Cells[1, 3].Value = "Değer";

                int row = 2;
                foreach (var point in data.DataPoints)
                {
                    ws.Cells[row, 1].Value = point.Date.ToString("dd.MM.yyyy");
                    ws.Cells[row, 2].Value = point.Label;
                    ws.Cells[row, 3].Value = (double)point.Value;
                    row++;
                }

                row++;
                ws.Cells[row, 1].Value = "Özet";
                row++;
                foreach (var kvp in data.Summary)
                {
                    ws.Cells[row, 1].Value = kvp.Key;
                    ws.Cells[row, 2].Value = (double)kvp.Value;
                    row++;
                }

                ws.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        #endregion
    }
}
