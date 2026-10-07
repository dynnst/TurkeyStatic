DECLARE @border NVARCHAR(MAX) = (SELECT TOP 1 fldBorder FROM CEZA.dbo.E_User WHERE fldSicil = '313258');

INSERT INTO SucOnlemeIcmals (Border, Tarih, SucTuru, VakaSayisi)
VALUES 
(@border, '2026-10-06', N'Hırsızlık', 5),
(@border, '2026-10-07', N'Gasp', 2),
(@border, '2026-10-07', N'Dolandırıcılık', 4),
(@border, '2026-10-07', N'Yaralama', 3),
(@border, '2026-10-07', N'Cinayet', 1);

INSERT INTO TrafikIstatistiks (Border, Tarih, KontrolEdilenAracSayisi, CezaYazilanSurucuSayisi, CezaTutari, TrafiktenMenEdilenAracSayisi, GeciciGeriAlinanSurucuBelgesi)
VALUES
(@border, '2026-10-06', 150, 20, 15000, 2, 1),
(@border, '2026-10-07', 200, 35, 25000, 5, 3),
(@border, '2026-10-07', 100, 10, 8000, 0, 0),
(@border, '2026-10-07', 300, 50, 45000, 8, 4),
(@border, '2026-10-07', 50, 5, 3000, 1, 0);
