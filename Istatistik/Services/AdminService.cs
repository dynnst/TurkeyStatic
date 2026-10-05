using Istatistik.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Istatistik.Services
{
    public class AdminUserRow
    {
        public int FldId { get; set; }
        public string Sicil { get; set; }
        public string FullName { get; set; }
        public string Border { get; set; }
        public bool HasAssignment { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }
        public bool IsConfigSuperAdmin { get; set; }
        public List<int> BureauIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Yetki yönetimi. Kurallar:
    /// - SuperAdmin: tüm havalimanları; UnitAdmin ve SuperAdmin atayabilir.
    /// - UnitAdmin: yalnızca kendi havalimanı; BureauUser atar, büro yetkisi verir, büro ekler.
    /// </summary>
    public class AdminService
    {
        private readonly IstatistikContext _db;
        private readonly CurrentUser _actor;

        public AdminService(IstatistikContext db, CurrentUser actor)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _actor = actor ?? throw new UnauthorizedAccessException("Oturum bulunamadı.");

            if (!_actor.IsSuperAdmin && !_actor.IsUnitAdmin)
                throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");
        }

        /// <summary>
        /// UnitAdmin için havalimanı daima kendi havalimanıdır; istenen değer yok sayılır.
        /// </summary>
        public string ResolveBorder(string requested)
        {
            if (_actor.IsSuperAdmin)
                return string.IsNullOrWhiteSpace(requested) ? _actor.Border : requested.Trim();
            return _actor.Border;
        }

        public List<string> GetBorders()
        {
            if (!_actor.IsSuperAdmin)
                return new List<string> { _actor.Border };

            using (var ceza = new CezaContext())
            {
                var fromUsers = ceza.EUsers
                    .Where(u => u.fldBorder != null && u.fldBorder != "")
                    .Select(u => u.fldBorder)
                    .Distinct()
                    .ToList();
                return fromUsers.Select(b => b.Trim()).Distinct().OrderBy(b => b).ToList();
            }
        }

        public List<AdminUserRow> GetUsers(string border, string search)
        {
            border = ResolveBorder(border);
            if (string.IsNullOrWhiteSpace(border))
                throw new ArgumentException("Havalimanı seçilmedi.");

            List<EUser> eusers;
            using (var ceza = new CezaContext())
            {
                var q = ceza.EUsers.Where(u => u.fldBorder == border && u.fldActive == "1" && u.IsActive == true);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    q = q.Where(u => u.fldSicil.Contains(s) || u.fldName.Contains(s) || u.fldLastName.Contains(s));
                }
                eusers = q.OrderBy(u => u.fldName).Take(500).ToList();
            }

            var sicils = eusers.Select(u => u.fldSicil).ToList();
            var assignments = _db.UserAssignments.Where(a => sicils.Contains(a.Sicil)).ToList();
            var ids = assignments.Select(a => a.Id).ToList();
            var links = _db.UserBureaus.Where(ub => ids.Contains(ub.UserAssignmentId)).ToList();

            return eusers.Select(u =>
            {
                var a = assignments.FirstOrDefault(x => x.Sicil == u.fldSicil);
                return new AdminUserRow
                {
                    FldId = u.fldId,
                    Sicil = u.fldSicil,
                    FullName = ((u.fldName ?? "") + " " + (u.fldLastName ?? "")).Trim(),
                    Border = u.fldBorder,
                    HasAssignment = a != null,
                    IsActive = a != null && a.IsActive,
                    Role = a?.Role,
                    IsConfigSuperAdmin = AuthService.IsSuperAdminSicil(u.fldSicil),
                    BureauIds = a == null ? new List<int>() : links.Where(l => l.UserAssignmentId == a.Id).Select(l => l.BureauId).ToList()
                };
            }).ToList();
        }

        public List<UserAssignment> GetSuperAdmins()
        {
            if (!_actor.IsSuperAdmin)
                throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");
            return _db.UserAssignments.Where(a => a.Role == AppRoles.SuperAdmin && a.IsActive).OrderBy(a => a.Sicil).ToList();
        }

        public void SetRole(string sicil, string role, string border, bool isActive)
        {
            sicil = (sicil ?? "").Trim();
            if (string.IsNullOrEmpty(sicil)) throw new ArgumentException("Sicil boş olamaz.");

            if (role != AppRoles.SuperAdmin && role != AppRoles.UnitAdmin && role != AppRoles.BureauUser)
                throw new ArgumentException("Geçersiz rol.");

            if (role == AppRoles.SuperAdmin || role == AppRoles.UnitAdmin)
            {
                if (!_actor.IsSuperAdmin)
                    throw new UnauthorizedAccessException("Yönetici yetkisini yalnızca Sistem Yöneticisi verebilir.");
            }

            if (string.Equals(sicil, _actor.Sicil, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Kendi yetkinizi değiştiremezsiniz.");

            if (AuthService.IsSuperAdminSicil(sicil))
                throw new InvalidOperationException("Bu kullanıcı yapılandırma dosyasındaki Sistem Yöneticisi, buradan değiştirilemez.");

            string cezaBorder;
            using (var ceza = new CezaContext())
            {
                var e = ceza.EUsers.FirstOrDefault(u => u.fldSicil == sicil && u.fldActive == "1" && u.IsActive == true);
                if (e == null) throw new ArgumentException("Kullanıcı bulunamadı veya pasif.");
                cezaBorder = (e.fldBorder ?? "").Trim();
            }

            if (string.IsNullOrEmpty(cezaBorder))
                throw new InvalidOperationException("Kullanıcının havalimanı bilgisi tanımlı değil.");

            // UnitAdmin yalnızca kendi havalimanındaki kullanıcıları yönetebilir
            if (!_actor.IsSuperAdmin &&
                !string.Equals(cezaBorder, _actor.Border, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Başka bir havalimanındaki kullanıcıyı yönetemezsiniz.");

            var a = _db.UserAssignments.FirstOrDefault(x => x.Sicil == sicil);

            // UnitAdmin, bir yöneticinin yetkisini düşüremez
            if (a != null && !_actor.IsSuperAdmin && (a.Role == AppRoles.UnitAdmin || a.Role == AppRoles.SuperAdmin))
                throw new UnauthorizedAccessException("Yönetici yetkisini yalnızca Sistem Yöneticisi değiştirebilir.");

            if (a == null)
            {
                a = new UserAssignment
                {
                    Sicil = sicil,
                    CreatedBy = _actor.Sicil,
                    CreatedDate = DateTime.Now
                };
                _db.UserAssignments.Add(a);
            }

            a.Border = cezaBorder;
            a.Role = role;
            a.IsActive = isActive;
            _db.SaveChanges();
        }

        public void RemoveAssignment(string sicil)
        {
            var a = LoadManageable(sicil);
            if (AuthService.IsSuperAdminSicil(a.Sicil))
                throw new InvalidOperationException("Yapılandırmadaki Sistem Yöneticisi kaldırılamaz.");
            if (!_actor.IsSuperAdmin && a.Role != AppRoles.BureauUser)
                throw new UnauthorizedAccessException("Yönetici yetkisini yalnızca Sistem Yöneticisi kaldırabilir.");
            if (a.Role == AppRoles.SuperAdmin &&
                _db.UserAssignments.Count(x => x.Role == AppRoles.SuperAdmin && x.IsActive) <= 1 &&
                !(System.Configuration.ConfigurationManager.AppSettings["SuperAdminSicils"] ?? "").Trim().Any())
                throw new InvalidOperationException("Son Sistem Yöneticisi kaldırılamaz.");

            var links = _db.UserBureaus.Where(ub => ub.UserAssignmentId == a.Id).ToList();
            _db.UserBureaus.RemoveRange(links);
            _db.UserAssignments.Remove(a);
            _db.SaveChanges();
        }

        public void SetUserBureaus(string sicil, List<int> bureauIds)
        {
            var a = LoadManageable(sicil);
            bureauIds = (bureauIds ?? new List<int>()).Distinct().ToList();

            // Yalnızca kullanıcının havalimanındaki aktif bürolar seçilebilir
            var valid = _db.Bureaus
                .Where(b => b.Border == a.Border && b.IsActive && bureauIds.Contains(b.BureauId))
                .Select(b => b.BureauId)
                .ToList();

            if (valid.Count != bureauIds.Count)
                throw new ArgumentException("Geçersiz büro seçimi.");

            var existing = _db.UserBureaus.Where(ub => ub.UserAssignmentId == a.Id).ToList();
            _db.UserBureaus.RemoveRange(existing.Where(e => !valid.Contains(e.BureauId)));

            foreach (var id in valid.Where(v => !existing.Any(e => e.BureauId == v)))
                _db.UserBureaus.Add(new UserBureau { UserAssignmentId = a.Id, BureauId = id });

            _db.SaveChanges();
        }

        public List<Bureau> GetBureaus(string border)
        {
            border = ResolveBorder(border);
            return _db.Bureaus.Where(b => b.Border == border).OrderBy(b => b.Name).ToList();
        }

        public Bureau AddBureau(string border, string code, string name)
        {
            border = ResolveBorder(border);
            if (string.IsNullOrWhiteSpace(border)) throw new ArgumentException("Havalimanı seçilmedi.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Büro adı gereklidir.");

            name = name.Trim();
            code = string.IsNullOrWhiteSpace(code) ? MakeCode(name) : code.Trim().ToUpperInvariant();

            if (_db.Bureaus.Any(b => b.Border == border && b.Code == code))
                throw new InvalidOperationException("Bu büro kodu zaten mevcut.");

            var bureau = new Bureau { Border = border, Code = code, Name = name, IsActive = true };
            _db.Bureaus.Add(bureau);
            _db.SaveChanges();
            return bureau;
        }

        public void SetBureauActive(int bureauId, bool isActive)
        {
            var b = _db.Bureaus.FirstOrDefault(x => x.BureauId == bureauId);
            if (b == null) throw new ArgumentException("Büro bulunamadı.");
            if (!_actor.IsSuperAdmin && !string.Equals(b.Border, _actor.Border, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Başka bir havalimanının bürosunu değiştiremezsiniz.");

            b.IsActive = isActive;
            _db.SaveChanges();
        }

        /// <summary>
        /// Havalimanında hiç büro yoksa varsayılan büroları oluşturur.
        /// </summary>
        public void EnsureDefaultBureaus(string border)
        {
            BureauSeeder.EnsureForBorder(_db, ResolveBorder(border));
        }

        private UserAssignment LoadManageable(string sicil)
        {
            sicil = (sicil ?? "").Trim();
            var a = _db.UserAssignments.FirstOrDefault(x => x.Sicil == sicil);
            if (a == null) throw new ArgumentException("Kullanıcı için yetki kaydı bulunamadı.");

            if (!_actor.IsSuperAdmin && !string.Equals(a.Border, _actor.Border, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Başka bir havalimanındaki kullanıcıyı yönetemezsiniz.");

            if (string.Equals(a.Sicil, _actor.Sicil, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Kendi yetkinizi değiştiremezsiniz.");

            return a;
        }

        private static string MakeCode(string name)
        {
            var map = new Dictionary<char, char>
            {
                {'Ç','C'},{'Ğ','G'},{'İ','I'},{'I','I'},{'Ö','O'},{'Ş','S'},{'Ü','U'}
            };
            var up = name.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
            var chars = up.Select(c => map.ContainsKey(c) ? map[c] : c)
                .Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray();
            var code = new string(chars);
            while (code.Contains("__")) code = code.Replace("__", "_");
            code = code.Trim('_');
            return code.Length > 50 ? code.Substring(0, 50) : code;
        }
    }
}
