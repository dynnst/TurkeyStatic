using Istatistik.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Istatistik.Services
{
    /// <summary>
    /// Pasaport istatistikleri. Tüm sorgu ve yazma işlemleri oturumdaki havalimanına (Border) kilitlidir.
    /// </summary>
    public partial class PassportService
    {
        private static readonly string[] DateFormats = { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd", "dd/MM/yyyy" };
        private static readonly CultureInfo Tr = new CultureInfo("tr-TR");

        private readonly IstatistikContext _db;
        private readonly CurrentUser _user;
        private readonly string _border;

        public PassportService(IstatistikContext db, CurrentUser user, string borderOverride = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _user = user ?? throw new ArgumentNullException(nameof(user));

            // Yalnızca SuperAdmin başka bir havalimanı seçebilir
            _border = _user.IsSuperAdmin && !string.IsNullOrWhiteSpace(borderOverride)
                ? borderOverride.Trim()
                : _user.Border;

            if (string.IsNullOrWhiteSpace(_border))
                throw new UnauthorizedAccessException("Havalimanı bilginiz tanımlı değil.");

            if (!_user.CanAccessBureau(BureauCodes.Pasaport))
                throw new UnauthorizedAccessException("Pasaport bürosu için yetkiniz yok.");
        }

        public string Border => _border;

        #region Analitik

        public List<YolcuUcakIstatistik> GetMonthlyPassengerStats(int year, HatTuru? flightType)
        {
            var q = _db.YolcuUcakIstatistikleri.Where(x => x.Border == _border && x.Yil == year);
            if (flightType.HasValue)
                q = q.Where(x => x.HatTuru == flightType.Value);
            return q.OrderBy(x => x.Ay).ToList();
        }

        public List<GunlukZamanSerisiYolcu> GetDailyTimeSeries(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("Bitiş tarihi başlangıç tarihinden küçük olamaz.");

            var s = startDate.Date;
            var e = endDate.Date;
            return _db.GunlukZamanSerisiYolcular
                .Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e)
                .OrderBy(x => x.Tarih)
                .ToList();
        }

        public List<object> GetInadSummaryByNationality()
        {
            return _db.InadYolcular
                .Where(i => i.Border == _border)
                .GroupBy(i => new { i.Uyruk, i.InadGerekcesi })
                .Select(g => new { Uyruk = g.Key.Uyruk, Gerekce = g.Key.InadGerekcesi, Adet = g.Count() })
                .OrderByDescending(x => x.Adet)
                .ToList()
                .Cast<object>()
                .ToList();
        }

        public List<TahditKayit> GetTahditRecords(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return new List<TahditKayit>();

            var q = searchQuery.Trim();
            return _db.TahditKayitlari
                .Where(x => x.Border == _border &&
                    (x.AdSoyad.Contains(q) || x.PasaportVeyaKimlikNo.Contains(q) || x.TahditKodu.Contains(q)))
                .OrderByDescending(x => x.Tarih)
                .Take(200)
                .ToList();
        }

        #endregion

        #region Ingestion

        /// <summary>
        /// CSV: Yil;Ay;HatTuru;GelenYolcu;GidenYolcu;GelenUcak;GidenUcak (ilk satır başlık)
        /// </summary>
        public int ImportYolcuUcakCsv(Stream stream, char delimiter = ';')
        {
            var list = new List<YolcuUcakIstatistik>();
            foreach (var cols in ReadCsv(stream, delimiter))
            {
                if (cols.Length < 7) continue;

                var yil = ParseInt(cols[0]);
                var ay = ParseInt(cols[1]);
                if (yil < 2000 || yil > 2100 || ay < 1 || ay > 12) continue;

                var gy = ParseInt(cols[3]);
                var by = ParseInt(cols[4]);
                var gu = ParseInt(cols[5]);
                var bu = ParseInt(cols[6]);

                list.Add(new YolcuUcakIstatistik
                {
                    Border = _border,
                    Yil = yil,
                    Ay = ay,
                    HatTuru = ParseHatTuru(cols[2]),
                    GelenYolcu = gy,
                    GidenYolcu = by,
                    ToplamYolcu = gy + by,
                    GelenUcak = gu,
                    GidenUcak = bu,
                    ToplamUcak = gu + bu
                });
            }
            return Save(_db.YolcuUcakIstatistikleri, list);
        }

        /// <summary>
        /// CSV: Tarih;Yon;HatTuru;GunlukYolcu;Kumulatif;OnAylikToplam;UcakSayisi(opsiyonel) (ilk satır başlık)
        /// </summary>
        public int ImportGunlukCsv(Stream stream, char delimiter = ';')
        {
            var list = new List<GunlukZamanSerisiYolcu>();
            foreach (var cols in ReadCsv(stream, delimiter))
            {
                if (cols.Length < 6) continue;
                if (!TryParseDate(cols[0], out var tarih)) continue;

                list.Add(new GunlukZamanSerisiYolcu
                {
                    Border = _border,
                    Tarih = tarih,
                    Yil = tarih.Year,
                    Ay = tarih.Month,
                    Gun = tarih.Day,
                    Yon = cols[1].Trim().ToLower(Tr).StartsWith("gid") ? Yon.Giden : Yon.Gelen,
                    HatTuru = ParseHatTuru(cols[2]),
                    GunlukYolcuSayisi = ParseInt(cols[3]),
                    UcakSayisi = cols.Length > 6 ? ParseInt(cols[6]) : 0,
                    GunlukKumulatifToplam = ParseInt(cols[4]),
                    OnAylikToplam = ParseInt(cols[5])
                });
            }
            return Save(_db.GunlukZamanSerisiYolcular, list);
        }

        public int ImportInadJson(Stream stream)
        {
            var list = ReadJson<InadYolcu>(stream)
                .Where(i => i.Tarih != DateTime.MinValue)
                .ToList();
            foreach (var i in list)
            {
                i.Id = 0;
                i.Border = _border;
                if (i.DogumTarihi == DateTime.MinValue) i.DogumTarihi = null;
                if (i.GelisTarihi == DateTime.MinValue) i.GelisTarihi = null;
                if (i.GidisTarihi == DateTime.MinValue) i.GidisTarihi = null;
            }
            return Save(_db.InadYolcular, list);
        }

        public int ImportTahditJson(Stream stream)
        {
            var list = ReadJson<TahditKayit>(stream)
                .Where(t => t.Tarih != DateTime.MinValue)
                .ToList();
            foreach (var t in list)
            {
                t.Id = 0;
                t.Border = _border;
                if (t.DogumTarihi == DateTime.MinValue) t.DogumTarihi = null;
            }
            return Save(_db.TahditKayitlari, list);
        }

        public int ImportHaftalikJson(Stream stream)
        {
            var list = ReadJson<HaftalikOlayCizelgesi>(stream);
            foreach (var h in list)
            {
                h.Id = 0;
                h.Border = _border;
                if (h.BaslangicTarihi == DateTime.MinValue) h.BaslangicTarihi = null;
                if (h.BitisTarihi == DateTime.MinValue) h.BitisTarihi = null;
                if (string.IsNullOrWhiteSpace(h.TarihAraligi) && h.BaslangicTarihi.HasValue && h.BitisTarihi.HasValue)
                    h.TarihAraligi = h.BaslangicTarihi.Value.ToString("dd.MM.yyyy") + " - " + h.BitisTarihi.Value.ToString("dd.MM.yyyy");
            }
            return Save(_db.HaftalikOlayCizelgeleri, list);
        }

        #endregion

        #region Helpers

        private int Save<T>(System.Data.Entity.DbSet<T> set, List<T> items) where T : class
        {
            if (items.Count == 0) return 0;
            set.AddRange(items);
            _db.SaveChanges();
            return items.Count;
        }

        private static List<T> ReadJson<T>(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<List<T>>(sr.ReadToEnd()) ?? new List<T>();
            }
        }

        private static IEnumerable<string[]> ReadCsv(Stream stream, char delimiter)
        {
            using (var sr = new StreamReader(stream, System.Text.Encoding.UTF8, true))
            {
                sr.ReadLine(); // başlık
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    yield return line.Split(delimiter).Select(c => c.Trim().Trim('"')).ToArray();
                }
            }
        }

        public static int ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            var cleaned = s.Trim().Replace(".", "").Replace(",", "").Replace(" ", "");
            return int.TryParse(cleaned, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;
        }

        public static bool TryParseDate(string s, out DateTime date)
        {
            return DateTime.TryParseExact(s?.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        public static HatTuru ParseHatTuru(string s)
        {
            var v = (s ?? "").Trim().ToLower(Tr);
            return v.StartsWith("iç") || v.StartsWith("ic") ? HatTuru.Ic : HatTuru.Dis;
        }

        #endregion
    }
}
