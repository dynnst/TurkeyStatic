using Istatistik.Models;
using System;
using System.Linq;

namespace Istatistik.Services
{
    public class DataService
    {
        private readonly string _border;
        private readonly IstatistikContext _db;

        public DataService(string border)
        {
            _border = border;
            _db = new IstatistikContext();
        }

        public object Get(string type, int? id, int? year, int? month)
        {
            if (id.HasValue && id > 0)
            {
                if (type == "bilgitek") return _db.BilgiTeknolojileriIstatistikleri.FirstOrDefault(x => x.Id == id && x.Border == _border);
                if (type == "cctv") return _db.CctvIstatistikleri.FirstOrDefault(x => x.Id == id && x.Border == _border);
                if (type == "trafik") return _db.TrafikIstatistikleri.FirstOrDefault(x => x.Id == id && x.Border == _border);
                if (type == "gbtuyap") return _db.GbtUyapSorgulari.FirstOrDefault(x => x.Id == id && x.Border == _border);
                if (type == "suconleme") return _db.SucOnlemeIcmallari.FirstOrDefault(x => x.Id == id && x.Border == _border);
                if (type == "ytssorgu") return _db.YtsSorgulari.FirstOrDefault(x => x.Id == id && x.Border == _border);
                if (type == "seyahat") return _db.SeyahatBelgesiRiskAnalizleri.FirstOrDefault(x => x.Id == id && x.Border == _border);
            }

            var qYil = year ?? DateTime.Today.Year;
            var qAy = month ?? 0;

            if (type == "bilgitek") return Query(_db.BilgiTeknolojileriIstatistikleri, qYil, qAy);
            if (type == "cctv") return Query(_db.CctvIstatistikleri, qYil, qAy);
            if (type == "trafik") return Query(_db.TrafikIstatistikleri, qYil, qAy);
            if (type == "gbtuyap") return Query(_db.GbtUyapSorgulari, qYil, qAy);
            if (type == "suconleme") return Query(_db.SucOnlemeIcmallari, qYil, qAy);
            if (type == "ytssorgu") return Query(_db.YtsSorgulari, qYil, qAy);
            if (type == "seyahat") return Query(_db.SeyahatBelgesiRiskAnalizleri, qYil, qAy);

            throw new Exception("Geçersiz veri tipi.");
        }

        private object Query<T>(IQueryable<T> set, int year, int month) where T : class
        {
            // Reflection for basic querying, or dynamic LINQ.
            // But since all our models have Border and Tarih:
            var param = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var borderProp = System.Linq.Expressions.Expression.Property(param, "Border");
            var borderVal = System.Linq.Expressions.Expression.Constant(_border);
            var borderEq = System.Linq.Expressions.Expression.Equal(borderProp, borderVal);

            var tarihProp = System.Linq.Expressions.Expression.Property(param, "Tarih");
            var yearProp = System.Linq.Expressions.Expression.Property(tarihProp, "Year");
            var yearVal = System.Linq.Expressions.Expression.Constant(year);
            var yearEq = System.Linq.Expressions.Expression.Equal(yearProp, yearVal);

            var filter = System.Linq.Expressions.Expression.AndAlso(borderEq, yearEq);

            if (month > 0)
            {
                var monthProp = System.Linq.Expressions.Expression.Property(tarihProp, "Month");
                var monthVal = System.Linq.Expressions.Expression.Constant(month);
                var monthEq = System.Linq.Expressions.Expression.Equal(monthProp, monthVal);
                filter = System.Linq.Expressions.Expression.AndAlso(filter, monthEq);
            }

            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(filter, param);
            
            // Default order by Tarih desc
            var orderByExp = System.Linq.Expressions.Expression.Lambda<Func<T, DateTime>>(tarihProp, param);

            return set.Where(lambda).OrderByDescending(orderByExp).Take(500).ToList();
        }

