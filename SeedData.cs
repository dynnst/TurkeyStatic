using System;
using System.Linq;
using Istatistik.Models;

namespace Istatistik.Seeder
{
    class Program
    {
        static void Main()
        {
            using (var db = new IstatistikContext())
            {
                var border = "ANTALYA GAZİPAŞA HAVA LİMANI";

                // 1. CCTV (KAPALI DEVRE TV) SAYILARI
                var c1 = new CctvIstatistik { Border = border, Tarih = new DateTime(2026, 9, 1), Bolge = "Terminal Dışı", IpSabit = 19, AnalogSabit = 2, IpHareketli = 4, AnalogHareketli = 1 };
                var c2 = new CctvIstatistik { Border = border, Tarih = new DateTime(2026, 9, 1), Bolge = "Terminal İçi", IpSabit = 41, AnalogSabit = 47, IpHareketli = 8, AnalogHareketli = 0 };
                var c3 = new CctvIstatistik { Border = border, Tarih = new DateTime(2026, 9, 1), Bolge = "Pist-Apron", IpSabit = 88, AnalogSabit = 18, IpHareketli = 15, AnalogHareketli = 0 };

                if (!db.CctvIstatistikleri.Any(x => x.Bolge == "Terminal Dışı"))
                {
                    db.CctvIstatistikleri.Add(c1);
                    db.CctvIstatistikleri.Add(c2);
                    db.CctvIstatistikleri.Add(c3);
                }

                // 2. BİLGİ TEKNOLOJİLERİ
                // 2025 Eylül
                var b2025 = new BilgiTeknolojileriIstatistik
                {
                    Border = border, Tarih = new DateTime(2025, 9, 1),
                    KameraKaydiIncelemesi = 0, // PPTX: 2025 Eylül Ayı -> 0
                    TahditBakilanSorunluYolcu = 1381,
                    YurdaGirisCikisBelgeTalebi = 24,
                    TahditEkleme = 69,
                    TahditKaldirma = 47,
                    PtsAracAraniyor = 0, PtsAracCalinti = 0, PtsPlakaCalinti = 0, PtsPlakaKayip = 0
                };
                
                // 2026 Eylül
                var b2026 = new BilgiTeknolojileriIstatistik
                {
                    Border = border, Tarih = new DateTime(2026, 9, 1),
                    KameraKaydiIncelemesi = 3, // PPTX: 2026 Eylül Ayı -> 3
                    TahditBakilanSorunluYolcu = 991,
                    YurdaGirisCikisBelgeTalebi = 2,
                    TahditEkleme = 49,
                    TahditKaldirma = 67,
                    PtsAracAraniyor = 0, PtsAracCalinti = 0, PtsPlakaCalinti = 0, PtsPlakaKayip = 0
                };

                if (!db.BilgiTeknolojileriIstatistikleri.Any(x => x.Tarih.Year == 2025 && x.Tarih.Month == 9))
                {
                    db.BilgiTeknolojileriIstatistikleri.Add(b2025);
                }
                if (!db.BilgiTeknolojileriIstatistikleri.Any(x => x.Tarih.Year == 2026 && x.Tarih.Month == 9))
                {
                    db.BilgiTeknolojileriIstatistikleri.Add(b2026);
                }

                db.SaveChanges();
                Console.WriteLine("Data seeded successfully!");
            }
        }
    }
}
