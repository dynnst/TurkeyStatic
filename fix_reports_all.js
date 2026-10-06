const fs = require('fs');
const path = require('path');

const basePath = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik';
const reportsViewPath = path.join(basePath, 'Views/Reports/Index.cshtml');
const reportServicePath = path.join(basePath, 'Services/ReportService.cs');

// 1. Update Views/Reports/Index.cshtml
let viewContent = fs.readFileSync(reportsViewPath, 'utf8');
viewContent = viewContent.replace(/<select id="dataType" class="form-select">([\s\S]*?)<\/select>/, 
`<select id="dataType" class="form-select">
    <optgroup label="Pasaport">
        <option value="yolcuucak">Yolcu/Uçak İstatistikleri</option>
        <option value="gunluk">Günlük Zaman Serisi</option>
        <option value="inad">İNAD Kayıtları</option>
        <option value="tahdit">Tahdit Kayıtları</option>
        <option value="haftalik">Haftalık Olay Çizelgesi</option>
    </optgroup>
    <optgroup label="Bilgi Teknolojileri">
        <option value="bilgitek_KameraKaydiIncelemesi">Kamera Kaydı İncelemesi</option>
        <option value="bilgitek_PtsAracAraniyor">PTS Araç Aranıyor</option>
        <option value="bilgitek_PtsAracCalinti">PTS Araç Çalıntı</option>
        <option value="bilgitek_PtsPlakaCalinti">PTS Plaka Çalıntı</option>
        <option value="bilgitek_PtsPlakaKayip">PTS Plaka Kayıp</option>
        <option value="bilgitek_TahditBakilanSorunluYolcu">Tahdit Bakılan Sorunlu Yolcu</option>
        <option value="bilgitek_YurdaGirisCikisBelgeTalebi">Giriş/Çıkış Belge Talebi</option>
        <option value="bilgitek_TahditEkleme">Tahdit Ekleme</option>
        <option value="bilgitek_TahditKaldirma">Tahdit Kaldırma</option>
    </optgroup>
    <optgroup label="CCTV">
        <option value="cctv_IpSabit">IP Sabit Kamera</option>
        <option value="cctv_IpHareketli">IP Hareketli Kamera</option>
        <option value="cctv_AnalogSabit">Analog Sabit Kamera</option>
        <option value="cctv_AnalogHareketli">Analog Hareketli Kamera</option>
    </optgroup>
    <optgroup label="Trafik">
        <option value="trafik_KontrolEdilenAracSayisi">Kontrol Edilen Araç</option>
        <option value="trafik_CezaYazilanSurucuSayisi">Ceza Yazılan Sürücü</option>
        <option value="trafik_TrafiktenMenEdilenAracSayisi">Trafikten Men Edilen Araç</option>
        <option value="trafik_GeciciGeriAlinanSurucuBelgesi">Geri Alınan Sürücü Belgesi</option>
    </optgroup>
    <optgroup label="GBT / UYAP">
        <option value="gbtuyap_SorgulananKisiSayisi">Sorgulanan Kişi</option>
        <option value="gbtuyap_YakalananKisiSayisi">Yakalanan Kişi</option>
    </optgroup>
    <optgroup label="YTS Sorgu">
        <option value="ytssorgu_GunlukSorguSayisi">Günlük Sorgu Sayısı</option>
    </optgroup>
</select>`);

fs.writeFileSync(reportsViewPath, viewContent, 'utf8');

// 2. Update ReportService.cs
let serviceContent = fs.readFileSync(reportServicePath, 'utf8');

// Fix the routing in GetAggregatedData
serviceContent = serviceContent.replace(
    /if \(dataType\.StartsWith\("bilgitek_", StringComparison\.OrdinalIgnoreCase\)\)[\s\S]*?else\s*\{/,
    `if (dataType.Contains("_"))
            {
                result.DataPoints = AggregateDynamic(startDate, endDate, periodType, dataType);
            }
            else
            {`
);

// We need to replace the `AggregateBilgiTek` with `AggregateDynamic` that can handle any entity.
// Let's remove the previously injected `AggregateBilgiTek` region if it exists
serviceContent = serviceContent.replace(/#region Bilgi Teknolojileri[\s\S]*?#endregion\s*#region İNAD/, '#region İNAD');

const aggregateDynamicCode = `
        #region Dinamik Raporlama

        private List<AggregatedDataPoint> AggregateDynamic(DateTime start, DateTime end, PeriodType periodType, string dataType)
        {
            var s = start.Date;
            var e = end.Date;
            var parts = dataType.Split('_');
            var prefix = parts[0].ToLowerInvariant();
            var propName = parts[1];

            List<RawDataPoint> raw = new List<RawDataPoint>();

            if (prefix == "bilgitek")
            {
                var records = _db.BilgiTeknolojileriIstatistiks.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e).ToList();
                var prop = typeof(BilgiTeknolojileriIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null) raw = records.Select(x => new RawDataPoint { Tarih = x.Tarih, Value = (int)prop.GetValue(x) }).ToList();
            }
            else if (prefix == "cctv")
            {
                var records = _db.CctvIstatistiks.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e).ToList();
                var prop = typeof(CctvIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null) raw = records.Select(x => new RawDataPoint { Tarih = x.Tarih, Value = (int)prop.GetValue(x) }).ToList();
            }
            else if (prefix == "trafik")
            {
                var records = _db.TrafikIstatistiks.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e).ToList();
                var prop = typeof(TrafikIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null) raw = records.Select(x => new RawDataPoint { Tarih = x.Tarih, Value = (int)prop.GetValue(x) }).ToList();
            }
            else if (prefix == "gbtuyap")
            {
                var records = _db.GbtUyapSorgus.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e).ToList();
                var prop = typeof(GbtUyapSorgu).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null) raw = records.Select(x => new RawDataPoint { Tarih = x.Tarih, Value = (int)prop.GetValue(x) }).ToList();
            }
            else if (prefix == "ytssorgu")
            {
                var records = _db.YtsSorgus.Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e).ToList();
                var prop = typeof(YtsSorgu).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null) raw = records.Select(x => new RawDataPoint { Tarih = x.Tarih, Value = (int)prop.GetValue(x) }).ToList();
            }

            return AggregateByPeriod(raw, periodType);
        }

        #endregion
`;

serviceContent = serviceContent.replace(/#region İNAD/, aggregateDynamicCode + '\n        #region İNAD');
fs.writeFileSync(reportServicePath, serviceContent, 'utf8');
console.log('Script completed.');
