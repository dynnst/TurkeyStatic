using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string[] files = {
            @""C:\Users\fb\Downloads\Istatistik\Istatistik\Istatistik\Views\Bureau\Index.cshtml"",
            @""C:\Users\fb\Downloads\Istatistik\Istatistik\Istatistik\Views\Home\DataEntry.cshtml"",
            @""C:\Users\fb\Downloads\Istatistik\Istatistik\Istatistik\Views\Passport\Index.cshtml""
        };
        
        string bad = ""\uFFFD"";
        
        var dict = new System.Collections.Generic.Dictionary<string, string> {
            { ""Veri Giri"" + bad + ""ii"", ""Veri Girişi"" },
            { ""Veri Giri"" + bad + ""i"", ""Veri Girişi"" },
            { ""B"" + bad + ""ro Se"" + bad + ""imi"", ""Büro Seçimi"" },
            { ""Havaliman"" + bad + "" bilginiz"", ""Havalimanı bilginiz"" },
            { ""Havaliman"" + bad + ""nda"", ""havalimanında"" },
            { ""tan"" + bad + ""ml"" + bad + "" de"" + bad + ""il"", ""tanımlı değil"" },
            { ""y"" + bad + ""zden b"" + bad + ""ro"", ""yüzden büro"" },
            { ""atanm"" + bad + ""al"" + bad + ""d"" + bad + ""r"", ""atanmalıdır"" },
            { ""atanm"" + bad + ""al\u00FDD"" + bad + ""r"", ""atanmalıdır"" },
            { ""atanm"" + bad + "" bir"", ""atanmış bir"" },
            { ""Y"" + bad + ""netimi ekran"" + bad + ""ndan"", ""Yönetimi ekranından"" },
            { ""atanmal"" + bad + ""d"" + bad + ""r"", ""atanmalıdır"" },
            { ""girece"" + bad + ""iniz b"" + bad + ""royu se"" + bad + ""in"", ""gireceğiniz büroyu seçin"" },
            { ""Veri Giri"" + bad + """", ""Veri Girişi"" },
            { ""B"" + bad + ""ro"", ""Büro"" },
            { ""b"" + bad + ""ro"", ""büro"" },
            { ""atanm"" + bad + """", ""atanmış"" },
            { ""Su"" + bad + "" "" + bad + ""nleme"", ""Suç Önleme"" },
            { ""G"" + bad + ""nl"" + bad + ""k"", ""Günlük"" },
            { ""Pasaport B"" + bad + ""rosu"", ""Pasaport Bürosu"" },
            { ""T"" + bad + ""r"" + bad + """", ""Türü"" },
            { ""Kay"" + bad + ""t"", ""Kayıt"" },
            { ""Ã§"", ""ç"" },
            { ""Ã¼"", ""ü"" },
            { ""Ã¶"", ""ö"" },
            { ""ÅŸ"", ""ş"" },
            { ""Ä±"", ""ı"" },
            { ""ÄŸ"", ""ğ"" },
            { ""Ã‡"", ""Ç"" },
            { ""Ãœ"", ""Ü"" },
            { ""Ã–"", ""Ö"" },
            { ""Åž"", ""Ş"" },
            { ""Ä°"", ""İ"" },
            { ""Äž"", ""Ğ"" }
        };

        foreach (var f in files)
        {
            if (!File.Exists(f)) continue;
            string content = File.ReadAllText(f, Encoding.UTF8);
            foreach (var kvp in dict)
            {
                content = content.Replace(kvp.Key, kvp.Value);
            }
            File.WriteAllText(f, content, new UTF8Encoding(true));
        }
    }
}