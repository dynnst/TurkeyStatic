const fs = require('fs');

// 1. Update Authorization.cs
let authCode = fs.readFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Models/Authorization.cs', 'utf8');
authCode = authCode.replace('public const string Pasaport = "PASAPORT";', 
`public const string Pasaport = "PASAPORT";
        public const string Idari = "IDARI";
        public const string Guvenlik = "GUVENLIK";`);
fs.writeFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Models/Authorization.cs', authCode, 'utf8');

// 2. Update BureauSeeder.cs
let seederCode = fs.readFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Services/BureauSeeder.cs', 'utf8');
seederCode = seederCode.replace(/private static readonly string\[\]\[\] Defaults =[\s\S]*?public static void/, 
`private static readonly string[][] Defaults =
        {
            new[] { BureauCodes.Pasaport, "Pasaport Bürosu" },
            new[] { BureauCodes.Idari, "İdari Büro Amirliği" },
            new[] { BureauCodes.Guvenlik, "Güvenlik Hizmetleri Büro Amirliği" },
            new[] { BureauCodes.SucOnleme, "Suç Önleme Bürosu" },
            new[] { BureauCodes.Trafik, "Trafik Bürosu" },
            new[] { BureauCodes.BilgiTeknolojileri, "Bilgi Teknolojileri Büro Amirliği" },
            new[] { BureauCodes.GbtUyap, "GBT ve UYAP Sorgulama Faaliyetleri" },
            new[] { BureauCodes.YtsSorgu, "YTS Sorgu Sayıları" },
            new[] { BureauCodes.SeyahatBelgeRisk, "Seyahat Belgeleri ve Risk Analizi" }
        };

        public static void`);
fs.writeFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Services/BureauSeeder.cs', seederCode, 'utf8');

console.log("Updated auth and seeder.");
