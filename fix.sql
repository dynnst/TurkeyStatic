DELETE FROM SucOnlemeIcmals;
DELETE FROM TrafikIstatistiks;
DELETE FROM BilgiTeknolojileriIstatistiks;

DECLARE @border NVARCHAR(MAX) = (SELECT TOP 1 fldBorder FROM CEZA.dbo.E_User WHERE fldSicil = '313258');

INSERT INTO SucOnlemeIcmals (Border, Tarih, SucTuru, VakaSayisi)
VALUES 
(@border, '2026-10-03', N'Genel Asayiş', 5),
(@border, '2026-10-04', N'Genel Asayiş', 2),
(@border, '2026-10-05', N'Genel Asayiş', 4),
(@border, '2026-10-06', N'Genel Asayiş', 3),
(@border, '2026-10-07', N'Genel Asayiş', 8);

INSERT INTO TrafikIstatistiks (Border, Tarih, KontrolEdilenAracSayisi, CezaYazilanSurucuSayisi, CezaTutari, TrafiktenMenEdilenAracSayisi, GeciciGeriAlinanSurucuBelgesi)
VALUES
(@border, '2026-10-03', 150, 20, 15000, 2, 1),
(@border, '2026-10-04', 200, 35, 25000, 5, 3),
(@border, '2026-10-05', 100, 10, 8000, 0, 0),
(@border, '2026-10-06', 300, 50, 45000, 8, 4),
(@border, '2026-10-07', 250, 25, 18000, 3, 2);

INSERT INTO BilgiTeknolojileriIstatistiks (Border, Tarih, KameraKaydiIncelemesi, PtsAracAraniyor, PtsAracCalinti, PtsPlakaCalinti, PtsPlakaKayip, TahditBakilanSorunluYolcu, YurdaGirisCikisBelgeTalebi, TahditEkleme, TahditKaldirma)
VALUES 
(@border, '2026-10-03', 12, 2, 0, 1, 3, 50, 15, 4, 2),
(@border, '2026-10-04', 8, 1, 1, 0, 2, 45, 10, 3, 1),
(@border, '2026-10-05', 15, 0, 0, 0, 1, 60, 20, 5, 5),
(@border, '2026-10-06', 5, 3, 0, 2, 0, 30, 8, 2, 0),
(@border, '2026-10-07', 20, 5, 2, 1, 4, 80, 25, 10, 8);