        public object Save(string type, string payload)
        {
            if (type == "bilgitek") return SaveEntity<BilgiTeknolojileriIstatistik>(payload, _db.BilgiTeknolojileriIstatistikleri);
            if (type == "cctv") return SaveEntity<CctvIstatistik>(payload, _db.CctvIstatistikleri);
            if (type == "trafik") return SaveEntity<TrafikIstatistik>(payload, _db.TrafikIstatistikleri);
            if (type == "gbtuyap") return SaveEntity<GbtUyapSorgu>(payload, _db.GbtUyapSorgulari);
            if (type == "suconleme") return SaveEntity<SucOnlemeIcmal>(payload, _db.SucOnlemeIcmallari);
            if (type == "ytssorgu") return SaveEntity<YtsSorgu>(payload, _db.YtsSorgulari);
            if (type == "seyahat") return SaveEntity<SeyahatBelgesiRiskAnaliz>(payload, _db.SeyahatBelgesiRiskAnalizleri);

            throw new Exception("Geçersiz veri tipi.");
        }

        private object SaveEntity<T>(string payload, System.Data.Entity.DbSet<T> set) where T : class, new()
        {
            var m = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(payload);
            var idProp = typeof(T).GetProperty("Id");
            int id = (int)idProp.GetValue(m);

            T e;
            if (id == 0)
            {
                e = new T();
                typeof(T).GetProperty("Border").SetValue(e, _border);
                set.Add(e);
            }
            else
            {
                var pId = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
                var cId = System.Linq.Expressions.Expression.Constant(id);
                var eqId = System.Linq.Expressions.Expression.Equal(System.Linq.Expressions.Expression.Property(pId, "Id"), cId);
                
                var pBorder = System.Linq.Expressions.Expression.Constant(_border);
                var eqBorder = System.Linq.Expressions.Expression.Equal(System.Linq.Expressions.Expression.Property(pId, "Border"), pBorder);

                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(System.Linq.Expressions.Expression.AndAlso(eqId, eqBorder), pId);
                
                e = set.FirstOrDefault(lambda);
                if (e == null) throw new Exception("Kayıt bulunamadı.");
            }

            // Copy properties explicitly from m to e (except Id, Border)
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.Name == "Id" || prop.Name == "Border") continue;
                
                // Ensure DateTimes are .Date only if they are Dates, but usually we just copy.
                if (prop.PropertyType == typeof(DateTime))
                {
                    DateTime dt = (DateTime)prop.GetValue(m);
                    prop.SetValue(e, dt.Date);
                }
                else
                {
                    prop.SetValue(e, prop.GetValue(m));
                }
            }

            _db.SaveChanges();
            return new { Id = idProp.GetValue(e) };
        }

        public void Delete(string type, int id)
        {
            if (type == "bilgitek") DeleteEntity(_db.BilgiTeknolojileriIstatistikleri, id);
            else if (type == "cctv") DeleteEntity(_db.CctvIstatistikleri, id);
            else if (type == "trafik") DeleteEntity(_db.TrafikIstatistikleri, id);
            else if (type == "gbtuyap") DeleteEntity(_db.GbtUyapSorgulari, id);
            else if (type == "suconleme") DeleteEntity(_db.SucOnlemeIcmallari, id);
            else if (type == "ytssorgu") DeleteEntity(_db.YtsSorgulari, id);
            else if (type == "seyahat") DeleteEntity(_db.SeyahatBelgesiRiskAnalizleri, id);
            else throw new Exception("Geçersiz veri tipi.");
            
            _db.SaveChanges();
        }

        private void DeleteEntity<T>(System.Data.Entity.DbSet<T> set, int id) where T : class
        {
            var pId = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var cId = System.Linq.Expressions.Expression.Constant(id);
            var eqId = System.Linq.Expressions.Expression.Equal(System.Linq.Expressions.Expression.Property(pId, "Id"), cId);
            
            var pBorder = System.Linq.Expressions.Expression.Constant(_border);
            var eqBorder = System.Linq.Expressions.Expression.Equal(System.Linq.Expressions.Expression.Property(pId, "Border"), pBorder);

            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(System.Linq.Expressions.Expression.AndAlso(eqId, eqBorder), pId);
            
            var e = set.FirstOrDefault(lambda);
            if (e == null) throw new Exception("Kayıt bulunamadı.");
            set.Remove(e);
        }
    }
}
