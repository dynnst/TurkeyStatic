using Istatistik.Models;
using System.Linq;

namespace Istatistik.Services
{
    public static class BureauSeeder
    {
        private static readonly string[][] Defaults =
        {
            new[] { BureauCodes.Pasaport, "Pasaport Bürosu" },
            new[] { "SUC_ONLEME", "Suç Önleme Bürosu" },
            new[] { "IDARI", "İdari Büro" },
            new[] { "TRAFIK", "Trafik Bürosu" }
        };

        /// <summary>
        /// Havalimanında hiç büro yoksa varsayılan büroları oluşturur.
        /// Sonradan pasifleştirilen veya silinen bürolar tekrar eklenmez.
        /// </summary>
        public static void EnsureForBorder(IstatistikContext db, string border)
        {
            if (string.IsNullOrWhiteSpace(border)) return;
            if (db.Bureaus.Any(b => b.Border == border)) return;

            foreach (var d in Defaults)
                db.Bureaus.Add(new Bureau { Border = border, Code = d[0], Name = d[1], IsActive = true });

            db.SaveChanges();
        }
    }
}
