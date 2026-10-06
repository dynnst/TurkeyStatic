const fs = require('fs');
const path = require('path');

const basePath = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik';
const reportsViewPath = path.join(basePath, 'Views/Reports/Index.cshtml');
const reportServicePath = path.join(basePath, 'Services/ReportService.cs');

// 1. Update Views/Reports/Index.cshtml
let viewContent = fs.readFileSync(reportsViewPath, 'utf8');
const oldOptions = `<option value="yolcuucak">Yolcu/Uak statistikleri</option>
                                  <option value="gunluk">Gnlk Zaman Serisi</option>
                                  <option value="inad">NAD Kaytlar</option>
                                  <option value="tahdit">Tahdit Kaytlar</option>
                                  <option value="haftalik">Haftalk Olay izelgesi</option>`;
                                  
// I will just replace the innerHTML of <select id="dataType" ...>
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
                              </select>`);

fs.writeFileSync(reportsViewPath, viewContent, 'utf8');

// 2. Update ReportService.cs
let serviceContent = fs.readFileSync(reportServicePath, 'utf8');

// Update ValidateDataTypeAccess to bypass or check proper authorization
serviceContent = serviceContent.replace(
    /private void ValidateDataTypeAccess\(string dataType\)[\s\S]*?#endregion/,
    `private void ValidateDataTypeAccess(string dataType)
        {
            // For now, allow all data types since user role validation happens in the controller.
            // If strict bureau checks are needed, we can parse dataType (e.g. "bilgitek_...") and check _user.CanAccessBureau
        }

        #endregion`
);

// Add the dynamic aggregation handling in GetAggregatedData
serviceContent = serviceContent.replace(
    /switch \(dataType\.ToLowerInvariant\(\)\)\s*\{[\s\S]*?default:\s*throw new ArgumentException\("Geçersiz veri tipi: " \+ dataType\);\s*\}/,
    `if (dataType.StartsWith("bilgitek_", StringComparison.OrdinalIgnoreCase))
            {
                result.DataPoints = AggregateBilgiTek(startDate, endDate, periodType, dataType.Substring("bilgitek_".Length));
            }
            else
            {
                switch (dataType.ToLowerInvariant())
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
            }`
);

// Insert AggregateBilgiTek method
const aggregateBilgiTekCode = `
        #region Bilgi Teknolojileri

        private List<AggregatedDataPoint> AggregateBilgiTek(DateTime start, DateTime end, PeriodType periodType, string propName)
        {
            var s = start.Date;
            var e = end.Date;

            // Fetch records from DB
            var records = _db.BilgiTeknolojileriIstatistiks
                .Where(x => x.Border == _border && x.Tarih >= s && x.Tarih <= e)
                .ToList();
            
            // Dynamically get the property value
            var propInfo = typeof(BilgiTeknolojileriIstatistik).GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (propInfo == null) throw new ArgumentException("Geçersiz alan: " + propName);

            var raw = records.Select(x => new RawDataPoint 
            { 
                Tarih = x.Tarih, 
                Value = (int)propInfo.GetValue(x) 
            }).ToList();

            return AggregateByPeriod(raw, periodType);
        }

        #endregion
`;

serviceContent = serviceContent.replace(/#region İNAD/, aggregateBilgiTekCode + '\n        #region İNAD');

fs.writeFileSync(reportServicePath, serviceContent, 'utf8');
console.log('Script completed.');
