DELETE FROM CrimeStatistics;
DELETE FROM CrimePreventionActivities;
DELETE FROM QueryStatistics;

INSERT INTO CrimeStatistics (UnitId, EntryDate, CrimeType, CrimeCode, CasesRequiringFollow_up, SuspectCount, ArrestedCount, JudicialControlCount, Notes, CreatedDate, CreatedBy)
VALUES 
(1, '2026-10-03', N'Hırsızlık', N'TCK141', 2, 3, 1, 0, N'Örnek kayıt', GETDATE(), N'System'),
(1, '2026-10-04', N'Gasp', N'TCK148', 1, 2, 2, 0, N'Örnek kayıt', GETDATE(), N'System'),
(1, '2026-10-05', N'Dolandırıcılık', N'TCK157', 5, 1, 0, 1, N'Örnek kayıt', GETDATE(), N'System'),
(1, '2026-10-06', N'Mala Zarar Verme', N'TCK151', 1, 1, 0, 0, N'Örnek kayıt', GETDATE(), N'System'),
(1, '2026-10-07', N'Tehdit', N'TCK106', 0, 1, 0, 0, N'Örnek kayıt', GETDATE(), N'System');

INSERT INTO CrimePreventionActivities (UnitId, EntryDate, WarrantSource, WarrantCount, ApprehendedCount, ApprehensionLocation, ArrestedCount, Notes, CreatedDate, CreatedBy)
VALUES 
(1, '2026-10-03', N'Mahkeme Kararı', 5, 2, N'Merkez', 1, N'Örnek faaliyet', GETDATE(), N'System'),
(1, '2026-10-04', N'Savcılık', 3, 3, N'Terminal Girişi', 2, N'Örnek faaliyet', GETDATE(), N'System'),
(1, '2026-10-05', N'İhbar', 1, 1, N'Otopark', 0, N'Örnek faaliyet', GETDATE(), N'System'),
(1, '2026-10-06', N'Rutin Kontrol', 4, 1, N'Kargo Bölümü', 1, N'Örnek faaliyet', GETDATE(), N'System'),
(1, '2026-10-07', N'Şok Uygulama', 2, 2, N'Nizamiye', 0, N'Örnek faaliyet', GETDATE(), N'System');

INSERT INTO QueryStatistics (UnitId, EntryDate, Shift, PersonQueriedCount, PersonArrestedSearchedCount, OperationType, Notes, CreatedDate, CreatedBy)
VALUES 
(1, '2026-10-03', N'08:00 - 20:00', 120, 2, N'Kimlik Kontrolü', N'Örnek sorgu', GETDATE(), N'System'),
(1, '2026-10-04', N'20:00 - 08:00', 85, 1, N'Asayiş Uygulaması', N'Örnek sorgu', GETDATE(), N'System'),
(1, '2026-10-05', N'08:00 - 17:00', 40, 0, N'Giriş Çıkış Kontrolü', N'Örnek sorgu', GETDATE(), N'System'),
(1, '2026-10-06', N'08:00 - 20:00', 300, 5, N'Hudut Kapısı GBT', N'Örnek sorgu', GETDATE(), N'System'),
(1, '2026-10-07', N'08:00 - 17:00', 95, 3, N'Trafik GBT', N'Örnek sorgu', GETDATE(), N'System');
