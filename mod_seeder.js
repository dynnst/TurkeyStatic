const fs = require('fs');
let code = fs.readFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Services/BureauSeeder.cs', 'utf8');

code = code.replace(/private static readonly string\[\]\[\] Defaults =[\s\S]*?db\.SaveChanges\(\);\s*\}/, 
`private static readonly string[][] Defaults =
        {
            new[] { BureauCodes.Pasaport, "Pasaport Bürosu" },
            new[] { BureauCodes.SucOnleme, "Suç Önleme Bürosu" },
            new[] { "IDARI", "İdari Büro" },
            new[] { BureauCodes.Trafik, "Trafik Bürosu" },
            new[] { BureauCodes.BilgiTeknolojileri, "Bilgi Teknolojileri Büro Amirliği" },
            new[] { BureauCodes.GbtUyap, "GBT ve UYAP Sorgulama Faaliyetleri" },
            new[] { BureauCodes.YtsSorgu, "YTS Sorgu Sayıları" },
            new[] { BureauCodes.SeyahatBelgeRisk, "Seyahat Belgeleri ve Risk Analizi" }
        };

        public static void EnsureForBorder(IstatistikContext db, string border)
        {
            if (string.IsNullOrWhiteSpace(border)) return;

            var existingCodes = db.Bureaus.Where(b => b.Border == border).Select(b => b.Code).ToList();

            foreach (var d in Defaults)
            {
                if (!existingCodes.Contains(d[0]))
                {
                    db.Bureaus.Add(new Bureau { Border = border, Code = d[0], Name = d[1], IsActive = true });
                }
            }

            db.SaveChanges();
        }`);

fs.writeFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Services/BureauSeeder.cs', code, 'utf8');
