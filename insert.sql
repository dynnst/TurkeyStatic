SET IDENTITY_INSERT Units ON;
INSERT INTO Units (UnitId, UnitName, Description, IsActive, CreatedDate) VALUES (1, N'Suç Önleme', N'Suç Önleme Şubesi', 1, GETDATE());
INSERT INTO Units (UnitId, UnitName, Description, IsActive, CreatedDate) VALUES (2, N'İdari Büro', N'İdari İşler Bürosu', 1, GETDATE());
INSERT INTO Units (UnitId, UnitName, Description, IsActive, CreatedDate) VALUES (3, N'Pasaport Büro', N'Pasaport İşleri Bürosu', 1, GETDATE());
INSERT INTO Units (UnitId, UnitName, Description, IsActive, CreatedDate) VALUES (4, N'Trafik', N'Trafik Şubesi', 1, GETDATE());
SET IDENTITY_INSERT Units OFF;

INSERT INTO CrimeStatistics (UnitId, EntryDate, CrimeType, CrimeCode, CasesRequiringFollow_up, SuspectCount, ArrestedCount, JudicialControlCount, Notes, CreatedDate, CreatedBy)
VALUES (1, '2026-10-06', N'Hırsızlık', N'TCK141', 2, 3, 1, 0, N'Örnek kayıt 1', GETDATE(), N'System');

INSERT INTO CrimeStatistics (UnitId, EntryDate, CrimeType, CrimeCode, CasesRequiringFollow_up, SuspectCount, ArrestedCount, JudicialControlCount, Notes, CreatedDate, CreatedBy)
VALUES (1, '2026-10-07', N'Gasp', N'TCK148', 1, 2, 2, 0, N'Örnek kayıt 2', GETDATE(), N'System');

INSERT INTO CrimeStatistics (UnitId, EntryDate, CrimeType, CrimeCode, CasesRequiringFollow_up, SuspectCount, ArrestedCount, JudicialControlCount, Notes, CreatedDate, CreatedBy)
VALUES (1, '2026-10-07', N'Dolandırıcılık', N'TCK157', 5, 1, 0, 1, N'Örnek kayıt 3', GETDATE(), N'System');

INSERT INTO CrimeStatistics (UnitId, EntryDate, CrimeType, CrimeCode, CasesRequiringFollow_up, SuspectCount, ArrestedCount, JudicialControlCount, Notes, CreatedDate, CreatedBy)
VALUES (4, '2026-10-07', N'Trafik Güvenliğini Tehlikeye Sokma', N'TCK179', 0, 1, 0, 0, N'Örnek kayıt 4', GETDATE(), N'System');

INSERT INTO CrimeStatistics (UnitId, EntryDate, CrimeType, CrimeCode, CasesRequiringFollow_up, SuspectCount, ArrestedCount, JudicialControlCount, Notes, CreatedDate, CreatedBy)
VALUES (4, '2026-10-07', N'Taksirle Yaralama', N'TCK89', 1, 1, 0, 0, N'Örnek kayıt 5', GETDATE(), N'System');
