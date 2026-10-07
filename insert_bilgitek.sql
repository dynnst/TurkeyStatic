DECLARE @border NVARCHAR(MAX) = (SELECT TOP 1 fldBorder FROM CEZA.dbo.E_User WHERE fldSicil = '313258');

INSERT INTO BilgiTeknolojileriIstatistiks (
    Border, Tarih, KameraKaydiIncelemesi, PtsAracAraniyor, PtsAracCalinti, PtsPlakaCalinti, PtsPlakaKayip, 
    TahditBakilanSorunluYolcu, YurdaGirisCikisBelgeTalebi, TahditEkleme, TahditKaldirma
)
VALUES 
(@border, '2026-10-06', 12, 2, 0, 1, 3, 50, 15, 4, 2),
(@border, '2026-10-07', 8, 1, 1, 0, 2, 45, 10, 3, 1),
(@border, '2026-10-07', 15, 0, 0, 0, 1, 60, 20, 5, 5),
(@border, '2026-10-07', 5, 3, 0, 2, 0, 30, 8, 2, 0),
(@border, '2026-10-07', 20, 5, 2, 1, 4, 80, 25, 10, 8);
